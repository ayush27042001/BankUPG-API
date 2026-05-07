using BankUPG.Application.Interfaces.BusinessEntity;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    /// <summary>
    /// Business Entity Controller - Handles business entity type selection
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BusinessEntityController : ControllerBase
    {
        private readonly IBusinessEntityService _businessEntityService;
        private readonly ILogger<BusinessEntityController> _logger;

        public BusinessEntityController(
            IBusinessEntityService businessEntityService,
            ILogger<BusinessEntityController> logger)
        {
            _businessEntityService = businessEntityService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active business entity types (master data)
        /// </summary>
        /// <returns>List of business entity types</returns>
        [HttpGet("types")]
        [ProducesResponseType(typeof(ApiResponse<List<BusinessEntityTypeDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<BusinessEntityTypeDto>>>> GetBusinessEntityTypes()
        {
            try
            {
                var types = await _businessEntityService.GetBusinessEntityTypesAsync();

                return Ok(new ApiResponse<List<BusinessEntityTypeDto>>
                {
                    Success = true,
                    Message = "Business entity types retrieved successfully",
                    Data = types
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving business entity types");
                return StatusCode(500, new ApiResponse<List<BusinessEntityTypeDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving business entity types"
                });
            }
        }

        /// <summary>
        /// Get business entity for authenticated user
        /// </summary>
        /// <returns>Current business entity selection</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<BusinessEntityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BusinessEntityResponse>>> GetBusinessEntity()
        {
            try
            {
                var userIdClaim = User.FindAll(ClaimTypes.NameIdentifier)
                    .FirstOrDefault(c => int.TryParse(c.Value, out _));
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new ApiResponse<BusinessEntityResponse>
                    {
                        Success = false,
                        Message = "Invalid user token"
                    });
                }

                var result = await _businessEntityService.GetBusinessEntityAsync(userId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<BusinessEntityResponse>
                    {
                        Success = false,
                        Message = "Business entity not found. Please complete this step."
                    });
                }

                return Ok(new ApiResponse<BusinessEntityResponse>
                {
                    Success = true,
                    Message = "Business entity retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving business entity for userId: {UserId}",
                    User.FindAll(ClaimTypes.NameIdentifier).FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value);
                return StatusCode(500, new ApiResponse<BusinessEntityResponse>
                {
                    Success = false,
                    Message = "An error occurred while retrieving business entity"
                });
            }
        }

        /// <summary>
        /// Save or update business entity for authenticated user
        /// </summary>
        /// <param name="request">Business entity type selection</param>
        /// <returns>Saved response with updated onboarding status and new JWT token</returns>
        [HttpPost("save")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<BusinessEntitySavedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<BusinessEntitySavedResponse>>> SaveBusinessEntity(
            [FromBody] SaveBusinessEntityRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<BusinessEntitySavedResponse>
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
                    return Unauthorized(new ApiResponse<BusinessEntitySavedResponse>
                    {
                        Success = false,
                        Message = "Invalid user token"
                    });
                }

                var result = await _businessEntityService.SaveBusinessEntityAsync(userId, request);

                return Ok(new ApiResponse<BusinessEntitySavedResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning("User or merchant not found: {Message}", ex.Message);
                return Unauthorized(new ApiResponse<BusinessEntitySavedResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<BusinessEntitySavedResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving business entity for userId: {UserId}",
                    User.FindAll(ClaimTypes.NameIdentifier).FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value);
                return StatusCode(500, new ApiResponse<BusinessEntitySavedResponse>
                {
                    Success = false,
                    Message = "An error occurred while saving business entity"
                });
            }
        }
    }
}
