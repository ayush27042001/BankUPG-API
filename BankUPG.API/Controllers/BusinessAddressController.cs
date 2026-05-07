using BankUPG.Application.Interfaces.BusinessAddress;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    /// <summary>
    /// Business Address Controller - Handles business address verification onboarding step
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BusinessAddressController : ControllerBase
    {
        private readonly IBusinessAddressService _businessAddressService;
        private readonly ILogger<BusinessAddressController> _logger;

        public BusinessAddressController(
            IBusinessAddressService businessAddressService,
            ILogger<BusinessAddressController> logger)
        {
            _businessAddressService = businessAddressService;
            _logger = logger;
        }

        /// <summary>
        /// Get business address for authenticated user
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<BusinessAddressResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BusinessAddressResponse>>> GetBusinessAddress()
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new ApiResponse<BusinessAddressResponse> { Success = false, Message = "Invalid user token" });

                var result = await _businessAddressService.GetBusinessAddressAsync(userId.Value);

                if (result == null)
                    return NotFound(new ApiResponse<BusinessAddressResponse> { Success = false, Message = "Business address not found. Please complete this step." });

                return Ok(new ApiResponse<BusinessAddressResponse> { Success = true, Message = "Business address retrieved successfully", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving business address for userId: {UserId}", GetUserId());
                return StatusCode(500, new ApiResponse<BusinessAddressResponse> { Success = false, Message = "An error occurred while retrieving business address" });
            }
        }

        /// <summary>
        /// Get business address by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<BusinessAddressResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BusinessAddressResponse>>> GetBusinessAddressById(int id)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new ApiResponse<BusinessAddressResponse> { Success = false, Message = "Invalid user token" });

                var result = await _businessAddressService.GetBusinessAddressAsync(userId.Value);
                
                if (result == null || result.BusinessAddressDetailId != id)
                    return NotFound(new ApiResponse<BusinessAddressResponse> { Success = false, Message = "Business address not found." });

                return Ok(new ApiResponse<BusinessAddressResponse> { Success = true, Message = "Business address retrieved successfully", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving business address for id: {Id}", id);
                return StatusCode(500, new ApiResponse<BusinessAddressResponse> { Success = false, Message = "An error occurred while retrieving business address" });
            }
        }

        /// <summary>
        /// Save or update business address for authenticated user
        /// </summary>
        [HttpPost("save")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<BusinessAddressSavedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<BusinessAddressSavedResponse>>> SaveBusinessAddress(
            [FromBody] SaveBusinessAddressRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<BusinessAddressSavedResponse>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized(new ApiResponse<BusinessAddressSavedResponse> { Success = false, Message = "Invalid user token" });

                var result = await _businessAddressService.SaveBusinessAddressAsync(userId.Value, request);

                return Ok(new ApiResponse<BusinessAddressSavedResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning("User or merchant not found: {Message}", ex.Message);
                return Unauthorized(new ApiResponse<BusinessAddressSavedResponse> { Success = false, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<BusinessAddressSavedResponse> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving business address for userId: {UserId}", GetUserId());
                return StatusCode(500, new ApiResponse<BusinessAddressSavedResponse> { Success = false, Message = "An error occurred while saving business address" });
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
