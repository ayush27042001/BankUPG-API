using BankUPG.Application.Interfaces.Dashboard;
using BankUPG.Infrastructure.Data;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(AppDBContext context, ILogger<DashboardService> logger)
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

        public async Task<DashboardSummaryResponse> GetSummaryAsync(int userId, GetDashboardSummaryRequest request)
        {
            var mid = await GetMidAsync(userId);
            var from = request.FromDate ?? DateTime.UtcNow.Date;
            var to = (request.ToDate ?? DateTime.UtcNow.Date).AddDays(1).AddTicks(-1);

            var txQuery = _context.Transactions.AsNoTracking()
                .Where(t => t.Mid == mid && t.CreatedDate >= from && t.CreatedDate <= to);

            var totalTx = await txQuery.CountAsync();
            var successTx = await txQuery.CountAsync(t => t.Status == "Success");
            var failedTx = totalTx - successTx;
            var totalAmount = await txQuery.Where(t => t.Status == "Success").SumAsync(t => (decimal?)t.Amount) ?? 0;

            var charges = await _context.TransactionCharges.AsNoTracking()
                .Where(c => c.Mid == mid && c.CreatedDate >= from && c.CreatedDate <= to)
                .Select(c => new { c.ChargeAmount, c.GstAmount, c.TotalDeduction })
                .ToListAsync();

            var totalMdr = charges.Sum(c => c.ChargeAmount);
            var totalGst = charges.Sum(c => c.GstAmount);
            var totalDeductions = charges.Sum(c => c.TotalDeduction);

            var settledAmount = await _context.Settlements.AsNoTracking()
                .Where(s => s.Mid == mid && s.CreatedDate >= from && s.CreatedDate <= to)
                .SumAsync(s => (decimal?)s.SettledAmount) ?? 0;

            var refundAmount = await _context.Refunds.AsNoTracking()
                .Where(r => r.Mid == mid && r.Status == "Success" && r.CreatedDate >= from && r.CreatedDate <= to)
                .SumAsync(r => (decimal?)r.Amount) ?? 0;

            var refundCount = await _context.Refunds.AsNoTracking()
                .CountAsync(r => r.Mid == mid && r.Status == "Success" && r.CreatedDate >= from && r.CreatedDate <= to);

            var wallet = await _context.MerchantWallets.AsNoTracking()
                .FirstOrDefaultAsync(w => w.Mid == mid);

            return new DashboardSummaryResponse
            {
                TotalTransactionAmount = totalAmount,
                TotalTransactions = totalTx,
                SuccessfulTransactions = successTx,
                FailedTransactions = failedTx,
                SuccessRate = totalTx > 0 ? Math.Round((decimal)successTx / totalTx * 100, 2) : 0,
                TotalMdrCharges = totalMdr,
                TotalGst = totalGst,
                TotalDeductions = totalDeductions,
                TotalSettledAmount = settledAmount,
                PendingSettlementAmount = totalAmount - totalDeductions - settledAmount,
                TotalRefundAmount = refundAmount,
                TotalRefunds = refundCount,
                WalletBalance = wallet?.TotalBalance ?? 0,
                OnHoldBalance = wallet?.OnHoldBalance ?? 0,
                FromDate = from,
                ToDate = to
            };
        }

        public async Task<List<DailyMetricResponse>> GetDailyMetricsAsync(int userId, int days)
        {
            var mid = await GetMidAsync(userId);
            var fromDate = DateTime.UtcNow.Date.AddDays(-Math.Abs(days) + 1);

            var summaries = await _context.MerchantDailySummaries.AsNoTracking()
                .Where(s => s.Mid == mid && s.SummaryDate >= fromDate)
                .OrderBy(s => s.SummaryDate)
                .ToListAsync();

            return summaries.Select(s => new DailyMetricResponse
            {
                SummaryDate = s.SummaryDate,
                TotalTransactions = s.TotalTransactions,
                TotalTransactionAmount = s.TotalTransactionAmount,
                SuccessfulTransactions = s.SuccessfulTransactions,
                TotalSettledAmount = s.TotalSettledAmount,
                TotalDeductions = s.TotalDeductions,
                TotalRefundAmount = s.TotalRefundAmount
            }).ToList();
        }
    }
}
