using BankUPG.Application.Interfaces.MerchantMaster;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class MerchantMasterController : ControllerBase
    {
        private readonly IMerchantMasterService _merchantMasterService;
        private readonly ILogger<MerchantMasterController> _logger;

        public MerchantMasterController(
            IMerchantMasterService merchantMasterService,
            ILogger<MerchantMasterController> logger)
        {
            _merchantMasterService = merchantMasterService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<MerchantResponse>>> CreateMerchant(
            [FromBody] CreateMerchantRequest request)
        {
            try
            {
                var result = await _merchantMasterService.CreateMerchantAsync(request);

                return Ok(new ApiResponse<MerchantResponse>
                {
                    Success = true,
                    Message = "Merchant created successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating merchant");

                return BadRequest(new ApiResponse<MerchantResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{merchantId}")]
        public async Task<ActionResult<ApiResponse<MerchantResponse>>> GetMerchantById(
            int merchantId)
        {
            try
            {
                var result = await _merchantMasterService.GetMerchantByIdAsync(merchantId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<MerchantResponse>
                    {
                        Success = false,
                        Message = "Merchant not found."
                    });
                }

                return Ok(new ApiResponse<MerchantResponse>
                {
                    Success = true,
                    Message = "Merchant retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting merchant");

                return BadRequest(new ApiResponse<MerchantResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("list")]
        public async Task<ActionResult<ApiResponse<PagedResponse<MerchantResponse>>>> GetMerchantList(
            [FromQuery] GetMerchantListRequest request)
        {
            try
            {
                var result = await _merchantMasterService.GetMerchantListAsync(request);

                return Ok(new ApiResponse<PagedResponse<MerchantResponse>>
                {
                    Success = true,
                    Message = "Merchant list retrieved successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting merchant list");

                return BadRequest(new ApiResponse<PagedResponse<MerchantResponse>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}