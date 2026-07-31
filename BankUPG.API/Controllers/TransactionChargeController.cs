using BankUPG.Application.Interfaces.Transaction;
using BankUPG.Application.Interfaces.TransactionCharge;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/transaction-charges")]
    [Authorize]
    [Produces("application/json")]
    public class TransactionChargeController : ControllerBase
    {
        private readonly ITransactionChargeService _service;
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionChargeController> _logger;

        public TransactionChargeController(
            ITransactionChargeService service,
            ITransactionService transactionService,
            ILogger<TransactionChargeController> logger)
        {
            _service = service;
            _transactionService = transactionService;
            _logger = logger;
        }

        private bool IsSuperAdmin() => User.HasClaim(ClaimTypes.Role, "SuperAdmin");

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        /// <summary>Merchant: get MDR charges for own transaction.</summary>
        [HttpGet("by-transaction/{transactionId:long}")]
        [ProducesResponseType(typeof(ApiResponse<List<TransactionChargeDetailResponse>>), 200)]
        public async Task<ActionResult<ApiResponse<List<TransactionChargeDetailResponse>>>> GetByTransaction(long transactionId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _transactionService.GetChargesAsync(userId.Value, transactionId);
            return Ok(new ApiResponse<List<TransactionChargeDetailResponse>> { Success = true, Message = "Transaction charges retrieved", Data = result });
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<TransactionChargeResponse>>> Create([FromBody] CreateTransactionChargeRequest request)
        {
            if (!IsSuperAdmin()) return Forbid();
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<TransactionChargeResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            try
            {
                var result = await _service.CreateAsync(request);
                return Ok(new ApiResponse<TransactionChargeResponse> { Success = true, Message = "Transaction charge created", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction charge");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPut("{transactionChargeId:long}")]
        public async Task<ActionResult<ApiResponse<TransactionChargeResponse>>> Update(long transactionChargeId, [FromBody] UpdateTransactionChargeRequest request)
        {
            if (!IsSuperAdmin()) return Forbid();
            if (transactionChargeId != request.TransactionChargeId)
                return BadRequest(new ApiResponse { Success = false, Message = "ID mismatch" });

            var result = await _service.UpdateAsync(transactionChargeId, request);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Transaction charge not found" });
            return Ok(new ApiResponse<TransactionChargeResponse> { Success = true, Message = "Transaction charge updated", Data = result });
        }

        [HttpGet("{transactionChargeId:long}")]
        public async Task<ActionResult<ApiResponse<TransactionChargeResponse>>> Get(long transactionChargeId)
        {
            if (!IsSuperAdmin()) return Forbid();
            var result = await _service.GetAsync(transactionChargeId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Transaction charge not found" });
            return Ok(new ApiResponse<TransactionChargeResponse> { Success = true, Message = "Transaction charge retrieved", Data = result });
        }

        [HttpPost("recalculate/{transactionId:long}")]
        public async Task<ActionResult<ApiResponse<TransactionChargeResponse>>> Recalculate(long transactionId)
        {
            if (!IsSuperAdmin()) return Forbid();
            var result = await _service.RecalculateAsync(transactionId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Transaction not found" });
            return Ok(new ApiResponse<TransactionChargeResponse> { Success = true, Message = "Transaction charge recalculated", Data = result });
        }

        [HttpDelete("{transactionChargeId:long}")]
        public async Task<ActionResult<ApiResponse>> Delete(long transactionChargeId)
        {
            if (!IsSuperAdmin()) return Forbid();
            var success = await _service.DeleteAsync(transactionChargeId);
            if (!success) return NotFound(new ApiResponse { Success = false, Message = "Transaction charge not found" });
            return Ok(new ApiResponse { Success = true, Message = "Transaction charge deleted" });
        }
    }
}
