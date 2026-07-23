using BankUPG.Application.Interfaces.Wallet;
using BankUPG.Infrastructure.Data;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.Wallet
{
    public class WalletService : IWalletService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<WalletService> _logger;

        public WalletService(AppDBContext context, ILogger<WalletService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private async Task<int> GetMidAsync(int userId)
        {
            var merchant = await _context.Merchants.AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId);
            if (merchant == null) throw new InvalidOperationException("Merchant not found.");
            return merchant.Mid;
        }

        public async Task<WalletBalanceResponse?> GetWalletAsync(int userId)
        {
            var mid = await GetMidAsync(userId);
            var wallet = await _context.MerchantWallets.AsNoTracking()
                .FirstOrDefaultAsync(w => w.Mid == mid);

            if (wallet == null)
                return new WalletBalanceResponse { Mid = mid };

            return new WalletBalanceResponse
            {
                Mid = wallet.Mid,
                TotalBalance = wallet.TotalBalance,
                OnHoldBalance = wallet.OnHoldBalance,
                RefundWalletBalance = wallet.RefundWalletBalance,
                TotalCredited = wallet.TotalCredited,
                TotalDebited = wallet.TotalDebited,
                UpdatedDate = wallet.UpdatedDate
            };
        }

        public async Task<PagedResponse<WalletLedgerItemResponse>> GetLedgerAsync(int userId, GetWalletLedgerRequest request)
        {
            var mid = await GetMidAsync(userId);

            var query = _context.WalletLedgers.AsNoTracking().Where(l => l.Mid == mid);

            if (!string.IsNullOrEmpty(request.EntryType))
                query = query.Where(l => l.EntryType == request.EntryType);
            if (request.FromDate.HasValue)
                query = query.Where(l => l.CreatedDate >= request.FromDate);
            if (request.ToDate.HasValue)
                query = query.Where(l => l.CreatedDate <= request.ToDate);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(l => l.CreatedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(l => new WalletLedgerItemResponse
                {
                    WalletLedgerId = l.WalletLedgerId,
                    ReferenceType = l.ReferenceType,
                    ReferenceId = l.ReferenceId,
                    EntryType = l.EntryType,
                    Amount = l.Amount,
                    BalanceAfter = l.BalanceAfter,
                    Description = l.Description,
                    CreatedDate = l.CreatedDate
                })
                .ToListAsync();

            return new PagedResponse<WalletLedgerItemResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
