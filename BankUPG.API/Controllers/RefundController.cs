using BankUPG.Application.Interfaces.Refund;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/refunds")]
    [Authorize]
    [Produces("application/json")]
    public class RefundController : ControllerBase
    {
        private readonly IRefundService _service;
        private readonly ILogger<RefundController> _logger;

        public RefundController(IRefundService service, ILogger<RefundController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        /// <summary>List refunds with filters for status, type, date range</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<RefundDetailResponse>>>> List([FromQuery] ListRefundsRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.ListAsync(userId.Value, request);
            return Ok(new ApiResponse<PagedResponse<RefundDetailResponse>> { Success = true, Message = "Refunds retrieved", Data = result });
        }

        /// <summary>Get refund by ID</summary>
        [HttpGet("{refundId:long}")]
        public async Task<ActionResult<ApiResponse<RefundDetailResponse>>> Get(long refundId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetAsync(userId.Value, refundId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Refund not found" });
            return Ok(new ApiResponse<RefundDetailResponse> { Success = true, Message = "Refund retrieved", Data = result });
        }

        /// <summary>Initiate a refund for a successful transaction (full or partial)</summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<RefundDetailResponse>>> Initiate([FromBody] InitiateRefundRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<RefundDetailResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            try
            {
                var result = await _service.InitiateRefundAsync(userId.Value, request);
                return Ok(new ApiResponse<RefundDetailResponse> { Success = true, Message = "Refund initiated successfully", Data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating refund");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }
    }
}
