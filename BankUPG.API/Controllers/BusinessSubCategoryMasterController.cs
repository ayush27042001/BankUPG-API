using BankUPG.Application.Interfaces.BusinessSubCategoryMaster;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BankUPG.Application.Interfaces.BusinessSubCategoryMaster;
namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class BusinessSubCategoryMasterController : ControllerBase
    {
        private readonly IBusinessSubCategoryMasterService _businessSubCategoryMasterService;
        private readonly ILogger<BusinessSubCategoryMasterController> _logger;

        public BusinessSubCategoryMasterController(
            IBusinessSubCategoryMasterService businessSubCategoryMasterService,
            ILogger<BusinessSubCategoryMasterController> logger)
        {
            _businessSubCategoryMasterService = businessSubCategoryMasterService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BusinessSubCategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<BusinessSubCategoryResponse>>> CreateBusinessSubCategory(
            [FromBody] CreateBusinessSubCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<BusinessSubCategoryResponse>
                    {
                        Success = false,
                        Message = "Invalid request data.",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                            .ToList()
                    });
                }

                var result = await _businessSubCategoryMasterService.CreateBusinessSubCategoryAsync(request);

                return Ok(new ApiResponse<BusinessSubCategoryResponse>
                {
                    Success = true,
                    Message = "Business Sub Category created successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.ToString());

                return BadRequest(new ApiResponse<BusinessSubCategoryResponse>
                {
                    Success = false,
                    Message = ex.ToString()
                });
            }
        }

        [HttpGet("{businessSubCategoryId}")]
        [ProducesResponseType(typeof(ApiResponse<BusinessSubCategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BusinessSubCategoryResponse>>> GetBusinessSubCategoryById(int businessSubCategoryId)
        {
            try
            {
                var result = await _businessSubCategoryMasterService.GetBusinessSubCategoryByIdAsync(businessSubCategoryId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<BusinessSubCategoryResponse>
                    {
                        Success = false,
                        Message = "Business Sub Category not found."
                    });
                }

                return Ok(new ApiResponse<BusinessSubCategoryResponse>
                {
                    Success = true,
                    Message = "Business Sub Category retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Business Sub Category");

                return BadRequest(new ApiResponse<BusinessSubCategoryResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<BusinessSubCategoryResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<BusinessSubCategoryResponse>>>> GetBusinessSubCategoryList(
            [FromQuery] GetBusinessSubCategoryListRequest request)
        {
            try
            {
                var result = await _businessSubCategoryMasterService.GetBusinessSubCategoryListAsync(request);

                return Ok(new ApiResponse<PagedResponse<BusinessSubCategoryResponse>>
                {
                    Success = true,
                    Message = "Business Sub Category list retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Business Sub Category list");

                return BadRequest(new ApiResponse<PagedResponse<BusinessSubCategoryResponse>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<BusinessSubCategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<BusinessSubCategoryResponse>>> UpdateBusinessSubCategory(
            [FromBody] UpdateBusinessSubCategoryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<BusinessSubCategoryResponse>
                    {
                        Success = false,
                        Message = "Invalid request data.",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                            .ToList()
                    });
                }

                var result = await _businessSubCategoryMasterService.UpdateBusinessSubCategoryAsync(request);

                return Ok(new ApiResponse<BusinessSubCategoryResponse>
                {
                    Success = true,
                    Message = "Business Sub Category updated successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Business Sub Category");

                return BadRequest(new ApiResponse<BusinessSubCategoryResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}