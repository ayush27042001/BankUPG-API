using BankUPG.Application.Interfaces.SubscriptionPlan;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/subscription-plans")]
    [Authorize]
    [Produces("application/json")]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly ISubscriptionPlanService _service;
        private readonly ILogger<SubscriptionPlanController> _logger;

        public SubscriptionPlanController(ISubscriptionPlanService service, ILogger<SubscriptionPlanController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<SubscriptionPlanResponse>>> Create([FromBody] CreateSubscriptionPlanRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<SubscriptionPlanResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.CreatePlanAsync(userId.Value, request);
            return Ok(new ApiResponse<SubscriptionPlanResponse> { Success = true, Message = "Plan created", Data = result });
        }

        [HttpGet("{planId:int}")]
        public async Task<ActionResult<ApiResponse<SubscriptionPlanResponse>>> Get(int planId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetPlanAsync(userId.Value, planId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Plan not found" });
            return Ok(new ApiResponse<SubscriptionPlanResponse> { Success = true, Message = "Plan retrieved", Data = result });
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<SubscriptionPlanResponse>>>> List()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.ListPlansAsync(userId.Value);
            return Ok(new ApiResponse<List<SubscriptionPlanResponse>> { Success = true, Message = "Plans retrieved", Data = result });
        }

        [HttpPut("{planId:int}")]
        public async Task<ActionResult<ApiResponse<SubscriptionPlanResponse>>> Update(int planId, [FromBody] UpdateSubscriptionPlanRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.UpdatePlanAsync(userId.Value, planId, request);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Plan not found" });
            return Ok(new ApiResponse<SubscriptionPlanResponse> { Success = true, Message = "Plan updated", Data = result });
        }

        [HttpDelete("{planId:int}")]
        public async Task<ActionResult<ApiResponse>> Deactivate(int planId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var success = await _service.DeactivatePlanAsync(userId.Value, planId);
            if (!success) return NotFound(new ApiResponse { Success = false, Message = "Plan not found" });
            return Ok(new ApiResponse { Success = true, Message = "Plan deactivated" });
        }
    }
}
