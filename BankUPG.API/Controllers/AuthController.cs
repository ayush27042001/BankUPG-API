using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.Application.Services.Auth;
using BankUPG.Application.Interfaces.Cache;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using BankUPG.SharedKernal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        //test cicd
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly PasswordService _passwordService;
        private readonly OtpService _otpService;
        private readonly ILogger<AuthController> _logger;
        private readonly AppSettings _appSettings;

        public AuthController(
            AppDBContext context,
            JwtService jwtService,
            PasswordService passwordService,
            OtpService otpService,
            ILogger<AuthController> logger,
            AppSettings appSettings)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordService = passwordService;
            _otpService = otpService;
            _logger = logger;
            _appSettings = appSettings;
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

                // Get merchant with onboarding status
                var merchant = await _context.Merchants
                    .Include(m => m.OnboardingStatus)
                    .FirstOrDefaultAsync(m => m.UserId == user.UserId);
                var mid = merchant?.Mid;
                await LogLoginAttempt(user.UserId, mid, true, GetClientIpAddress());

                // Generate JWT token
                var token = _jwtService.GenerateToken(
                    user.Email,
                    user.Email, // Using email as first name for now
                    string.Empty, // Last name
                    user.UserId
                );

                // Generate refresh token
                var refreshToken = _jwtService.GenerateRefreshToken();
                var refreshTokenExpiration = _jwtService.GetRefreshTokenExpiration();

                // Save refresh token to database
                _context.RefreshTokens.Add(new RefreshToken
                {
                    UserId = user.UserId,
                    Token = refreshToken,
                    ExpiresAt = refreshTokenExpiration,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = Request.Headers["User-Agent"].ToString()
                });
                await _context.SaveChangesAsync();

                // Build onboarding status for response from OnboardingStepTracking table
                var (currentStepName, formStep, step) = await GetOnboardingStepInfoAsync(mid ?? 0);
                var onboardingStatus = await BuildOnboardingStatusAsync(mid ?? 0);

                var response = new LoginResponse
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddMinutes(_appSettings.Jwt.ExpirationMinutes),
                    Email = user.Email,
                    MobileNumber = user.MobileNumber,
                    IsMobileVerified = user.IsMobileVerified ?? false,
                    FirstName = user.Email, // Using email as first name for now
                    LastName = string.Empty,
                    RefreshToken = refreshToken,
                    RefreshTokenExpiration = refreshTokenExpiration,
                    OnboardingStatusId = step, // 0-based step number
                    CurrentStepName = currentStepName,
                    FormStep = formStep,
                    Step = step,
                    OnboardingStatus = onboardingStatus
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

                // Get merchant for onboarding status
                var merchant = await _context.Merchants
                    .FirstOrDefaultAsync(m => m.UserId == user.UserId);
                var mid = merchant?.Mid;

                // Generate JWT token
                var token = _jwtService.GenerateToken(
                    user.Email,
                    user.Email,
                    string.Empty,
                    user.UserId
                );

                // Build onboarding status for response from OnboardingStepTracking table
                var (currentStepName, formStep, step) = await GetOnboardingStepInfoAsync(mid ?? 0);
                var onboardingStatus = await BuildOnboardingStatusAsync(mid ?? 0);

                var response = new LoginResponse
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddMinutes(_appSettings.Jwt.ExpirationMinutes),
                    Email = user.Email,
                    MobileNumber = user.MobileNumber,
                    IsMobileVerified = true,
                    FirstName = user.Email,
                    LastName = string.Empty,
                    OnboardingStatusId = step,
                    CurrentStepName = currentStepName,
                    FormStep = formStep,
                    Step = step,
                    OnboardingStatus = onboardingStatus
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

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(new ApiResponse<RefreshTokenResponse>
                    {
                        Success = false,
                        Message = "Refresh token is required"
                    });
                }

                // Find the refresh token in database
                var existingToken = await _context.RefreshTokens
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Token == request.RefreshToken);

                if (existingToken == null || !existingToken.IsActive)
                {
                    return Unauthorized(new ApiResponse<RefreshTokenResponse>
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token"
                    });
                }

                // If expired access token is provided, validate it matches the refresh token user
                if (!string.IsNullOrEmpty(request.ExpiredToken))
                {
                    var principal = _jwtService.ValidateExpiredToken(request.ExpiredToken);
                    if (principal != null)
                    {
                        var userIdClaim = principal.FindAll(ClaimTypes.NameIdentifier)
                            .FirstOrDefault(c => int.TryParse(c.Value, out _));
                        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int tokenUserId))
                        {
                            if (tokenUserId != existingToken.UserId)
                            {
                                return Unauthorized(new ApiResponse<RefreshTokenResponse>
                                {
                                    Success = false,
                                    Message = "Token mismatch"
                                });
                            }
                        }
                    }
                }

                var user = existingToken.User;

                // Revoke the old refresh token
                existingToken.IsRevoked = true;
                existingToken.RevokedAt = DateTime.UtcNow;

                // Generate new tokens
                var newToken = _jwtService.GenerateToken(
                    user.Email,
                    user.Email,
                    string.Empty,
                    user.UserId
                );

                var newRefreshToken = _jwtService.GenerateRefreshToken();
                var newRefreshTokenExpiration = _jwtService.GetRefreshTokenExpiration();

                // Save new refresh token
                _context.RefreshTokens.Add(new RefreshToken
                {
                    UserId = user.UserId,
                    Token = newRefreshToken,
                    ExpiresAt = newRefreshTokenExpiration,
                    ReplacedByToken = newRefreshToken,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = Request.Headers["User-Agent"].ToString()
                });

                await _context.SaveChangesAsync();

                var response = new RefreshTokenResponse
                {
                    Token = newToken,
                    Expiration = DateTime.UtcNow.AddMinutes(_appSettings.Jwt.ExpirationMinutes),
                    RefreshToken = newRefreshToken,
                    RefreshTokenExpiration = newRefreshTokenExpiration,
                    Email = user.Email
                };

                return Ok(new ApiResponse<RefreshTokenResponse>
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new ApiResponse<RefreshTokenResponse>
                {
                    Success = false,
                    Message = "An error occurred while refreshing token"
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

        private async Task<(string stepName, string formStep, int step)> GetOnboardingStepInfoAsync(int mid)
        {
            // Define the order of onboarding steps
            var stepOrder = new[]
            {
                
                "PAN Verification",
                "Business Entity",
                "Phone CKYC",
                "Business Category",
                "Share Business Details",
                "Connect Platform",
                "Upload Documents",
                "Service Agreement"
            };

            // Get completed steps from OnboardingStepTracking table
            var completedSteps = await _context.OnboardingStepTrackings
                .Where(s => s.Mid == mid && s.StepStatus == "COMPLETED")
                .Select(s => s.StepName)
                .ToListAsync();

            // Find the first incomplete step (current step)
            string currentStepName = "Account Creation";
            int stepIndex = 0;

            if (completedSteps.Count == 0)
            {
                stepIndex = 0;
            }
            else
            {
                bool foundIncomplete = false;
                foreach (var step in stepOrder)
                {
                    if (!completedSteps.Contains(step))
                    {
                        currentStepName = step;
                        foundIncomplete = true;
                        break;
                    }
                    stepIndex++;
                }
                // If all steps completed
                if (!foundIncomplete)
                {
                    currentStepName = "Completed";
                    stepIndex = 9;
                }
            }

            // Map step to form step string
            string formStep = currentStepName switch
            {
                
                "PAN Verification" => "BusinessPANCompletedBusinessEntityPending",
                "Business Entity" => "BusinessEntityCompletedPhoneCKYCPending",
                "Phone CKYC" => "PhoneCKYCCompletedBusinessCategoryPending",
                "Business Category" => "BusinessCategoryCompletedShareDetailsPending",
                "Share Business Details" => "ShareDetailsCompletedConnectPlatformPending",
                "Connect Platform" => "ConnectPlatformCompletedUploadDocsPending",
                "Upload Documents" => "UploadDocsCompletedServiceAgreementPending",
                "Service Agreement" => "ServiceAgreementCompleted",
                "Completed" => "RegistrationCompleted",
                _ => "SignupCompleteBusinessPANNeedcomplete"
            };

            return (currentStepName, formStep, stepIndex);
        }

        private async Task<OnboardingStatusDto> BuildOnboardingStatusAsync(int mid)
        {
            // Define the order of onboarding steps
            var stepOrder = new[]
            {
               
                new { StepNumber = 1, StepName = "PAN Verification", StepKey = "PAN_VERIFICATION" },
                new { StepNumber = 2, StepName = "Business Entity", StepKey = "BUSINESS_ENTITY" },
                new { StepNumber = 3, StepName = "Phone CKYC", StepKey = "PHONE_CKYC" },
                new { StepNumber = 4, StepName = "Business Category", StepKey = "BUSINESS_CATEGORY" },
                new { StepNumber = 5, StepName = "Share Business Details", StepKey = "SHARE_BUSINESS_DETAILS" },
                new { StepNumber = 6, StepName = "Connect Platform", StepKey = "CONNECT_PLATFORM" },
                new { StepNumber = 7, StepName = "Upload Documents", StepKey = "UPLOAD_DOCUMENTS" },
                new { StepNumber = 8, StepName = "Service Agreement", StepKey = "SERVICE_AGREEMENT" }
            };

            // Get completed steps from OnboardingStepTracking table
            var completedSteps = await _context.OnboardingStepTrackings
                .Where(s => s.Mid == mid && s.StepStatus == "COMPLETED")
                .Select(s => s.StepName)
                .ToListAsync();

            // Find the current (first incomplete) step
            int currentStepIndex = 0;
            string currentStepName = "Account Creation";
            bool allCompleted = true;

            foreach (var step in stepOrder)
            {
                if (!completedSteps.Contains(step.StepName))
                {
                    currentStepIndex = step.StepNumber;
                    currentStepName = step.StepName;
                    allCompleted = false;
                    break;
                }
            }

            if (allCompleted)
            {
                currentStepIndex = 9;
                currentStepName = "Completed";
            }

            // Build steps list with completion status
            var steps = stepOrder.Select(step => new OnboardingStepDto
            {
                StepNumber = step.StepNumber,
                StepName = step.StepName,
                StepKey = step.StepKey,
                IsCompleted = completedSteps.Contains(step.StepName),
                IsActive = step.StepName == currentStepName
            }).ToList();

            return new OnboardingStatusDto
            {
                StepNumber = currentStepIndex,
                StepName = currentStepName,
                IsCompleted = allCompleted,
                Steps = steps
            };
        }
    }
}
