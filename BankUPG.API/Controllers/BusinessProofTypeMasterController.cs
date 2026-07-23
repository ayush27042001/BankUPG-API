using BankUPG.Application.Interfaces.BusinessProofTypeMaster;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class BusinessProofTypeMasterController : ControllerBase
    {
        private readonly IBusinessProofTypeMasterService _businessProofTypeMasterService;
        private readonly ILogger<BusinessProofTypeMasterController> _logger;

        public BusinessProofTypeMasterController(
            IBusinessProofTypeMasterService businessProofTypeMasterService,
            ILogger<BusinessProofTypeMasterController> logger)
        {
            _businessProofTypeMasterService = businessProofTypeMasterService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BusinessProofTypeMasterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<BusinessProofTypeMasterResponse>>> CreateBusinessProofType([FromBody] CreateBusinessProofTypeMasterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<BusinessProofTypeMasterResponse>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _businessProofTypeMasterService.CreateBusinessProofTypeAsync(request);

                return Ok(new ApiResponse<BusinessProofTypeMasterResponse>
                {
                    Success = true,
                    Message = "Business proof type created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating business proof type");
                return StatusCode(500, new ApiResponse<BusinessProofTypeMasterResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{businessProofTypeId}")]
        [ProducesResponseType(typeof(ApiResponse<BusinessProofTypeMasterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BusinessProofTypeMasterResponse>>> GetBusinessProofTypeById(int businessProofTypeId)
        {
            try
            {
                var result = await _businessProofTypeMasterService.GetBusinessProofTypeByIdAsync(businessProofTypeId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<BusinessProofTypeMasterResponse>
                    {
                        Success = false,
                        Message = "Business proof type not found"
                    });
                }

                return Ok(new ApiResponse<BusinessProofTypeMasterResponse>
                {
                    Success = true,
                    Message = "Business proof type retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving business proof type");
                return StatusCode(500, new ApiResponse<BusinessProofTypeMasterResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<BusinessProofTypeMasterResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<BusinessProofTypeMasterResponse>>>> GetAllBusinessProofTypes()
        {
            try
            {
                var result = await _businessProofTypeMasterService.GetAllBusinessProofTypesAsync();

                return Ok(new ApiResponse<List<BusinessProofTypeMasterResponse>>
                {
                    Success = true,
                    Message = "Business proof types retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving business proof types");
                return StatusCode(500, new ApiResponse<List<BusinessProofTypeMasterResponse>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<BusinessProofTypeMasterResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<BusinessProofTypeMasterResponse>>>> GetBusinessProofTypeList([FromQuery] GetBusinessProofTypeListRequest request)
        {
            try
            {
                var result = await _businessProofTypeMasterService.GetBusinessProofTypeListAsync(request);

                return Ok(new ApiResponse<PagedResponse<BusinessProofTypeMasterResponse>>
                {
                    Success = true,
                    Message = "Business proof types retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving business proof type list");
                return StatusCode(500, new ApiResponse<PagedResponse<BusinessProofTypeMasterResponse>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<BusinessProofTypeMasterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<BusinessProofTypeMasterResponse>>> UpdateBusinessProofType([FromBody] UpdateBusinessProofTypeMasterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<BusinessProofTypeMasterResponse>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _businessProofTypeMasterService.UpdateBusinessProofTypeAsync(request);

                return Ok(new ApiResponse<BusinessProofTypeMasterResponse>
                {
                    Success = true,
                    Message = "Business proof type updated successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating business proof type");
                return StatusCode(500, new ApiResponse<BusinessProofTypeMasterResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("{businessProofTypeId}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DeleteBusinessProofType(int businessProofTypeId)
        {
            try
            {
                var result = await _businessProofTypeMasterService.DeleteBusinessProofTypeAsync(businessProofTypeId);

                if (!result)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Business proof type not found"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Business proof type deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting business proof type");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPatch("{businessProofTypeId}/toggle-status")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> ToggleBusinessProofTypeStatus(int businessProofTypeId)
        {
            try
            {
                var result = await _businessProofTypeMasterService.ToggleBusinessProofTypeStatusAsync(businessProofTypeId);

                if (!result)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Business proof type not found"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Business proof type status toggled successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling business proof type status");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
