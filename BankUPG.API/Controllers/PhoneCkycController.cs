using BankUPG.Application.Interfaces.PhoneCkyc;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    /// <summary>
    /// Phone CKYC Controller - Handles Phone CKYC step of merchant onboarding
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PhoneCkycController : ControllerBase
    {
        private readonly IPhoneCkycService _phoneCkycService;
        private readonly ILogger<PhoneCkycController> _logger;

        public PhoneCkycController(
            IPhoneCkycService phoneCkycService,
            ILogger<PhoneCkycController> logger)
        {
            _phoneCkycService = phoneCkycService;
            _logger = logger;
        }

        /// <summary>
        /// Get Phone CKYC details for authenticated user (includes mobile number)
        /// </summary>
        /// <returns>Mobile number and current CKYC identifier/consent status</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PhoneCkycResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PhoneCkycResponse>>> GetPhoneCkyc()
        {
            try
            {
                var userIdClaim = User.FindAll(ClaimTypes.NameIdentifier)
                    .FirstOrDefault(c => int.TryParse(c.Value, out _));
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new ApiResponse<PhoneCkycResponse>
                    {
                        Success = false,
                        Message = "Invalid user token"
                    });
                }

                var result = await _phoneCkycService.GetPhoneCkycAsync(userId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<PhoneCkycResponse>
                    {
                        Success = false,
                        Message = "Phone CKYC details not found. Please complete this step."
                    });
                }

                return Ok(new ApiResponse<PhoneCkycResponse>
                {
                    Success = true,
                    Message = "Phone CKYC details retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Phone CKYC for userId: {UserId}",
                    User.FindAll(ClaimTypes.NameIdentifier).FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value);
                return StatusCode(500, new ApiResponse<PhoneCkycResponse>
                {
                    Success = false,
                    Message = "An error occurred while retrieving Phone CKYC details"
                });
            }
        }

        /// <summary>
        /// Send OTP to registered mobile number for Phone CKYC verification
        /// </summary>
        /// <returns>OTP expiry time</returns>
        [HttpPost("send-otp")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<OtpResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ApiResponse<OtpResponse>>> SendOtp()
        {
            try
            {
                var userIdClaim = User.FindAll(ClaimTypes.NameIdentifier)
                    .FirstOrDefault(c => int.TryParse(c.Value, out _));
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new ApiResponse<OtpResponse>
                    {
                        Success = false,
                        Message = "Invalid user token"
                    });
                }

                var result = await _phoneCkycService.SendOtpAsync(userId);

                return Ok(new ApiResponse<OtpResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Send OTP failed: {Message}", ex.Message);
                return BadRequest(new ApiResponse<OtpResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP for Phone CKYC userId: {UserId}",
                    User.FindAll(ClaimTypes.NameIdentifier).FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value);
                return StatusCode(500, new ApiResponse<OtpResponse>
                {
                    Success = false,
                    Message = "An error occurred while sending OTP"
                });
            }
        }

        /// <summary>
        /// Verify OTP for Phone CKYC
        /// </summary>
        /// <param name="request">OTP code</param>
        /// <returns>Verification result</returns>
        [HttpPost("verify-otp")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<OtpVerificationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<OtpVerificationResponse>>> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<OtpVerificationResponse>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var userIdClaim = User.FindAll(ClaimTypes.NameIdentifier)
                    .FirstOrDefault(c => int.TryParse(c.Value, out _));
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new ApiResponse<OtpVerificationResponse>
                    {
                        Success = false,
                        Message = "Invalid user token"
                    });
                }

                var result = await _phoneCkycService.VerifyOtpAsync(userId, request.Otp);

                return Ok(new ApiResponse<OtpVerificationResponse>
                {
                    Success = result.IsVerified,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Verify OTP failed: {Message}", ex.Message);
                return BadRequest(new ApiResponse<OtpVerificationResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for Phone CKYC userId: {UserId}",
                    User.FindAll(ClaimTypes.NameIdentifier).FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value);
                return StatusCode(500, new ApiResponse<OtpVerificationResponse>
                {
                    Success = false,
                    Message = "An error occurred while verifying OTP"
                });
            }
        }

        /// <summary>
        /// Save or update Phone CKYC details for authenticated user
        /// </summary>
        /// <param name="request">CKYC identifier and consent flag</param>
        /// <returns>Saved response with updated onboarding status and new JWT token</returns>
        [HttpPost("save")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PhoneCkycSavedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<PhoneCkycSavedResponse>>> SavePhoneCkyc(
            [FromBody] SavePhoneCkycRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<PhoneCkycSavedResponse>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var userIdClaim = User.FindAll(ClaimTypes.NameIdentifier)
                    .FirstOrDefault(c => int.TryParse(c.Value, out _));
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new ApiResponse<PhoneCkycSavedResponse>
                    {
                        Success = false,
                        Message = "Invalid user token"
                    });
                }

                var result = await _phoneCkycService.SavePhoneCkycAsync(userId, request);

                return Ok(new ApiResponse<PhoneCkycSavedResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning("User or merchant not found: {Message}", ex.Message);
                return Unauthorized(new ApiResponse<PhoneCkycSavedResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<PhoneCkycSavedResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Phone CKYC for userId: {UserId}",
                    User.FindAll(ClaimTypes.NameIdentifier).FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value);
                return StatusCode(500, new ApiResponse<PhoneCkycSavedResponse>
                {
                    Success = false,
                    Message = "An error occurred while saving Phone CKYC details"
                });
            }
        }
    }
}
