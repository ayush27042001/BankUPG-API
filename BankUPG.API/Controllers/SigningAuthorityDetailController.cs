using BankUPG.Application.Interfaces.SigningAuthorityDetail;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    /// <summary>
    /// Signing Authority Details Controller - Handles signing authority details onboarding step
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SigningAuthorityDetailController : ControllerBase
    {
        private readonly ISigningAuthorityDetailService _signingAuthorityDetailService;
        private readonly ILogger<SigningAuthorityDetailController> _logger;

        public SigningAuthorityDetailController(
            ISigningAuthorityDetailService signingAuthorityDetailService,
            ILogger<SigningAuthorityDetailController> logger)
        {
            _signingAuthorityDetailService = signingAuthorityDetailService;
            _logger = logger;
        }

        /// <summary>
        /// Get signing authority details for authenticated user
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<SigningAuthorityDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<SigningAuthorityDetailResponse>>> GetSigningAuthorityDetail()
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new ApiResponse<SigningAuthorityDetailResponse> { Success = false, Message = "Invalid user token" });

                var result = await _signingAuthorityDetailService.GetSigningAuthorityDetailAsync(userId.Value);

                if (result == null)
                    return NotFound(new ApiResponse<SigningAuthorityDetailResponse> { Success = false, Message = "Signing authority details not found. Please complete this step." });

                return Ok(new ApiResponse<SigningAuthorityDetailResponse> { Success = true, Message = "Signing authority details retrieved successfully", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving signing authority details for userId: {UserId}", GetUserId());
                return StatusCode(500, new ApiResponse<SigningAuthorityDetailResponse> { Success = false, Message = "An error occurred while retrieving signing authority details" });
            }
        }

        /// <summary>
        /// Get signing authority details by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<SigningAuthorityDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<SigningAuthorityDetailResponse>>> GetSigningAuthorityDetailById(int id)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new ApiResponse<SigningAuthorityDetailResponse> { Success = false, Message = "Invalid user token" });

                // Get the merchant for the current user
                var merchant = await _signingAuthorityDetailService.GetSigningAuthorityDetailAsync(userId.Value);
                
                if (merchant == null || merchant.Mid != id)
                    return NotFound(new ApiResponse<SigningAuthorityDetailResponse> { Success = false, Message = "Signing authority details not found." });

                return Ok(new ApiResponse<SigningAuthorityDetailResponse> { Success = true, Message = "Signing authority details retrieved successfully", Data = merchant });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving signing authority details for id: {Id}", id);
                return StatusCode(500, new ApiResponse<SigningAuthorityDetailResponse> { Success = false, Message = "An error occurred while retrieving signing authority details" });
            }
        }

        /// <summary>
        /// Save or update signing authority details for authenticated user
        /// </summary>
        [HttpPost("save")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<SigningAuthorityDetailSavedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<SigningAuthorityDetailSavedResponse>>> SaveSigningAuthorityDetail(
            [FromBody] SaveSigningAuthorityDetailRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<SigningAuthorityDetailSavedResponse>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new ApiResponse<SigningAuthorityDetailSavedResponse> { Success = false, Message = "Invalid user token" });

                var result = await _signingAuthorityDetailService.SaveSigningAuthorityDetailAsync(userId.Value, request);

                return Ok(new ApiResponse<SigningAuthorityDetailSavedResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning("User or merchant not found: {Message}", ex.Message);
                return Unauthorized(new ApiResponse<SigningAuthorityDetailSavedResponse> { Success = false, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<SigningAuthorityDetailSavedResponse> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving signing authority details for userId: {UserId}", GetUserId());
                return StatusCode(500, new ApiResponse<SigningAuthorityDetailSavedResponse> { Success = false, Message = "An error occurred while saving signing authority details" });
            }
        }

        private int? GetUserId()
        {
            var claim = User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _));
            return claim != null && int.TryParse(claim.Value, out int id) ? id : null;
        }
    }
}
