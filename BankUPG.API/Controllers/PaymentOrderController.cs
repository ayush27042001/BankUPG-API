using BankUPG.Application.Interfaces.PaymentOrder;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/payment-orders")]
    [Authorize]
    [Produces("application/json")]
    public class PaymentOrderController : ControllerBase
    {
        private readonly IPaymentOrderService _service;
        private readonly ILogger<PaymentOrderController> _logger;

        public PaymentOrderController(IPaymentOrderService service, ILogger<PaymentOrderController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PaymentOrderResponse>>> Create([FromBody] CreatePaymentOrderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<PaymentOrderResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            try
            {
                var result = await _service.CreateOrderAsync(userId.Value, request);
                return Ok(new ApiResponse<PaymentOrderResponse> { Success = true, Message = "Order created successfully", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment order");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpGet("{orderId:long}")]
        public async Task<ActionResult<ApiResponse<PaymentOrderResponse>>> Get(long orderId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetOrderAsync(userId.Value, orderId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Order not found" });
            return Ok(new ApiResponse<PaymentOrderResponse> { Success = true, Message = "Order retrieved", Data = result });
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<PaymentOrderResponse>>>> List([FromQuery] ListPaymentOrdersRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.ListOrdersAsync(userId.Value, request);
            return Ok(new ApiResponse<PagedResponse<PaymentOrderResponse>> { Success = true, Message = "Orders retrieved", Data = result });
        }

        [HttpPost("{orderId:long}/cancel")]
        public async Task<ActionResult<ApiResponse>> Cancel(long orderId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var success = await _service.CancelOrderAsync(userId.Value, orderId);
            if (!success) return BadRequest(new ApiResponse { Success = false, Message = "Order cannot be cancelled or not found" });
            return Ok(new ApiResponse { Success = true, Message = "Order cancelled" });
        }

        [HttpGet("{orderId:long}/attempts")]
        public async Task<ActionResult<ApiResponse<List<PaymentAttemptResponse>>>> GetAttempts(long orderId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetOrderAttemptsAsync(userId.Value, orderId);
            return Ok(new ApiResponse<List<PaymentAttemptResponse>> { Success = true, Message = "Attempts retrieved", Data = result });
        }
    }
}
