using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace BankUPG.Application.Services.Auth
{
    public class OtpService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<OtpService> _logger;
        private readonly AppSettings _appSettings;
        private readonly IMemoryCache _cache;
        private const int OtpExpiryMinutes = 5;
        private const int OtpLength = 6;

        public OtpService(AppDBContext context, ILogger<OtpService> logger, AppSettings appSettings, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _appSettings = appSettings;
            _cache = cache;
        }

        public async Task<string> GenerateOtpAsync(string mobileNumber, string purpose, int? userId = null, int? mid = null, string? ipAddress = null)
        {
            try
            {
                // For registration flow (no user/merchant yet), use cache to store OTP
                if (userId == null && mid == null)
                {
                    // Invalidate previous OTPs for this mobile number and purpose
                    var cacheKey = $"otp:{mobileNumber}:{purpose}";
                    _cache.Remove(cacheKey);

                    // Generate new OTP
                    var otpCode = GenerateOtpCode();
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(OtpExpiryMinutes))
                        .SetSize(1); // Size in MB units
                    _cache.Set(cacheKey, otpCode, cacheOptions);

                    // Send OTP via SMS
                    var smsStatus = SendSms(mobileNumber, otpCode);

                    if (smsStatus == "-1")
                    {
                        _logger.LogError($"Failed to send OTP SMS to {mobileNumber}");
                    }
                    else
                    {
                        _logger.LogInformation($"OTP sent successfully to {mobileNumber}: {otpCode}");
                    }

                    return otpCode;
                }
                else
                {
                    // For existing users/merchants, use database to store OTP
                    var effectiveUserId = userId ?? -1; // -1 indicates pending registration
                    var effectiveMid = mid ?? -1;

                    // Invalidate previous OTPs for this mobile number and purpose
                    var previousOtps = await _context.Otpverifications
                        .Where(o => o.MobileNumber == mobileNumber && o.Otppurpose == purpose && o.IsUsed == false)
                        .ToListAsync();

                    foreach (var otp in previousOtps)
                    {
                        otp.IsUsed = true;
                    }

                    // Generate new OTP
                    var otpCode = GenerateOtpCode();
                    var otpVerification = new Otpverification
                    {
                        MobileNumber = mobileNumber,
                        Otpcode = otpCode,
                        Otppurpose = purpose,
                        OtpcreatedTime = DateTime.UtcNow,
                        OtpexpiryTime = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
                        IsUsed = false,
                        UserId = effectiveUserId,
                        Mid = effectiveMid,
                        Ipaddress = ipAddress
                    };

                    _context.Otpverifications.Add(otpVerification);
                    await _context.SaveChangesAsync();

                    // Send OTP via SMS
                    var smsStatus = SendSms(mobileNumber, otpCode);

                    if (smsStatus == "-1")
                    {
                        _logger.LogError($"Failed to send OTP SMS to {mobileNumber}");
                    }
                    else
                    {
                        _logger.LogInformation($"OTP sent successfully to {mobileNumber}: {otpCode}");
                    }

                    return otpCode;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OTP for {MobileNumber}", mobileNumber);
                throw;
            }
        }

        public async Task<bool> VerifyOtpAsync(string mobileNumber, string otpCode)
        {
            // First check cache for registration OTPs
            var cacheKey = $"otp:{mobileNumber}:REGISTRATION";
            if (_cache.TryGetValue(cacheKey, out string? cachedOtp))
            {
                if (cachedOtp == otpCode)
                {
                    _cache.Remove(cacheKey);
                    return true;
                }
            }

            // Check database for existing user OTPs
            var otpRecord = await _context.Otpverifications
                .Where(o => o.MobileNumber == mobileNumber && o.Otpcode == otpCode && o.IsUsed == false)
                .OrderByDescending(o => o.OtpcreatedTime)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
            {
                return false;
            }

            if (otpRecord.OtpexpiryTime < DateTime.UtcNow)
            {
                return false;
            }

            otpRecord.IsUsed = true;
            otpRecord.UsedTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int?> GetRemainingTimeAsync(string mobileNumber)
        {
            var latestOtp = await _context.Otpverifications
                .Where(o => o.MobileNumber == mobileNumber && o.IsUsed == false)
                .OrderByDescending(o => o.OtpcreatedTime)
                .FirstOrDefaultAsync();

            if (latestOtp == null)
            {
                return null;
            }

            var remainingSeconds = (int)(latestOtp.OtpexpiryTime - DateTime.UtcNow).TotalSeconds;
            return remainingSeconds > 0 ? remainingSeconds : 0;
        }

        private string GenerateOtpCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var number = BitConverter.ToUInt32(bytes, 0) % 1000000;
            return number.ToString("D6"); // 6-digit format
        }

        private string SendSms(string mobileNumber, string otp)
        {
            try
            {
                string message = $"{otp} is your OTP from BankU India to authenticate. Never share your OTP or account details with anyone.";
                string encodedMessage = Uri.EscapeDataString(message);
                string strUrl = $"{_appSettings.Sms.ApiUrl}?username={_appSettings.Sms.Username}&apikey={_appSettings.Sms.ApiKey}&apirequest=Text&sender={_appSettings.Sms.Sender}&mobile={mobileNumber}&message={encodedMessage}&route={_appSettings.Sms.Route}&TemplateID={_appSettings.Sms.TemplateId}&format={_appSettings.Sms.Format}";

                using var httpClient = new HttpClient();
                var response = httpClient.GetAsync(strUrl).Result;
                var content = response.Content.ReadAsStringAsync().Result;

                _logger.LogInformation($"SMS API response for {mobileNumber}: {content}");
                return "1";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending SMS to {mobileNumber}");
                return "-1";
            }
        }
    }
}
