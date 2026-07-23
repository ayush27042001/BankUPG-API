using BankUPG.Application.Interfaces.BatchRefund;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/batch-refunds")]
    [Authorize]
    [Produces("application/json")]
    public class BatchRefundController : ControllerBase
    {
        private readonly IBatchRefundService _service;
        private readonly ILogger<BatchRefundController> _logger;

        public BatchRefundController(IBatchRefundService service, ILogger<BatchRefundController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<BatchRefundResponse>>> Create([FromBody] CreateBatchRefundRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<BatchRefundResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            try
            {
                var result = await _service.CreateBatchRefundAsync(userId.Value, request);
                return Ok(new ApiResponse<BatchRefundResponse> { Success = true, Message = "Batch refund created", Data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating batch refund");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpGet("{batchRefundId:long}")]
        public async Task<ActionResult<ApiResponse<BatchRefundResponse>>> Get(long batchRefundId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetBatchRefundAsync(userId.Value, batchRefundId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Batch refund not found" });
            return Ok(new ApiResponse<BatchRefundResponse> { Success = true, Message = "Batch refund retrieved", Data = result });
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<BatchRefundResponse>>>> List([FromQuery] ListBatchRefundsRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.ListBatchRefundsAsync(userId.Value, request);
            return Ok(new ApiResponse<PagedResponse<BatchRefundResponse>> { Success = true, Message = "Batch refunds retrieved", Data = result });
        }
    }
}
