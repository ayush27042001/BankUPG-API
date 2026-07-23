using BankUPG.Application.Interfaces.Settlement;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.Settlement
{
    public class SettlementService : ISettlementService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<SettlementService> _logger;

        public SettlementService(AppDBContext context, ILogger<SettlementService> logger)
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

        public async Task<PagedResponse<SettlementResponse>> ListAsync(int userId, ListSettlementsRequest request)
        {
            var mid = await GetMidAsync(userId);

            var (from, to) = ResolveDateFilter(request.DateFilter, request.FromDate, request.ToDate);

            var query = _context.Settlements.AsNoTracking().Where(s => s.Mid == mid);

            if (!string.IsNullOrEmpty(request.UtrNumber))
                query = query.Where(s => s.UtrNumber != null && s.UtrNumber.Contains(request.UtrNumber));
            if (request.TransactionId.HasValue)
                query = query.Where(s => _context.TransactionCharges.AsNoTracking()
                    .Any(c => c.TransactionId == request.TransactionId && c.Mid == mid));
            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(s => s.Status == request.Status);
            if (!string.IsNullOrEmpty(request.SettlementCycle))
                query = query.Where(s => s.SettlementCycle == request.SettlementCycle);
            if (request.SettlementT.HasValue)
                query = query.Where(s => s.SettlementT == request.SettlementT);
            if (from.HasValue)
                query = query.Where(s => s.SettlementDate >= from || s.CreatedDate >= from);
            if (to.HasValue)
                query = query.Where(s => s.SettlementDate <= to || s.CreatedDate <= to);

            query = ApplySorting(query, request.SortBy, request.SortDirection);

            var total = await query.CountAsync();
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => MapToResponse(s))
                .ToListAsync();

            return new PagedResponse<SettlementResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<SettlementResponse?> GetAsync(int userId, long settlementId)
        {
            var mid = await GetMidAsync(userId);
            var s = await _context.Settlements.AsNoTracking()
                .FirstOrDefaultAsync(s => s.SettlementId == settlementId && s.Mid == mid);
            return s == null ? null : MapToResponse(s);
        }

        public async Task<SettlementSummaryResponse> GetSummaryAsync(int userId)
        {
            var mid = await GetMidAsync(userId);

            var config = await _context.MerchantSettlementConfigs.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Mid == mid && c.IsActive);

            var settlements = await _context.Settlements.AsNoTracking()
                .Where(s => s.Mid == mid)
                .ToListAsync();

            var totalSettled = settlements.Sum(s => s.SettledAmount ?? 0);

            // Pending = successful transactions not yet fully settled
            var totalSuccessAmount = await _context.Transactions.AsNoTracking()
                .Where(t => t.Mid == mid && t.Status == "Success")
                .SumAsync(t => (decimal?)t.Amount) ?? 0;
            var totalFeesOnSuccess = await _context.TransactionCharges.AsNoTracking()
                .Where(c => c.Mid == mid)
                .SumAsync(c => (decimal?)c.TotalDeduction) ?? 0;
            var pending = totalSuccessAmount - totalSettled;

            // Upcoming = pending that will be in the next settlement cycle
            var upcoming = pending > 0 ? pending * 0.98m : 0; // rough estimate or just equal to pending until cycle processes

            var lastSettled = settlements
                .Where(s => s.Status == "Settled" && s.SettledAmount.HasValue)
                .OrderByDescending(s => s.SettlementDate)
                .FirstOrDefault()?.SettledAmount ?? 0;

            return new SettlementSummaryResponse
            {
                TotalSalesAmount = settlements.Sum(s => s.SalesAmount ?? 0),
                TotalFees = totalFeesOnSuccess,
                TotalSettledAmount = totalSettled,
                PendingAmount = totalSuccessAmount - totalSettled,
                LastSettledAmount = lastSettled,
                TotalSettlementPending = pending,
                UpcomingSettlementAmount = upcoming,
                TotalSettlements = settlements.Count,
                CurrentSettlementT = config?.SettlementT ?? 0
            };
        }

        public async Task<SettlementConfigResponse?> GetConfigAsync(int userId)
        {
            var mid = await GetMidAsync(userId);
            var config = await _context.MerchantSettlementConfigs.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Mid == mid && c.IsActive);
            return config == null ? null : MapConfigToResponse(config);
        }

        public async Task<SettlementConfigResponse> UpdateConfigAsync(int userId, UpdateSettlementConfigRequest request)
        {
            var mid = await GetMidAsync(userId);

            var existing = await _context.MerchantSettlementConfigs
                .FirstOrDefaultAsync(c => c.Mid == mid && c.IsActive);

            if (existing != null)
            {
                existing.IsActive = false;
                existing.UpdatedDate = DateTime.UtcNow;
            }

            var config = new MerchantSettlementConfig
            {
                Mid = mid,
                SettlementT = request.SettlementT,
                SettlementCycleType = request.SettlementCycleType ?? "T+" + request.SettlementT,
                IsActive = true,
                EffectiveFrom = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.MerchantSettlementConfigs.Add(config);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Settlement config updated for MID {Mid}: T+{T}", mid, request.SettlementT);
            return MapConfigToResponse(config);
        }

        private static SettlementResponse MapToResponse(Infrastructure.Entities.Settlement s) => new()
        {
            SettlementId = s.SettlementId,
            Mid = s.Mid,
            UtrNumber = s.UtrNumber,
            SalesAmount = s.SalesAmount,
            Fees = s.Fees,
            SettledAmount = s.SettledAmount,
            Status = s.Status,
            SettlementCycle = s.SettlementCycle,
            SettlementT = s.SettlementT,
            SettlementDate = s.SettlementDate,
            CreatedDate = s.CreatedDate,
            UpdatedDate = s.UpdatedDate
        };

        private static SettlementConfigResponse MapConfigToResponse(MerchantSettlementConfig c) => new()
        {
            MerchantSettlementConfigId = c.MerchantSettlementConfigId,
            Mid = c.Mid,
            SettlementT = c.SettlementT,
            SettlementCycleType = c.SettlementCycleType,
            IsActive = c.IsActive,
            EffectiveFrom = c.EffectiveFrom,
            UpdatedDate = c.UpdatedDate
        };

        private static (DateTime? From, DateTime? To) ResolveDateFilter(string? dateFilter, DateTime? fromDate, DateTime? toDate)
        {
            var now = DateTime.UtcNow;
            return dateFilter?.ToLower() switch
            {
                "today" => (now.Date, now.Date.AddDays(1).AddTicks(-1)),
                "yesterday" => (now.Date.AddDays(-1), now.Date.AddTicks(-1)),
                "last7days" => (now.Date.AddDays(-7), now),
                "last30days" => (now.Date.AddDays(-30), now),
                "custom" => (fromDate, toDate),
                _ => (fromDate, toDate)
            };
        }

        private static IQueryable<Infrastructure.Entities.Settlement> ApplySorting(
            IQueryable<Infrastructure.Entities.Settlement> query, string? sortBy, string? sortDirection)
        {
            var desc = string.IsNullOrEmpty(sortDirection) || sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
            return sortBy?.ToLower() switch
            {
                "utr" or "utrnumber" => desc ? query.OrderByDescending(s => s.UtrNumber) : query.OrderBy(s => s.UtrNumber),
                "salesamount" => desc ? query.OrderByDescending(s => s.SalesAmount) : query.OrderBy(s => s.SalesAmount),
                "settledamount" => desc ? query.OrderByDescending(s => s.SettledAmount) : query.OrderBy(s => s.SettledAmount),
                "settlementdate" => desc ? query.OrderByDescending(s => s.SettlementDate) : query.OrderBy(s => s.SettlementDate),
                _ => desc ? query.OrderByDescending(s => s.CreatedDate) : query.OrderBy(s => s.CreatedDate)
            };
        }
    }
}
