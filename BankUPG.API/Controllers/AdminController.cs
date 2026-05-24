using BankUPG.Application.Services.Auth;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Models;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly PasswordService _passwordService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AdminController> _logger;
        private readonly AppSettings _appSettings;

        private const int OtpExpiryMinutes = 5;
        private const string OtpCacheKeyPrefix = "admin_otp:";

        public AdminController(
            AppDBContext context,
            JwtService jwtService,
            PasswordService passwordService,
            IMemoryCache cache,
            ILogger<AdminController> logger,
            AppSettings appSettings)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordService = passwordService;
            _cache = cache;
            _logger = logger;
            _appSettings = appSettings;
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

                // Generate 6-digit OTP and store in memory cache
                var otpCode = GenerateOtpCode();
                var cacheKey = $"{OtpCacheKeyPrefix}{admin.Username}";

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(OtpExpiryMinutes))
                    .SetSize(1);

                _cache.Set(cacheKey, otpCode, cacheOptions);

                // TODO: Send OTP via email to admin.Email
                // For now the OTP is logged – replace with your email service
                _logger.LogInformation("SuperAdmin OTP for {Username}: {Otp}", admin.Username, otpCode);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"OTP sent to registered email. Valid for {OtpExpiryMinutes} minutes."
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
                var cacheKey = $"{OtpCacheKeyPrefix}{request.Username}";

                if (!_cache.TryGetValue(cacheKey, out string? cachedOtp) || cachedOtp != request.Otp)
                {
                    return BadRequest(new ApiResponse<AdminVerifyOtpData>
                    {
                        Success = false,
                        Message = "Invalid or expired OTP."
                    });
                }

                // Invalidate the OTP immediately after successful verification
                _cache.Remove(cacheKey);

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

        private static string GenerateOtpCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var number = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
            return number.ToString("D6");
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
