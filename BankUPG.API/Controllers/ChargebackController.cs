using BankUPG.Application.Interfaces.Chargeback;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/chargebacks")]
    [Authorize]
    [Produces("application/json")]
    public class ChargebackController : ControllerBase
    {
        private readonly IChargebackService _service;
        private readonly ILogger<ChargebackController> _logger;

        public ChargebackController(IChargebackService service, ILogger<ChargebackController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        /// <summary>List chargebacks with filters for status, type, date range</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<ChargebackResponse>>>> List([FromQuery] ListChargebacksRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.ListAsync(userId.Value, request);
            return Ok(new ApiResponse<PagedResponse<ChargebackResponse>> { Success = true, Message = "Chargebacks retrieved", Data = result });
        }

        /// <summary>Get chargeback detail — includes IsOverdue flag if ReplyBefore has passed</summary>
        [HttpGet("{chargebackId:long}")]
        public async Task<ActionResult<ApiResponse<ChargebackResponse>>> Get(long chargebackId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetAsync(userId.Value, chargebackId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Chargeback not found" });
            return Ok(new ApiResponse<ChargebackResponse> { Success = true, Message = "Chargeback retrieved", Data = result });
        }

        /// <summary>Update chargeback — set status, upload doc path, close reason</summary>
        [HttpPut("{chargebackId:long}")]
        public async Task<ActionResult<ApiResponse<ChargebackResponse>>> Update(long chargebackId, [FromBody] UpdateChargebackRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.UpdateAsync(userId.Value, chargebackId, request);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Chargeback not found" });
            return Ok(new ApiResponse<ChargebackResponse> { Success = true, Message = "Chargeback updated", Data = result });
        }
    }
}
