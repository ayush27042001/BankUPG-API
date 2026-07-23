using BankUPG.Application.Interfaces.MerchantPaymentMethod;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/merchant-payment-methods")]
    [Authorize(Roles = "SuperAdmin")]
    [Produces("application/json")]
    public class MerchantPaymentMethodController : ControllerBase
    {
        private readonly IMerchantPaymentMethodService _service;
        private readonly ILogger<MerchantPaymentMethodController> _logger;

        public MerchantPaymentMethodController(IMerchantPaymentMethodService service, ILogger<MerchantPaymentMethodController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<MerchantPaymentMethodResponse>>> Create([FromBody] CreateMerchantPaymentMethodRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<MerchantPaymentMethodResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            try
            {
                var result = await _service.CreateAsync(request);
                return Ok(new ApiResponse<MerchantPaymentMethodResponse> { Success = true, Message = "Merchant payment method created", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating merchant payment method");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPut("{merchantPaymentMethodId:int}")]
        public async Task<ActionResult<ApiResponse<MerchantPaymentMethodResponse>>> Update(int merchantPaymentMethodId, [FromBody] UpdateMerchantPaymentMethodRequest request)
        {
            if (merchantPaymentMethodId != request.MerchantPaymentMethodId)
                return BadRequest(new ApiResponse { Success = false, Message = "ID mismatch" });

            var result = await _service.UpdateAsync(merchantPaymentMethodId, request);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Merchant payment method not found" });
            return Ok(new ApiResponse<MerchantPaymentMethodResponse> { Success = true, Message = "Merchant payment method updated", Data = result });
        }

        [HttpGet("{merchantPaymentMethodId:int}")]
        public async Task<ActionResult<ApiResponse<MerchantPaymentMethodResponse>>> Get(int merchantPaymentMethodId)
        {
            var result = await _service.GetAsync(merchantPaymentMethodId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Merchant payment method not found" });
            return Ok(new ApiResponse<MerchantPaymentMethodResponse> { Success = true, Message = "Merchant payment method retrieved", Data = result });
        }

        [HttpGet("by-mid/{mid:int}")]
        public async Task<ActionResult<ApiResponse<PagedResponse<MerchantPaymentMethodResponse>>>> ListByMid(int mid, int pageNumber = 1, int pageSize = 20)
        {
            var result = await _service.ListByMidAsync(mid, pageNumber, pageSize);
            return Ok(new ApiResponse<PagedResponse<MerchantPaymentMethodResponse>> { Success = true, Message = "Merchant payment methods retrieved", Data = result });
        }

        [HttpDelete("{merchantPaymentMethodId:int}")]
        public async Task<ActionResult<ApiResponse>> Delete(int merchantPaymentMethodId)
        {
            var success = await _service.DeleteAsync(merchantPaymentMethodId);
            if (!success) return NotFound(new ApiResponse { Success = false, Message = "Merchant payment method not found" });
            return Ok(new ApiResponse { Success = true, Message = "Merchant payment method deleted" });
        }
    }
}
