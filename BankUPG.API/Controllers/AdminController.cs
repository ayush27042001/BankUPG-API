using BankUPG.Application.Services.Auth;
using BankUPG.Application.Interfaces.Admin;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Models;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly PasswordService _passwordService;
        private readonly OtpService _otpService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AdminController> _logger;
        private readonly AppSettings _appSettings;
        private readonly IAdminService _adminService;

        private const string AdminOtpPurpose = "ADMIN_LOGIN";

        public AdminController(
            AppDBContext context,
            JwtService jwtService,
            PasswordService passwordService,
            OtpService otpService,
            IMemoryCache cache,
            ILogger<AdminController> logger,
            AppSettings appSettings,
            IAdminService adminService)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordService = passwordService;
            _otpService = otpService;
            _cache = cache;
            _logger = logger;
            _appSettings = appSettings;
            _adminService = adminService;
        }

        /// <summary>
        /// Step 1 – Validate credentials and dispatch a 6-digit OTP.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> Login([FromBody] SuperAdminLoginRequest request)
        {
            try
            {
                var admin = await _context.SuperAdmins
                    .FirstOrDefaultAsync(a => a.Username == request.Username);

                if (admin == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid username or password."
                    });
                }

                if (admin.IsLocked)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Account is locked due to multiple failed login attempts. Please contact support."
                    });
                }

                if (!admin.IsActive)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Account is not active. Please contact support."
                    });
                }

                if (!_passwordService.VerifyPassword(request.Password, admin.PasswordHash, admin.Salt))
                {
                    admin.FailedLoginAttempts += 1;

                    if (admin.FailedLoginAttempts >= 5)
                    {
                        admin.IsLocked = true;
                        _logger.LogWarning("SuperAdmin {Username} locked after {Attempts} failed attempts.",
                            admin.Username, admin.FailedLoginAttempts);
                    }

                    await _context.SaveChangesAsync();

                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = admin.IsLocked
                            ? "Account locked due to multiple failed login attempts. Please contact support."
                            : "Invalid username or password."
                    });
                }

                // Reset failed attempts
                admin.FailedLoginAttempts = 0;
                await _context.SaveChangesAsync();

                if (string.IsNullOrEmpty(admin.MobileNumber))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No mobile number configured for this admin account. Please contact support."
                    });
                }

                // Use the same OtpService used by AuthController – generates OTP,
                // stores it in cache as otp:{mobile}:ADMIN_LOGIN, and sends SMS.
                await _otpService.GenerateOtpAsync(
                    admin.MobileNumber,
                    AdminOtpPurpose,
                    null,
                    null,
                    GetClientIpAddress());

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "OTP sent to registered mobile number. Valid for 5 minutes."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during SuperAdmin login.");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred during login."
                });
            }
        }

        /// <summary>
        /// Step 2 – Verify OTP and return JWT + refresh token.
        /// </summary>
        [HttpPost("verify-otp")]
        [ProducesResponseType(typeof(ApiResponse<AdminVerifyOtpData>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AdminVerifyOtpData>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AdminVerifyOtpData>>> VerifyOtp([FromBody] SuperAdminVerifyOtpRequest request)
        {
            try
            {
                var admin = await _context.SuperAdmins
                    .FirstOrDefaultAsync(a => a.Username == request.Username);

                if (admin == null)
                {
                    return BadRequest(new ApiResponse<AdminVerifyOtpData>
                    {
                        Success = false,
                        Message = "Admin not found."
                    });
                }

                // OtpService stores the OTP in cache with key: otp:{mobile}:{purpose}
                // (same key format used for merchant registration OTPs)
                var cacheKey = $"otp:{admin.MobileNumber}:{AdminOtpPurpose}";

                if (!_cache.TryGetValue(cacheKey, out string? cachedOtp) || cachedOtp != request.Otp)
                {
                    return BadRequest(new ApiResponse<AdminVerifyOtpData>
                    {
                        Success = false,
                        Message = "Invalid or expired OTP."
                    });
                }

                // Invalidate OTP immediately after successful verification
                _cache.Remove(cacheKey);

                // Update last login date
                admin.LastLoginDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Generate JWT and refresh token
                var token = _jwtService.GenerateAdminToken(
                    admin.Username,
                    admin.Email,
                    admin.AdminId,
                    admin.Role);

                var refreshToken = _jwtService.GenerateRefreshToken();
                var refreshTokenExpiration = _jwtService.GetRefreshTokenExpiration();

                // Revoke any existing active refresh tokens for this admin
                var existingTokens = await _context.SuperAdminRefreshTokens
                    .Where(r => r.AdminId == admin.AdminId && !r.IsRevoked)
                    .ToListAsync();

                foreach (var existing in existingTokens)
                {
                    existing.IsRevoked = true;
                    existing.RevokedAt = DateTime.UtcNow;
                }

                // Persist new refresh token
                _context.SuperAdminRefreshTokens.Add(new SuperAdminRefreshToken
                {
                    AdminId = admin.AdminId,
                    Token = refreshToken,
                    ExpiresAt = refreshTokenExpiration,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = Request.Headers["User-Agent"].ToString()
                });

                await _context.SaveChangesAsync();

                var expiration = DateTime.UtcNow.AddMinutes(_appSettings.Jwt.ExpirationMinutes);

                return Ok(new ApiResponse<AdminVerifyOtpData>
                {
                    Success = true,
                    Message = "OTP verified successfully.",
                    Data = new AdminVerifyOtpData
                    {
                        Token = token,
                        RefreshToken = refreshToken,
                        Expiration = expiration,
                        RefreshTokenExpiration = refreshTokenExpiration,
                        Username = admin.Username,
                        Role = admin.Role
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during SuperAdmin OTP verification.");
                return StatusCode(500, new ApiResponse<AdminVerifyOtpData>
                {
                    Success = false,
                    Message = "An error occurred while verifying OTP."
                });
            }
        }

        /// <summary>
        /// Refresh an expired JWT using a valid refresh token.
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<AdminRefreshTokenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AdminRefreshTokenResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<AdminRefreshTokenResponse>>> RefreshToken(
            [FromBody] SuperAdminRefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(new ApiResponse<AdminRefreshTokenResponse>
                    {
                        Success = false,
                        Message = "Refresh token is required."
                    });
                }

                var existingToken = await _context.SuperAdminRefreshTokens
                    .Include(r => r.Admin)
                    .FirstOrDefaultAsync(r => r.Token == request.RefreshToken);

                if (existingToken == null || !existingToken.IsActive)
                {
                    return Unauthorized(new ApiResponse<AdminRefreshTokenResponse>
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token."
                    });
                }

                var admin = existingToken.Admin;

                // Revoke old refresh token
                existingToken.IsRevoked = true;
                existingToken.RevokedAt = DateTime.UtcNow;

                // Generate new tokens
                var newToken = _jwtService.GenerateAdminToken(
                    admin.Username,
                    admin.Email,
                    admin.AdminId,
                    admin.Role);

                var newRefreshToken = _jwtService.GenerateRefreshToken();
                var newRefreshTokenExpiration = _jwtService.GetRefreshTokenExpiration();

                _context.SuperAdminRefreshTokens.Add(new SuperAdminRefreshToken
                {
                    AdminId = admin.AdminId,
                    Token = newRefreshToken,
                    ExpiresAt = newRefreshTokenExpiration,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = Request.Headers["User-Agent"].ToString()
                });

                await _context.SaveChangesAsync();

                var expiration = DateTime.UtcNow.AddMinutes(_appSettings.Jwt.ExpirationMinutes);

                return Ok(new ApiResponse<AdminRefreshTokenResponse>
                {
                    Success = true,
                    Message = "Token refreshed successfully.",
                    Data = new AdminRefreshTokenResponse
                    {
                        Token = newToken,
                        Expiration = expiration,
                        RefreshToken = newRefreshToken,
                        RefreshTokenExpiration = newRefreshTokenExpiration,
                        Username = admin.Username,
                        Role = admin.Role
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during SuperAdmin token refresh.");
                return StatusCode(500, new ApiResponse<AdminRefreshTokenResponse>
                {
                    Success = false,
                    Message = "An error occurred while refreshing the token."
                });
            }
        }

        /// <summary>
        /// Get complete user/merchant data along with uploaded documents.
        /// Expects UserId and BehalfOfUserId query parameters.
        /// </summary>
        [HttpGet("user-detail")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserCompleteDataResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserCompleteDataResponse>>> GetUserCompleteData([FromQuery] UserDetailRequest request)
        {
            try
            {
                if (request.UserId <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "UserId is required and must be greater than 0."
                    });
                }

                if (request.BehalfOfUserId <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "BehalfOfUserId is required and must be greater than 0."
                    });
                }

                var data = await _adminService.GetUserCompleteDataAsync(request);

                if (data == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                return Ok(new ApiResponse<UserCompleteDataResponse>
                {
                    Success = true,
                    Message = "User complete data retrieved successfully.",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving complete user data for UserId: {UserId}", request.UserId);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user data."
                });
            }
        }

        private string? GetClientIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                var forwardedFor = Request.Headers["X-Forwarded-For"].ToString();
                if (!string.IsNullOrEmpty(forwardedFor))
                    return forwardedFor.Split(',')[0].Trim();
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}
