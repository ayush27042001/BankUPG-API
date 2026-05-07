using BankUPG.Application.Interfaces.BusinessCategory;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    /// <summary>
    /// Business Category Controller - Handles business category and sub-category selection
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BusinessCategoryController : ControllerBase
    {
        private readonly IBusinessCategoryService _businessCategoryService;
        private readonly ILogger<BusinessCategoryController> _logger;

        public BusinessCategoryController(
            IBusinessCategoryService businessCategoryService,
            ILogger<BusinessCategoryController> logger)
        {
            _businessCategoryService = businessCategoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active business categories with their sub-categories (master data)
        /// </summary>
        /// <returns>List of business categories grouped with sub-categories</returns>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(ApiResponse<List<BusinessCategoryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<BusinessCategoryDto>>>> GetAllCategories()
        {
            try
            {
                var categories = await _businessCategoryService.GetAllCategoriesAsync();

                return Ok(new ApiResponse<List<BusinessCategoryDto>>
                {
                    Success = true,
                    Message = "Business categories retrieved successfully",
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving business categories");
                return StatusCode(500, new ApiResponse<List<BusinessCategoryDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving business categories"
                });
            }
        }

        /// <summary>
        /// Get business category selection for authenticated merchant
        /// </summary>
        /// <returns>Current business category and sub-category selection</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<MerchantBusinessCategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<MerchantBusinessCategoryResponse>>> GetBusinessCategory()
        {
            try
            {
                var userIdClaim = User.FindAll(ClaimTypes.NameIdentifier)
                    .FirstOrDefault(c => int.TryParse(c.Value, out _));
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new ApiResponse<MerchantBusinessCategoryResponse>
                    {
                        Success = false,
                        Message = "Invalid user token"
                    });
                }

                var result = await _businessCategoryService.GetBusinessCategoryAsync(userId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<MerchantBusinessCategoryResponse>
                    {
                        Success = false,
                        Message = "Business category not found. Please complete this step."
                    });
                }

                return Ok(new ApiResponse<MerchantBusinessCategoryResponse>
                {
                    Success = true,
                    Message = "Business category retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving business category for userId: {UserId}",
                    User.FindAll(ClaimTypes.NameIdentifier).FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value);
                return StatusCode(500, new ApiResponse<MerchantBusinessCategoryResponse>
                {
                    Success = false,
                    Message = "An error occurred while retrieving business category"
                });
            }
        }

        /// <summary>
        /// Save or update business category selection for authenticated merchant
        /// </summary>
        /// <param name="request">Selected business category and sub-category</param>
        /// <returns>Saved response with updated onboarding status and new JWT token</returns>
        [HttpPost("save")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<BusinessCategorySavedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<BusinessCategorySavedResponse>>> SaveBusinessCategory(
            [FromBody] SaveBusinessCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<BusinessCategorySavedResponse>
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
                    return Unauthorized(new ApiResponse<BusinessCategorySavedResponse>
                    {
                        Success = false,
                        Message = "Invalid user token"
                    });
                }

                var result = await _businessCategoryService.SaveBusinessCategoryAsync(userId, request);

                return Ok(new ApiResponse<BusinessCategorySavedResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning("User or merchant not found: {Message}", ex.Message);
                return Unauthorized(new ApiResponse<BusinessCategorySavedResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<BusinessCategorySavedResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving business category for userId: {UserId}",
                    User.FindAll(ClaimTypes.NameIdentifier).FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value);
                return StatusCode(500, new ApiResponse<BusinessCategorySavedResponse>
                {
                    Success = false,
                    Message = "An error occurred while saving business category"
                });
            }
        }
    }
}
