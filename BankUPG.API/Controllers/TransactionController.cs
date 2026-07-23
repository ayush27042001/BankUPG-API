using BankUPG.Application.Interfaces.Transaction;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    [Authorize]
    [Produces("application/json")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _service;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionService service, ILogger<TransactionController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        /// <summary>List transactions with full filter support</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<TransactionResponse>>>> List([FromQuery] ListTransactionsRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            try
            {
                var result = await _service.ListAsync(userId.Value, request);
                return Ok(new ApiResponse<PagedResponse<TransactionResponse>> { Success = true, Message = "Transactions retrieved", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing transactions");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>Get transaction detail with MDR charge summary</summary>
        [HttpGet("{transactionId:long}")]
        public async Task<ActionResult<ApiResponse<TransactionResponse>>> Get(long transactionId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetAsync(userId.Value, transactionId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Transaction not found" });
            return Ok(new ApiResponse<TransactionResponse> { Success = true, Message = "Transaction retrieved", Data = result });
        }

        /// <summary>Get itemised MDR + GST charges for a transaction</summary>
        [HttpGet("{transactionId:long}/charges")]
        public async Task<ActionResult<ApiResponse<List<TransactionChargeDetailResponse>>>> GetCharges(long transactionId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetChargesAsync(userId.Value, transactionId);
            return Ok(new ApiResponse<List<TransactionChargeDetailResponse>> { Success = true, Message = "Transaction charges retrieved", Data = result });
        }

        /// <summary>Get transaction data cards: Total Payments, Number Of Transactions, Success Rate</summary>
        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<TransactionSummaryResponse>>> GetSummary([FromQuery] ListTransactionsRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetSummaryAsync(userId.Value, request);
            return Ok(new ApiResponse<TransactionSummaryResponse> { Success = true, Message = "Transaction summary retrieved", Data = result });
        }

        /// <summary>Get all MDR rate configurations (credit card, debit card, UPI, net banking etc.)</summary>
        [HttpGet("mdr-rates")]
        public async Task<ActionResult<ApiResponse<List<PaymentMethodChargeResponse>>>> GetMdrRates()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetMdrRatesAsync();
            return Ok(new ApiResponse<List<PaymentMethodChargeResponse>> { Success = true, Message = "MDR rates retrieved", Data = result });
        }
    }
}
