using BankUPG.Application.Interfaces.Payout;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/payouts")]
    [Authorize]
    [Produces("application/json")]
    public class PayoutController : ControllerBase
    {
        private readonly IPayoutService _service;
        private readonly ILogger<PayoutController> _logger;

        public PayoutController(IPayoutService service, ILogger<PayoutController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PayoutResponse>>> Create([FromBody] CreatePayoutRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<PayoutResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            try
            {
                var result = await _service.CreatePayoutAsync(userId.Value, request);
                return Ok(new ApiResponse<PayoutResponse> { Success = true, Message = "Payout queued successfully", Data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payout");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpGet("{payoutId:long}")]
        public async Task<ActionResult<ApiResponse<PayoutResponse>>> Get(long payoutId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetPayoutAsync(userId.Value, payoutId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Payout not found" });
            return Ok(new ApiResponse<PayoutResponse> { Success = true, Message = "Payout retrieved", Data = result });
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<PayoutResponse>>>> List([FromQuery] ListPayoutsRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.ListPayoutsAsync(userId.Value, request);
            return Ok(new ApiResponse<PagedResponse<PayoutResponse>> { Success = true, Message = "Payouts retrieved", Data = result });
        }

        [HttpPost("{payoutId:long}/cancel")]
        public async Task<ActionResult<ApiResponse>> Cancel(long payoutId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var success = await _service.CancelPayoutAsync(userId.Value, payoutId);
            if (!success) return BadRequest(new ApiResponse { Success = false, Message = "Payout cannot be cancelled (not in queued state or not found)" });
            return Ok(new ApiResponse { Success = true, Message = "Payout cancelled" });
        }
    }
}
