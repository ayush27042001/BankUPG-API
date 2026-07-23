using BankUPG.Application.Interfaces.PaymentLink;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/payment-links")]
    [Authorize]
    [Produces("application/json")]
    public class PaymentLinkController : ControllerBase
    {
        private readonly IPaymentLinkService _service;
        private readonly ILogger<PaymentLinkController> _logger;

        public PaymentLinkController(IPaymentLinkService service, ILogger<PaymentLinkController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PaymentLinkResponse>>> Create([FromBody] CreatePaymentLinkRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<PaymentLinkResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            try
            {
                var result = await _service.CreateLinkAsync(userId.Value, request);
                return Ok(new ApiResponse<PaymentLinkResponse> { Success = true, Message = "Payment link created", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment link");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpGet("{linkId:long}")]
        public async Task<ActionResult<ApiResponse<PaymentLinkResponse>>> Get(long linkId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetLinkAsync(userId.Value, linkId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Payment link not found" });
            return Ok(new ApiResponse<PaymentLinkResponse> { Success = true, Message = "Payment link retrieved", Data = result });
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<PaymentLinkResponse>>>> List([FromQuery] ListPaymentLinksRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.ListLinksAsync(userId.Value, request);
            return Ok(new ApiResponse<PagedResponse<PaymentLinkResponse>> { Success = true, Message = "Payment links retrieved", Data = result });
        }

        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<PaymentLinkSummaryResponse>>> GetSummary()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetSummaryAsync(userId.Value);
            return Ok(new ApiResponse<PaymentLinkSummaryResponse> { Success = true, Message = "Payment link summary retrieved", Data = result });
        }

        [HttpPost("{linkId:long}/cancel")]
        public async Task<ActionResult<ApiResponse>> Cancel(long linkId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var success = await _service.CancelLinkAsync(userId.Value, linkId);
            if (!success) return BadRequest(new ApiResponse { Success = false, Message = "Link cannot be cancelled (already paid or not found)" });
            return Ok(new ApiResponse { Success = true, Message = "Payment link cancelled" });
        }
    }
}
