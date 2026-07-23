using BankUPG.Application.Interfaces.Subscription;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/subscriptions")]
    [Authorize]
    [Produces("application/json")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _service;
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(ISubscriptionService service, ILogger<SubscriptionController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<SubscriptionResponse>>> Create([FromBody] CreateSubscriptionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<SubscriptionResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            try
            {
                var result = await _service.CreateSubscriptionAsync(userId.Value, request);
                return Ok(new ApiResponse<SubscriptionResponse> { Success = true, Message = "Subscription created", Data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("{subscriptionId:long}")]
        public async Task<ActionResult<ApiResponse<SubscriptionResponse>>> Get(long subscriptionId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetSubscriptionAsync(userId.Value, subscriptionId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Subscription not found" });
            return Ok(new ApiResponse<SubscriptionResponse> { Success = true, Message = "Subscription retrieved", Data = result });
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<SubscriptionResponse>>>> List([FromQuery] ListSubscriptionsRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.ListSubscriptionsAsync(userId.Value, request);
            return Ok(new ApiResponse<PagedResponse<SubscriptionResponse>> { Success = true, Message = "Subscriptions retrieved", Data = result });
        }

        [HttpPost("{subscriptionId:long}/cancel")]
        public async Task<ActionResult<ApiResponse>> Cancel(long subscriptionId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var success = await _service.CancelSubscriptionAsync(userId.Value, subscriptionId);
            if (!success) return BadRequest(new ApiResponse { Success = false, Message = "Subscription cannot be cancelled or not found" });
            return Ok(new ApiResponse { Success = true, Message = "Subscription cancelled" });
        }
    }
}
