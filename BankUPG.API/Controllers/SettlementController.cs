using BankUPG.Application.Interfaces.Settlement;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/settlements")]
    [Authorize]
    [Produces("application/json")]
    public class SettlementController : ControllerBase
    {
        private readonly ISettlementService _service;
        private readonly ILogger<SettlementController> _logger;

        public SettlementController(ISettlementService service, ILogger<SettlementController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        /// <summary>List settlements — filter by T+0/T+1/T+2/T+N, status, date range</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<SettlementResponse>>>> List([FromQuery] ListSettlementsRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.ListAsync(userId.Value, request);
            return Ok(new ApiResponse<PagedResponse<SettlementResponse>> { Success = true, Message = "Settlements retrieved", Data = result });
        }

        /// <summary>Get settlement by ID</summary>
        [HttpGet("{settlementId:long}")]
        public async Task<ActionResult<ApiResponse<SettlementResponse>>> Get(long settlementId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetAsync(userId.Value, settlementId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Settlement not found" });
            return Ok(new ApiResponse<SettlementResponse> { Success = true, Message = "Settlement retrieved", Data = result });
        }

        /// <summary>Get total settled / pending settlement summary</summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<SettlementSummaryResponse>>> GetSummary()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetSummaryAsync(userId.Value);
            return Ok(new ApiResponse<SettlementSummaryResponse> { Success = true, Message = "Settlement summary retrieved", Data = result });
        }

        /// <summary>Get current T+N settlement cycle configuration</summary>
        [HttpGet("config")]
        public async Task<ActionResult<ApiResponse<SettlementConfigResponse>>> GetConfig()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetConfigAsync(userId.Value);
            return Ok(new ApiResponse<SettlementConfigResponse> { Success = true, Message = "Settlement config retrieved", Data = result });
        }

        /// <summary>Update settlement cycle (T+0, T+1, T+2, T+N)</summary>
        [HttpPut("config")]
        public async Task<ActionResult<ApiResponse<SettlementConfigResponse>>> UpdateConfig([FromBody] UpdateSettlementConfigRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Validation failed" });

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            try
            {
                var result = await _service.UpdateConfigAsync(userId.Value, request);
                return Ok(new ApiResponse<SettlementConfigResponse> { Success = true, Message = $"Settlement cycle updated to T+{request.SettlementT}", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating settlement config");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }
    }
}
