using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.API.Services;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly PasswordService _passwordService;
        private readonly OtpService _otpService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            AppDBContext context,
            JwtService jwtService,
            PasswordService passwordService,
            OtpService otpService,
            ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordService = passwordService;
            _otpService = otpService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                //email: ayush@gmail.com //password: Ayush@123
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                //Islocked: false, isActive: true

                if (user == null)
                {
                    return Unauthorized(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    });
                }

                if (user.IsLocked == true) //wring
                {
                    return Unauthorized(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Account is locked due to multiple failed login attempts. Please contact support."
                    });
                }

                if (user.IsActive == false)//wrong
                {
                    return Unauthorized(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Account is not active. Please contact support."
                    });
                }

                if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash, user.Salt))
                {
                    user.FailedLoginAttempts = (user.FailedLoginAttempts ?? 0) + 1;

                    if (user.FailedLoginAttempts >= 5)
                    {
                        user.IsLocked = true;
                        _logger.LogWarning($"User {user.Email} locked after {user.FailedLoginAttempts} failed attempts");
                    }

                    await _context.SaveChangesAsync();

                    return Unauthorized(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = user.IsLocked == true 
                            ? "Account locked due to multiple failed login attempts. Please contact support."
                            : "Invalid email or password"
                    });
                }

                // Reset failed attempts on successful login
                user.FailedLoginAttempts = 0;
                user.LastLoginDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Log successful login
                var merchant = user.Merchants.FirstOrDefault();
                var mid = merchant?.Mid;
                await LogLoginAttempt(user.UserId, mid, true, GetClientIpAddress());

                // Generate JWT token
                var token = _jwtService.GenerateToken(
                    user.Email,
                    user.Email, // Using email as first name for now
                    string.Empty, // Last name
                    user.UserId
                );

                var response = new LoginResponse
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddMinutes(60),
                    Email = user.Email,
                    MobileNumber = user.MobileNumber,
                    IsMobileVerified = user.IsMobileVerified ?? false,
                    FirstName = user.Email, // Using email as first name for now
                    LastName = string.Empty
                };

                return Ok(new ApiResponse<LoginResponse>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        [HttpPost("send-otp")]
        public async Task<ActionResult<ApiResponse<OtpResponse>>> SendOtp([FromBody] SendOtpRequest request)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.MobileNumber == request.MobileNumber);

                if (user == null)
                {
                    return BadRequest(new ApiResponse<OtpResponse>
                    {
                        Success = false,
                        Message = "Mobile number not registered"
                    });
                }

                // Check if there's a recent OTP (within 60 seconds)
                var recentOtp = await _context.Otpverifications
                    .Where(o => o.MobileNumber == request.MobileNumber 
                        && o.Otppurpose == request.Purpose 
                        && o.IsUsed == false
                        && o.OtpcreatedTime > DateTime.UtcNow.AddSeconds(-60))
                    .OrderByDescending(o => o.OtpcreatedTime)
                    .FirstOrDefaultAsync();

                if (recentOtp != null)
                {
                    var remainingSeconds = await _otpService.GetRemainingTimeAsync(request.MobileNumber);
                    return Ok(new ApiResponse<OtpResponse>
                    {
                        Success = false,
                        Message = "Please wait before requesting another OTP",
                        Data = new OtpResponse
                        {
                            Success = false,
                            Message = "Please wait before requesting another OTP",
                            RemainingSeconds = remainingSeconds
                        }
                    });
                }

                var merchant = user.Merchants.FirstOrDefault();
                var mid = merchant?.Mid;
                var otpCode = await _otpService.GenerateOtpAsync(
                    request.MobileNumber,
                    request.Purpose,
                    user.UserId,
                    mid,
                    GetClientIpAddress()
                );

                _logger.LogInformation($"OTP sent to {request.MobileNumber}: {otpCode}");

                return Ok(new ApiResponse<OtpResponse>
                {
                    Success = true,
                    Message = "OTP sent successfully",
                    Data = new OtpResponse
                    {
                        Success = true,
                        Message = "OTP sent successfully",
                        RemainingSeconds = 300 // 5 minutes
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP");
                return StatusCode(500, new ApiResponse<OtpResponse>
                {
                    Success = false,
                    Message = "An error occurred while sending OTP"
                });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            try
            {
                var isValid = await _otpService.VerifyOtpAsync(request.MobileNumber, request.Otp);

                if (!isValid)
                {
                    return BadRequest(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Invalid or expired OTP"
                    });
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.MobileNumber == request.MobileNumber);

                if (user == null)
                {
                    return BadRequest(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                // Mark mobile as verified
                user.IsMobileVerified = true;
                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = _jwtService.GenerateToken(
                    user.Email,
                    user.Email,
                    string.Empty,
                    user.UserId
                );

                var response = new LoginResponse
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddMinutes(60),
                    Email = user.Email,
                    MobileNumber = user.MobileNumber,
                    IsMobileVerified = true,
                    FirstName = user.Email,
                    LastName = string.Empty
                };

                return Ok(new ApiResponse<LoginResponse>
                {
                    Success = true,
                    Message = "OTP verified successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP");
                return StatusCode(500, new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "An error occurred while verifying OTP"
                });
            }
        }

        private async Task LogLoginAttempt(int userId, int? mid, bool success, string? ipAddress)
        {
            var auditLog = new LoginAuditLog
            {
                UserId = userId,
                Mid = mid,
                LoginStatus = success ? "Success" : "Failed",
                LoginIpaddress = ipAddress,
                LoginAttemptDate = DateTime.UtcNow,
                UserAgent = Request.Headers["User-Agent"].ToString()
            };

            _context.LoginAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        private string? GetClientIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                var forwardedFor = Request.Headers["X-Forwarded-For"].ToString();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    ipAddress = forwardedFor.Split(',')[0].Trim();
                }
            }

            return ipAddress;
        }
    }
}
