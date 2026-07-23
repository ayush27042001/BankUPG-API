using BankUPG.Application.Interfaces.Dashboard;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    [Produces("application/json")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService service, ILogger<DashboardController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<DashboardSummaryResponse>>> GetSummary([FromQuery] GetDashboardSummaryRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            try
            {
                var result = await _service.GetSummaryAsync(userId.Value, request);
                return Ok(new ApiResponse<DashboardSummaryResponse> { Success = true, Message = "Dashboard summary retrieved", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard summary");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpGet("daily-metrics")]
        public async Task<ActionResult<ApiResponse<List<DailyMetricResponse>>>> GetDailyMetrics([FromQuery] int days = 30)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetDailyMetricsAsync(userId.Value, days);
            return Ok(new ApiResponse<List<DailyMetricResponse>> { Success = true, Message = "Daily metrics retrieved", Data = result });
        }
    }
}
