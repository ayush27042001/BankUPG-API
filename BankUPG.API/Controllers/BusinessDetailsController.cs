using BankUPG.Application.Interfaces.BusinessDetails;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    /// <summary>
    /// Business Details Controller - Handles share business details onboarding step
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BusinessDetailsController : ControllerBase
    {
        private readonly IBusinessDetailsService _businessDetailsService;
        private readonly ILogger<BusinessDetailsController> _logger;

        public BusinessDetailsController(
            IBusinessDetailsService businessDetailsService,
            ILogger<BusinessDetailsController> logger)
        {
            _businessDetailsService = businessDetailsService;
            _logger = logger;
        }

        /// <summary>
        /// Get business details for authenticated user
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<BusinessDetailsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BusinessDetailsResponse>>> GetBusinessDetails()
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new ApiResponse<BusinessDetailsResponse> { Success = false, Message = "Invalid user token" });

                var result = await _businessDetailsService.GetBusinessDetailsAsync(userId.Value);

                if (result == null)
                    return NotFound(new ApiResponse<BusinessDetailsResponse> { Success = false, Message = "Business details not found. Please complete this step." });

                return Ok(new ApiResponse<BusinessDetailsResponse> { Success = true, Message = "Business details retrieved successfully", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving business details for userId: {UserId}", GetUserId());
                return StatusCode(500, new ApiResponse<BusinessDetailsResponse> { Success = false, Message = "An error occurred while retrieving business details" });
            }
        }

        /// <summary>
        /// Save or update business details (expected sales, GSTIN) for authenticated user
        /// </summary>
        [HttpPost("save")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<BusinessDetailsSavedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<BusinessDetailsSavedResponse>>> SaveBusinessDetails(
            [FromBody] SaveBusinessDetailsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<BusinessDetailsSavedResponse>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new ApiResponse<BusinessDetailsSavedResponse> { Success = false, Message = "Invalid user token" });

                var result = await _businessDetailsService.SaveBusinessDetailsAsync(userId.Value, request);

                return Ok(new ApiResponse<BusinessDetailsSavedResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning("User or merchant not found: {Message}", ex.Message);
                return Unauthorized(new ApiResponse<BusinessDetailsSavedResponse> { Success = false, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<BusinessDetailsSavedResponse> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving business details for userId: {UserId}", GetUserId());
                return StatusCode(500, new ApiResponse<BusinessDetailsSavedResponse> { Success = false, Message = "An error occurred while saving business details" });
            }
        }

        /// <summary>
        /// Validate a GSTIN number via Cashfree verification API
        /// </summary>
        [HttpPost("validate-gst")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<GstVerifyResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<GstVerifyResult>>> ValidateGst(
            [FromBody] ValidateGstRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<GstVerifyResult>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new ApiResponse<GstVerifyResult> { Success = false, Message = "Invalid user token" });

                var result = await _businessDetailsService.VerifyGstAsync(request.Gstin, request.BusinessName);

                return Ok(new ApiResponse<GstVerifyResult>
                {
                    Success = true,
                    Message = result.IsValid ? "GSTIN is valid" : "GSTIN verification failed",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating GSTIN: {Gstin}", request.Gstin);
                return StatusCode(500, new ApiResponse<GstVerifyResult> { Success = false, Message = "An error occurred while validating GSTIN" });
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
