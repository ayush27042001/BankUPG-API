using BankUPG.Application.Interfaces.BusinessEntityTypeMaster;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessEntityTypeMasterController : ControllerBase
    {
        private readonly IBusinessEntityTypeMasterService _businessEntityTypeMasterService;
        private readonly ILogger<BusinessEntityTypeMasterController> _logger;

        public BusinessEntityTypeMasterController(
            IBusinessEntityTypeMasterService businessEntityTypeMasterService,
            ILogger<BusinessEntityTypeMasterController> logger)
        {
            _businessEntityTypeMasterService = businessEntityTypeMasterService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BusinessEntityTypeMasterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<BusinessEntityTypeMasterResponse>>> CreateBusinessEntityType(
     [FromBody] CreateBusinessEntityTypeMasterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<BusinessEntityTypeMasterResponse>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _businessEntityTypeMasterService.CreateBusinessEntityTypeAsync(request);

                return Ok(new ApiResponse<BusinessEntityTypeMasterResponse>
                {
                    Success = true,
                    Message = "Business Entity Type created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Business Entity Type");

                return StatusCode(500, new ApiResponse<BusinessEntityTypeMasterResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{businessEntityTypeId}")]
        [ProducesResponseType(typeof(ApiResponse<BusinessEntityTypeMasterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BusinessEntityTypeMasterResponse>>> GetBusinessEntityTypeById(int businessEntityTypeId)
        {
            try
            {
                var result = await _businessEntityTypeMasterService.GetBusinessEntityTypeByIdAsync(businessEntityTypeId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<BusinessEntityTypeMasterResponse>
                    {
                        Success = false,
                        Message = "Business Entity Type not found"
                    });
                }

                return Ok(new ApiResponse<BusinessEntityTypeMasterResponse>
                {
                    Success = true,
                    Message = "Business Entity Type retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Business Entity Type");

                return StatusCode(500, new ApiResponse<BusinessEntityTypeMasterResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<BusinessEntityTypeMasterResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<BusinessEntityTypeMasterResponse>>>> GetAllBusinessEntityTypes()
        {
            try
            {
                var result = await _businessEntityTypeMasterService.GetAllBusinessEntityTypesAsync();

                return Ok(new ApiResponse<List<BusinessEntityTypeMasterResponse>>
                {
                    Success = true,
                    Message = "Business Entity Types retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Business Entity Types");

                return StatusCode(500, new ApiResponse<List<BusinessEntityTypeMasterResponse>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        [HttpGet("list")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<BusinessEntityTypeMasterResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<BusinessEntityTypeMasterResponse>>>> GetBusinessEntityTypeList(
            [FromQuery] GetBusinessEntityTypeListRequest request)
        {
            try
            {
                var result = await _businessEntityTypeMasterService.GetBusinessEntityTypeListAsync(request);

                return Ok(new ApiResponse<PagedResponse<BusinessEntityTypeMasterResponse>>
                {
                    Success = true,
                    Message = "Business Entity Types retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Business Entity Type list");

                return StatusCode(500, new ApiResponse<PagedResponse<BusinessEntityTypeMasterResponse>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<BusinessEntityTypeMasterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<BusinessEntityTypeMasterResponse>>> UpdateBusinessEntityType(
      [FromBody] UpdateBusinessEntityTypeMasterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<BusinessEntityTypeMasterResponse>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _businessEntityTypeMasterService.UpdateBusinessEntityTypeAsync(request);

                return Ok(new ApiResponse<BusinessEntityTypeMasterResponse>
                {
                    Success = true,
                    Message = "Business Entity Type updated successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Business Entity Type");

                return StatusCode(500, new ApiResponse<BusinessEntityTypeMasterResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
        [HttpDelete("{businessEntityTypeId}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DeleteBusinessEntityType(int businessEntityTypeId)
        {
            try
            {
                var result = await _businessEntityTypeMasterService.DeleteBusinessEntityTypeAsync(businessEntityTypeId);

                if (!result)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Business Entity Type not found"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Business Entity Type deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Business Entity Type");

                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPatch("{businessEntityTypeId}/toggle-status")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> ToggleBusinessEntityTypeStatus(int businessEntityTypeId)
        {
            try
            {
                var result = await _businessEntityTypeMasterService.ToggleBusinessEntityTypeStatusAsync(businessEntityTypeId);

                if (!result)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Business Entity Type not found"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Business Entity Type status toggled successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling Business Entity Type status");

                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}