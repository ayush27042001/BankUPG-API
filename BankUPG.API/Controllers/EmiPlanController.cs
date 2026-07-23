using BankUPG.Application.Interfaces.EmiPlan;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/emi-plans")]
    [Authorize]
    [Produces("application/json")]
    public class EmiPlanController : ControllerBase
    {
        private readonly IEmiPlanService _service;
        private readonly ILogger<EmiPlanController> _logger;

        public EmiPlanController(IEmiPlanService service, ILogger<EmiPlanController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<EmiPlanResponse>>> Create([FromBody] CreateEmiPlanRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<EmiPlanResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.CreateEmiPlanAsync(userId.Value, request);
            return Ok(new ApiResponse<EmiPlanResponse> { Success = true, Message = "EMI plan created", Data = result });
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<EmiPlanResponse>>>> List()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.ListEmiPlansAsync(userId.Value);
            return Ok(new ApiResponse<List<EmiPlanResponse>> { Success = true, Message = "EMI plans retrieved", Data = result });
        }

        [HttpPut("{emiPlanId:int}")]
        public async Task<ActionResult<ApiResponse<EmiPlanResponse>>> Update(int emiPlanId, [FromBody] UpdateEmiPlanRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.UpdateEmiPlanAsync(userId.Value, emiPlanId, request);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "EMI plan not found" });
            return Ok(new ApiResponse<EmiPlanResponse> { Success = true, Message = "EMI plan updated", Data = result });
        }

        [HttpDelete("{emiPlanId:int}")]
        public async Task<ActionResult<ApiResponse>> Deactivate(int emiPlanId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var success = await _service.DeactivateEmiPlanAsync(userId.Value, emiPlanId);
            if (!success) return NotFound(new ApiResponse { Success = false, Message = "EMI plan not found" });
            return Ok(new ApiResponse { Success = true, Message = "EMI plan deactivated" });
        }
    }
}
