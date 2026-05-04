using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Models;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using BankUPG.Application.Services.Auth;
using BankUPG.Application.Interfaces.Cache;
using BankUPG.Application.Interfaces.Registration;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.Registration
{
    public class RegistrationService : IRegistrationService
    {
        private readonly AppDBContext _context;
        private readonly PasswordService _passwordService;
        private readonly OtpService _otpService;
        private readonly JwtService _jwtService;
        private readonly ICacheService _cache;
        private readonly ILogger<RegistrationService> _logger;
        private readonly AppSettings _appSettings;

        private const string RegistrationCachePrefix = "reg:";
        private const int MaxOtpAttempts = 3;
        private const int MaxResendAttempts = 3;

        public RegistrationService(
            AppDBContext context,
            PasswordService passwordService,
            OtpService otpService,
            JwtService jwtService,
            ICacheService cache,
            ILogger<RegistrationService> logger,
            AppSettings appSettings)
        {
            _context = context;
            _passwordService = passwordService;
            _otpService = otpService;
            _jwtService = jwtService;
            _cache = cache;
            _logger = logger;
            _appSettings = appSettings;
        }

        public async Task<(string registrationToken, string mobileNumber, int expirySeconds)> InitiateRegistrationAsync(
            InitiateRegistrationRequest request, string? ipAddress)
        {
            // Check if email already exists
            var existingUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
            {
                throw new InvalidOperationException("Email already registered");
            }

            // Check if mobile already exists
            var existingMobile = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.MobileNumber == request.MobileNumber);

            if (existingMobile != null)
            {
                throw new InvalidOperationException("Mobile number already registered");
            }

            // Generate registration token
            var registrationToken = GenerateRegistrationToken();

            // Hash password
            var (passwordHash, salt) = _passwordService.HashPassword(request.Password);

            // Store registration data in cache (temporary until OTP verified)
            var registrationData = new PendingRegistration
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                Salt = salt,
                MobileNumber = request.MobileNumber,
                CompanyWebsite = request.CompanyWebsite,
                BusinessName = request.BusinessName,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow,
                OtpAttempts = 0,
                ResendAttempts = 0
            };

            _cache.Set($"{RegistrationCachePrefix}{registrationToken}", registrationData, TimeSpan.FromMinutes(30));

            // Send OTP
            await _otpService.GenerateOtpAsync(request.MobileNumber, "REGISTRATION", null, null, ipAddress);

            _logger.LogInformation("Registration initiated for email: {Email}, mobile: {Mobile}", 
                request.Email, request.MobileNumber);

            return (registrationToken, request.MobileNumber, 300); // 5 minutes expiry
        }

        public async Task<OtpVerificationResponse> VerifyRegistrationOtpAsync(string mobileNumber, string otp, string registrationToken)
        {
            var cacheKey = $"{RegistrationCachePrefix}{registrationToken}";
            
            if (!_cache.TryGetValue(cacheKey, out PendingRegistration? registrationData) || registrationData == null)
            {
                return new OtpVerificationResponse
                {
                    IsVerified = false,
                    Message = "Registration session expired or invalid. Please start again."
                };
            }

            // Check max attempts
            if (registrationData.OtpAttempts >= MaxOtpAttempts)
            {
                _cache.Remove(cacheKey);
                return new OtpVerificationResponse
                {
                    IsVerified = false,
                    Message = "Maximum OTP attempts exceeded. Please start registration again."
                };
            }

            registrationData.OtpAttempts++;

            // Verify OTP
            var isValid = await _otpService.VerifyOtpAsync(mobileNumber, otp);

            if (!isValid)
            {
                _cache.Set(cacheKey, registrationData, TimeSpan.FromMinutes(30));
                var remainingAttempts = MaxOtpAttempts - registrationData.OtpAttempts;
                
                return new OtpVerificationResponse
                {
                    IsVerified = false,
                    Message = $"Invalid OTP. {remainingAttempts} attempts remaining.",
                    RegistrationToken = registrationToken,
                    RemainingAttempts = remainingAttempts > 0 ? remainingAttempts : null
                };
            }

            // OTP verified - create user and merchant immediately
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Create User
                var user = new User
                {
                    Email = registrationData.Email,
                    PasswordHash = registrationData.PasswordHash,
                    Salt = registrationData.Salt,
                    MobileNumber = registrationData.MobileNumber,
                    IsEmailVerified = false,
                    IsMobileVerified = true,
                    IsActive = true,
                    IsLocked = false,
                    FailedLoginAttempts = 0,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    PasswordLastChangedDate = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create Merchant
                var merchant = new Merchant
                {
                    UserId = user.UserId,
                    BusinessName = registrationData.BusinessName,
                    OnboardingStatusId = (int)OnboardingStatusEnum.AccountCreation,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.Merchants.Add(merchant);
                await _context.SaveChangesAsync();

                // Create Website/App Detail
                var websiteAppDetail = new WebsiteAppDetail
                {
                    Mid = merchant.Mid,
                    WebsiteAppUrl = registrationData.CompanyWebsite,
                    PaymentCollectionPreference = "WEBSITE",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.WebsiteAppDetails.Add(websiteAppDetail);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Store created user details in cache for PAN step
                registrationData.IsMobileVerified = true;
                registrationData.UserId = user.UserId;
                registrationData.Mid = merchant.Mid;
                _cache.Set(cacheKey, registrationData, TimeSpan.FromMinutes(30));

                // Generate JWT token
                var token = _jwtService.GenerateToken(
                    user.Email,
                    registrationData.BusinessName,
                    string.Empty,
                    user.UserId
                );

                _logger.LogInformation("OTP verified and user created for mobile: {Mobile}, UserId: {UserId}, MID: {Mid}", 
                    mobileNumber, user.UserId, merchant.Mid);

                return new OtpVerificationResponse
                {
                    IsVerified = true,
                    Message = "OTP verified successfully. Please complete your registration with PAN details.",
                    RegistrationToken = registrationToken,
                    UserId = user.UserId,
                    Mid = merchant.Mid,
                    UserName = registrationData.BusinessName,
                    Email = user.Email,
                    MobileNumber = user.MobileNumber,
                    CompanyWebsite = registrationData.CompanyWebsite,
                    Token = token,
                    TokenExpiration = DateTime.UtcNow.AddMinutes(_appSettings.Jwt.ExpirationMinutes),
                    FormStep = "SignupCompleteBusinessPANNeedcomplete",
                    Step = 1
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating user after OTP verification");
                throw;
            }
        }

        public async Task<RegistrationCompletedResponse> CompletePanRegistrationAsync(int userId, CompletePanRegistrationRequest request)
        {
            // Get user and merchant from JWT userId
            var merchant = await _context.Merchants
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null || merchant.User == null)
            {
                throw new InvalidOperationException("User or merchant not found. Please ensure you are logged in.");
            }

            var mid = merchant.Mid;

            // Validate PAN format
            if (!IsValidPanFormat(request.PanCardNumber))
            {
                throw new ArgumentException("Invalid PAN card format");
            }

            // Check if PAN already exists for ANOTHER merchant (not current one)
            var existingPanForOther = await _context.BusinessPandetails
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PancardNumber == request.PanCardNumber && p.Mid != mid);

            if (existingPanForOther != null)
            {
                throw new InvalidOperationException("PAN card already registered with another account");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Check if PAN details already exist for this merchant (Update case)
                var existingPanDetail = await _context.BusinessPandetails
                    .FirstOrDefaultAsync(p => p.Mid == mid);

                BusinessPandetail panDetail;
                bool isUpdate = existingPanDetail != null;

                if (isUpdate)
                {
                    // Update existing PAN details
                    existingPanDetail.PancardNumber = request.PanCardNumber.ToUpper();
                    existingPanDetail.NameOnPancard = request.NameOnPanCard;
                    existingPanDetail.DateOfBirthOrIncorporation = DateOnly.FromDateTime(request.DateOfBirthOrIncorporation);
                    existingPanDetail.PanverificationStatus = "COMPLETED";
                    existingPanDetail.UpdatedDate = DateTime.UtcNow;
                    panDetail = existingPanDetail;
                }
                else
                {
                    // Create new Business PAN Detail
                    panDetail = new BusinessPandetail
                    {
                        Mid = mid,
                        PancardNumber = request.PanCardNumber.ToUpper(),
                        NameOnPancard = request.NameOnPanCard,
                        DateOfBirthOrIncorporation = DateOnly.FromDateTime(request.DateOfBirthOrIncorporation),
                        PanverificationStatus = "COMPLETED",
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    };
                    _context.BusinessPandetails.Add(panDetail);
                }

                // Check if onboarding step tracking already exists
                var existingStepTracking = await _context.OnboardingStepTrackings
                    .FirstOrDefaultAsync(s => s.Mid == mid && s.StepName == "PAN Verification");

                if (existingStepTracking != null)
                {
                    // Update existing tracking
                    existingStepTracking.StepStatus = "COMPLETED";
                    existingStepTracking.CompletedDate = DateTime.UtcNow;
                    existingStepTracking.UpdatedDate = DateTime.UtcNow;
                }
                else
                {
                    // Create new Onboarding Step Tracking
                    var stepTracking = new OnboardingStepTracking
                    {
                        Mid = mid,
                        StepName = "PAN Verification",
                        StepStatus = "COMPLETED",
                        CompletedDate = DateTime.UtcNow,
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.OnboardingStepTrackings.Add(stepTracking);
                }

                // Update merchant onboarding status
                merchant.OnboardingStatusId = (int)OnboardingStatusEnum.PanVerification;
                merchant.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Generate JWT token
                var token = _jwtService.GenerateToken(
                    merchant.User.Email,
                    request.NameOnPanCard,
                    string.Empty,
                    merchant.User.UserId
                );

                _logger.LogInformation("PAN details {Operation} for user: {UserId}, merchant: {Mid}", 
                    isUpdate ? "updated" : "saved", userId, mid);

                return new RegistrationCompletedResponse
                {
                    UserId = merchant.User.UserId,
                    Mid = mid,
                    Email = merchant.User.Email,
                    Token = token,
                    TokenExpiration = DateTime.UtcNow.AddMinutes(_appSettings.Jwt.ExpirationMinutes),
                    Message = isUpdate ? "PAN details updated successfully" : "PAN details saved successfully",
                    UserName = request.NameOnPanCard,
                    FormStep = "BusinessPANCompletedBusinessEntityPending",
                    Step = 1,
                    OnboardingStatus = new OnboardingStatusDto
                    {
                        StepNumber = 2,
                        StepName = "PAN Verification",
                        IsCompleted = true,
                        Steps = new List<OnboardingStepDto>
                        {
                            new() { StepNumber = 0, StepName = "Account Creation", StepKey = "ACCOUNT_CREATION", IsCompleted = true, IsActive = false },
                            new() { StepNumber = 1, StepName = "PAN Verification", StepKey = "PAN_VERIFICATION", IsCompleted = true, IsActive = false },
                            new() { StepNumber = 2, StepName = "Business Entity", StepKey = "BUSINESS_ENTITY", IsCompleted = false, IsActive = true },
                            new() { StepNumber = 3, StepName = "Phone CKYC", StepKey = "PHONE_CKYC", IsCompleted = false, IsActive = false },
                            new() { StepNumber = 4, StepName = "Business Category", StepKey = "BUSINESS_CATEGORY", IsCompleted = false, IsActive = false },
                            new() { StepNumber = 5, StepName = "Share Business Details", StepKey = "SHARE_BUSINESS_DETAILS", IsCompleted = false, IsActive = false },
                            new() { StepNumber = 6, StepName = "Connect Platform", StepKey = "CONNECT_PLATFORM", IsCompleted = false, IsActive = false },
                            new() { StepNumber = 7, StepName = "Upload Documents", StepKey = "UPLOAD_DOCUMENTS", IsCompleted = false, IsActive = false },
                            new() { StepNumber = 8, StepName = "Service Agreement", StepKey = "SERVICE_AGREEMENT", IsCompleted = false, IsActive = false }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error completing PAN registration for userId: {UserId}", userId);
                throw;
            }
        }

        public async Task<ResendOtpResponse> ResendRegistrationOtpAsync(string mobileNumber, string registrationToken)
        {
            var cacheKey = $"{RegistrationCachePrefix}{registrationToken}";
            
            if (!_cache.TryGetValue(cacheKey, out PendingRegistration? registrationData) || registrationData == null)
            {
                return new ResendOtpResponse
                {
                    Success = false,
                    Message = "Registration session expired. Please start again."
                };
            }

            if (registrationData.ResendAttempts >= MaxResendAttempts)
            {
                return new ResendOtpResponse
                {
                    Success = false,
                    Message = "Maximum resend attempts exceeded. Please start registration again."
                };
            }

            // Check cooldown (60 seconds)
            var timeSinceLastOtp = DateTime.UtcNow - registrationData.LastOtpSentAt;
            if (timeSinceLastOtp.TotalSeconds < 60)
            {
                var remainingSeconds = 60 - (int)timeSinceLastOtp.TotalSeconds;
                return new ResendOtpResponse
                {
                    Success = false,
                    Message = $"Please wait {remainingSeconds} seconds before requesting another OTP",
                    RemainingSeconds = remainingSeconds
                };
            }

            registrationData.ResendAttempts++;
            registrationData.LastOtpSentAt = DateTime.UtcNow;

            // Send new OTP
            await _otpService.GenerateOtpAsync(mobileNumber, "REGISTRATION", null, null, registrationData.IpAddress);

            _cache.Set(cacheKey, registrationData, TimeSpan.FromMinutes(30));

            _logger.LogInformation("OTP resent for mobile: {Mobile}, attempt: {Attempt}", 
                mobileNumber, registrationData.ResendAttempts);

            return new ResendOtpResponse
            {
                Success = true,
                Message = "OTP sent successfully",
                RemainingSeconds = 60,
                MaxResendAttempts = MaxResendAttempts,
                RemainingResendAttempts = MaxResendAttempts - registrationData.ResendAttempts
            };
        }

        public async Task<PanDetailsResponse?> GetPanDetailsAsync(int userId)
        {
            // Get merchant for the user
            var merchant = await _context.Merchants
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null)
            {
                _logger.LogWarning("Merchant not found for userId: {UserId}", userId);
                return null;
            }

            // Get PAN details for the merchant
            var panDetails = await _context.BusinessPandetails
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Mid == merchant.Mid);

            if (panDetails == null)
            {
                _logger.LogInformation("PAN details not found for merchant: {Mid}", merchant.Mid);
                return null;
            }

            return new PanDetailsResponse
            {
                PanCardNumber = panDetails.PancardNumber,
                NameOnPanCard = panDetails.NameOnPancard,
                DateOfBirthOrIncorporation = panDetails.DateOfBirthOrIncorporation.ToDateTime(TimeOnly.MinValue),
                VerificationStatus = panDetails.PanverificationStatus,
                MerchantId = merchant.Mid
            };
        }

        private static string GenerateRegistrationToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private static bool IsValidPanFormat(string pan)
        {
            if (string.IsNullOrEmpty(pan) || pan.Length != 10)
                return false;

            // PAN format: AAAAA9999A (5 letters, 4 digits, 1 letter)
            for (int i = 0; i < 5; i++)
                if (!char.IsLetter(pan[i])) return false;

            for (int i = 5; i < 9; i++)
                if (!char.IsDigit(pan[i])) return false;

            return char.IsLetter(pan[9]);
        }
    }

    public class PendingRegistration
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string CompanyWebsite { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsMobileVerified { get; set; }
        public int OtpAttempts { get; set; }
        public int ResendAttempts { get; set; }
        public DateTime LastOtpSentAt { get; set; }
        public int? UserId { get; set; }
        public int? Mid { get; set; }
    }
}
