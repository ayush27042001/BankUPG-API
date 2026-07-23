using BankUPG.Application.Interfaces.Wallet;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/wallet")]
    [Authorize]
    [Produces("application/json")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _service;
        private readonly ILogger<WalletController> _logger;

        public WalletController(IWalletService service, ILogger<WalletController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        [HttpGet]
        public async Task<ActionResult<ApiResponse<WalletBalanceResponse>>> GetBalance()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetWalletAsync(userId.Value);
            return Ok(new ApiResponse<WalletBalanceResponse> { Success = true, Message = "Wallet balance retrieved", Data = result });
        }

        [HttpGet("ledger")]
        public async Task<ActionResult<ApiResponse<PagedResponse<WalletLedgerItemResponse>>>> GetLedger([FromQuery] GetWalletLedgerRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetLedgerAsync(userId.Value, request);
            return Ok(new ApiResponse<PagedResponse<WalletLedgerItemResponse>> { Success = true, Message = "Wallet ledger retrieved", Data = result });
        }
    }
}
