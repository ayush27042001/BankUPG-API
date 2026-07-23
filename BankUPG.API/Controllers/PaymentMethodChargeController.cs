using BankUPG.Application.Interfaces.PaymentMethodCharge;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/payment-method-charges")]
    [Authorize]
    [Produces("application/json")]
    public class PaymentMethodChargeController : ControllerBase
    {
        private readonly IPaymentMethodChargeService _service;
        private readonly ILogger<PaymentMethodChargeController> _logger;

        public PaymentMethodChargeController(IPaymentMethodChargeService service, ILogger<PaymentMethodChargeController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private bool IsSuperAdmin() => User.HasClaim(ClaimTypes.Role, "SuperAdmin");

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PaymentMethodChargeResponse>>> Create([FromBody] CreatePaymentMethodChargeRequest request)
        {
            if (!IsSuperAdmin()) return Forbid();
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<PaymentMethodChargeResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            try
            {
                var result = await _service.CreateAsync(request);
                return Ok(new ApiResponse<PaymentMethodChargeResponse> { Success = true, Message = "Payment method charge created", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment method charge");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPut("{paymentMethodChargeId:int}")]
        public async Task<ActionResult<ApiResponse<PaymentMethodChargeResponse>>> Update(int paymentMethodChargeId, [FromBody] CreatePaymentMethodChargeRequest request)
        {
            if (!IsSuperAdmin()) return Forbid();
            var result = await _service.UpdateAsync(paymentMethodChargeId, request);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Payment method charge not found" });
            return Ok(new ApiResponse<PaymentMethodChargeResponse> { Success = true, Message = "Payment method charge updated", Data = result });
        }

        [HttpGet("{paymentMethodChargeId:int}")]
        public async Task<ActionResult<ApiResponse<PaymentMethodChargeResponse>>> Get(int paymentMethodChargeId)
        {
            if (!IsSuperAdmin()) return Forbid();
            var result = await _service.GetAsync(paymentMethodChargeId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Payment method charge not found" });
            return Ok(new ApiResponse<PaymentMethodChargeResponse> { Success = true, Message = "Payment method charge retrieved", Data = result });
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<PaymentMethodChargeResponse>>>> List(int pageNumber = 1, int pageSize = 20)
        {
            if (!IsSuperAdmin()) return Forbid();
            var result = await _service.ListAsync(pageNumber, pageSize);
            return Ok(new ApiResponse<PagedResponse<PaymentMethodChargeResponse>> { Success = true, Message = "Payment method charges retrieved", Data = result });
        }

        [HttpDelete("{paymentMethodChargeId:int}")]
        public async Task<ActionResult<ApiResponse>> Delete(int paymentMethodChargeId)
        {
            if (!IsSuperAdmin()) return Forbid();
            var success = await _service.DeleteAsync(paymentMethodChargeId);
            if (!success) return NotFound(new ApiResponse { Success = false, Message = "Payment method charge not found" });
            return Ok(new ApiResponse { Success = true, Message = "Payment method charge deleted" });
        }
    }
}
