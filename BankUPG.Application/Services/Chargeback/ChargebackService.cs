using BankUPG.Application.Interfaces.Chargeback;
using BankUPG.Infrastructure.Data;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.Chargeback
{
    public class ChargebackService : IChargebackService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<ChargebackService> _logger;

        public ChargebackService(AppDBContext context, ILogger<ChargebackService> logger)
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

        public async Task<PagedResponse<ChargebackResponse>> ListAsync(int userId, ListChargebacksRequest request)
        {
            var mid = await GetMidAsync(userId);
            var (from, to) = ResolveDateFilter(request.DateFilter, request.FromDate, request.ToDate);

            var query = _context.Chargebacks.AsNoTracking().Where(c => c.Mid == mid);

            if (request.TransactionId.HasValue)
                query = query.Where(c => c.TransactionId == request.TransactionId);
            if (!string.IsNullOrEmpty(request.BankCaseNumber))
                query = query.Where(c => c.BankCaseNumber != null && c.BankCaseNumber.Contains(request.BankCaseNumber));
            if (!string.IsNullOrEmpty(request.CaseNumber))
                query = query.Where(c => c.CaseNumber != null && c.CaseNumber.Contains(request.CaseNumber));
            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(c => c.Status == request.Status);
            if (!string.IsNullOrEmpty(request.ChargebackType))
                query = query.Where(c => c.ChargebackType == request.ChargebackType);
            if (!string.IsNullOrEmpty(request.CloseReason))
                query = query.Where(c => c.CloseReason == request.CloseReason);
            if (from.HasValue)
                query = query.Where(c => c.ChargebackDate >= from || c.CreatedDate >= from);
            if (to.HasValue)
                query = query.Where(c => c.ChargebackDate <= to || c.CreatedDate <= to);

            query = ApplySorting(query, request.SortBy, request.SortDirection);

            var total = await query.CountAsync();
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => MapToResponse(c))
                .ToListAsync();

            return new PagedResponse<ChargebackResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<ChargebackResponse?> GetAsync(int userId, long chargebackId)
        {
            var mid = await GetMidAsync(userId);
            var cb = await _context.Chargebacks.AsNoTracking()
                .FirstOrDefaultAsync(c => c.ChargebackId == chargebackId && c.Mid == mid);
            return cb == null ? null : MapToResponse(cb);
        }

        public async Task<ChargebackResponse?> UpdateAsync(int userId, long chargebackId, UpdateChargebackRequest request)
        {
            var mid = await GetMidAsync(userId);
            var cb = await _context.Chargebacks
                .FirstOrDefaultAsync(c => c.ChargebackId == chargebackId && c.Mid == mid);

            if (cb == null) return null;

            if (!string.IsNullOrEmpty(request.Status)) cb.Status = request.Status;
            if (!string.IsNullOrEmpty(request.CloseReason)) cb.CloseReason = request.CloseReason;
            if (!string.IsNullOrEmpty(request.DocumentPath)) cb.DocumentPath = request.DocumentPath;
            cb.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Chargeback {ChargebackId} updated for MID {Mid}, Status: {Status}", chargebackId, mid, cb.Status);
            return MapToResponse(cb);
        }

        private static ChargebackResponse MapToResponse(Infrastructure.Entities.Chargeback c) => new()
        {
            ChargebackId = c.ChargebackId,
            Mid = c.Mid,
            TransactionId = c.TransactionId,
            PayuId = c.PayuId,
            BankCaseNumber = c.BankCaseNumber,
            CaseNumber = c.CaseNumber,
            ChargebackDate = c.ChargebackDate,
            ReplyBefore = c.ReplyBefore,
            Status = c.Status,
            ChargebackReason = c.ChargebackReason,
            ChargebackType = c.ChargebackType,
            CloseReason = c.CloseReason,
            DocumentPath = c.DocumentPath,
            CreatedDate = c.CreatedDate,
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

        private static IQueryable<Infrastructure.Entities.Chargeback> ApplySorting(
            IQueryable<Infrastructure.Entities.Chargeback> query, string? sortBy, string? sortDirection)
        {
            var desc = string.IsNullOrEmpty(sortDirection) || sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
            return sortBy?.ToLower() switch
            {
                "transactionid" => desc ? query.OrderByDescending(c => c.TransactionId) : query.OrderBy(c => c.TransactionId),
                "casenumber" => desc ? query.OrderByDescending(c => c.CaseNumber) : query.OrderBy(c => c.CaseNumber),
                "chargebackdate" => desc ? query.OrderByDescending(c => c.ChargebackDate) : query.OrderBy(c => c.ChargebackDate),
                "replybefore" => desc ? query.OrderByDescending(c => c.ReplyBefore) : query.OrderBy(c => c.ReplyBefore),
                "status" => desc ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
                _ => desc ? query.OrderByDescending(c => c.CreatedDate) : query.OrderBy(c => c.CreatedDate)
            };
        }
    }
}
