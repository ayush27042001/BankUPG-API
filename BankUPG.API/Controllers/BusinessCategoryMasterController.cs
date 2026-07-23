using BankUPG.Application.Interfaces.BusinessCategoryMaster;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class BusinessCategoryMasterController : ControllerBase
    {
        private readonly IBusinessCategoryMasterService _businessCategoryMasterService;
        private readonly ILogger<BusinessCategoryMasterController> _logger;

        public BusinessCategoryMasterController(
            IBusinessCategoryMasterService businessCategoryMasterService,
            ILogger<BusinessCategoryMasterController> logger)
        {
            _businessCategoryMasterService = businessCategoryMasterService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BusinessCategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<BusinessCategoryResponse>>> CreateBusinessCategory(
            [FromBody] CreateBusinessCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<BusinessCategoryResponse>
                    {
                        Success = false,
                        Message = "Invalid request data.",
                        Errors = ModelState.Values
                            .SelectMany(x => x.Errors.Select(e => e.ErrorMessage))
                            .ToList()
                    });
                }

                var result = await _businessCategoryMasterService.CreateBusinessCategoryAsync(request);

                return Ok(new ApiResponse<BusinessCategoryResponse>
                {
                    Success = true,
                    Message = "Business Category created successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Business Category");

                return BadRequest(new ApiResponse<BusinessCategoryResponse>
                {
                    Success = false,
                    Message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpGet("{businessCategoryId}")]
        [ProducesResponseType(typeof(ApiResponse<BusinessCategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BusinessCategoryResponse>>> GetBusinessCategoryById(int businessCategoryId)
        {
            try
            {
                var result = await _businessCategoryMasterService.GetBusinessCategoryByIdAsync(businessCategoryId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<BusinessCategoryResponse>
                    {
                        Success = false,
                        Message = "Business Category not found."
                    });
                }

                return Ok(new ApiResponse<BusinessCategoryResponse>
                {
                    Success = true,
                    Message = "Business Category retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Business Category");

                return BadRequest(new ApiResponse<BusinessCategoryResponse>
                {
                    Success = false,
                    Message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<BusinessCategoryResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<BusinessCategoryResponse>>>> GetBusinessCategoryList(
            [FromQuery] GetBusinessCategoryListRequest request)
        {
            try
            {
                var result = await _businessCategoryMasterService.GetBusinessCategoryListAsync(request);

                return Ok(new ApiResponse<PagedResponse<BusinessCategoryResponse>>
                {
                    Success = true,
                    Message = "Business Category list retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Business Category list");

                return BadRequest(new ApiResponse<PagedResponse<BusinessCategoryResponse>>
                {
                    Success = false,
                    Message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<BusinessCategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<BusinessCategoryResponse>>> UpdateBusinessCategory(
            [FromBody] UpdateBusinessCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<BusinessCategoryResponse>
                    {
                        Success = false,
                        Message = "Invalid request data.",
                        Errors = ModelState.Values
                            .SelectMany(x => x.Errors.Select(e => e.ErrorMessage))
                            .ToList()
                    });
                }

                var result = await _businessCategoryMasterService.UpdateBusinessCategoryAsync(request);

                return Ok(new ApiResponse<BusinessCategoryResponse>
                {
                    Success = true,
                    Message = "Business Category updated successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Business Category");

                return BadRequest(new ApiResponse<BusinessCategoryResponse>
                {
                    Success = false,
                    Message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
    }
}