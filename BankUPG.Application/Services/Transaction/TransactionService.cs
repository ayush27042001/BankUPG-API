using BankUPG.Application.Interfaces.Transaction;
using BankUPG.Infrastructure.Data;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.Transaction
{
    public class TransactionService : ITransactionService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(AppDBContext context, ILogger<TransactionService> logger)
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

        public async Task<PagedResponse<TransactionResponse>> ListAsync(int userId, ListTransactionsRequest request)
        {
            var mid = await GetMidAsync(userId);

            var (from, to) = ResolveDateFilter(request.DateFilter, request.FromDate, request.ToDate);

            var query = _context.Transactions.AsNoTracking()
                .Include(t => t.TransactionCharges)
                .Include(t => t.Refunds)
                .Where(t => t.Mid == mid);

            if (request.TransactionId.HasValue)
                query = query.Where(t => t.TransactionId == request.TransactionId);
            if (!string.IsNullOrEmpty(request.CustomerEmail))
                query = query.Where(t => t.CustomerEmail != null && t.CustomerEmail.Contains(request.CustomerEmail));
            if (!string.IsNullOrEmpty(request.CustomerPhone))
                query = query.Where(t => t.CustomerPhone != null && t.CustomerPhone.Contains(request.CustomerPhone));
            if (!string.IsNullOrEmpty(request.CustomerName))
                query = query.Where(t => t.CustomerName != null && t.CustomerName.Contains(request.CustomerName));
            if (!string.IsNullOrEmpty(request.MerchantReferenceId))
                query = query.Where(t => t.MerchantReferenceId != null && t.MerchantReferenceId.Contains(request.MerchantReferenceId));
            if (!string.IsNullOrEmpty(request.UpiReference))
                query = query.Where(t => t.UpiReference != null && t.UpiReference.Contains(request.UpiReference));
            if (!string.IsNullOrEmpty(request.BankReference))
                query = query.Where(t => t.BankReference != null && t.BankReference.Contains(request.BankReference));
            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(t => t.Status == request.Status);
            if (!string.IsNullOrEmpty(request.PaymentMode))
                query = query.Where(t => t.PaymentMode == request.PaymentMode);
            if (!string.IsNullOrEmpty(request.Source))
                query = query.Where(t => t.Source == request.Source);
            if (!string.IsNullOrEmpty(request.RefundType))
                query = query.Where(t => t.Refunds.Any(r => r.RefundType == request.RefundType));
            if (from.HasValue)
                query = query.Where(t => t.CreatedDate >= from);
            if (to.HasValue)
                query = query.Where(t => t.CreatedDate <= to);
            if (request.MinAmount.HasValue)
                query = query.Where(t => t.Amount >= request.MinAmount);
            if (request.MaxAmount.HasValue)
                query = query.Where(t => t.Amount <= request.MaxAmount);
            if (request.OrderId.HasValue)
                query = query.Where(t => t.OrderId == request.OrderId);
            if (request.PaymentLinkId.HasValue)
                query = query.Where(t => t.PaymentLinkId == request.PaymentLinkId);
            if (request.SubscriptionId.HasValue)
                query = query.Where(t => t.SubscriptionId == request.SubscriptionId);
            if (request.InvoiceId.HasValue)
                query = query.Where(t => t.InvoiceId == request.InvoiceId);

            query = ApplySorting(query, request.SortBy, request.SortDirection);

            var total = await query.CountAsync();
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponse<TransactionResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<TransactionSummaryResponse> GetSummaryAsync(int userId, ListTransactionsRequest? request)
        {
            var mid = await GetMidAsync(userId);

            var (from, to) = ResolveDateFilter(
                request?.DateFilter,
                request?.FromDate,
                request?.ToDate);

            var query = _context.Transactions.AsNoTracking()
                .Include(t => t.Refunds)
                .Where(t => t.Mid == mid);

            if (request != null)
            {
                if (request.TransactionId.HasValue)
                    query = query.Where(t => t.TransactionId == request.TransactionId);
                if (!string.IsNullOrEmpty(request.Status))
                    query = query.Where(t => t.Status == request.Status);
                if (!string.IsNullOrEmpty(request.PaymentMode))
                    query = query.Where(t => t.PaymentMode == request.PaymentMode);
                if (!string.IsNullOrEmpty(request.Source))
                    query = query.Where(t => t.Source == request.Source);
                if (!string.IsNullOrEmpty(request.RefundType))
                    query = query.Where(t => t.Refunds.Any(r => r.RefundType == request.RefundType));
                if (from.HasValue)
                    query = query.Where(t => t.CreatedDate >= from);
                if (to.HasValue)
                    query = query.Where(t => t.CreatedDate <= to);
                if (request.MinAmount.HasValue)
                    query = query.Where(t => t.Amount >= request.MinAmount);
                if (request.MaxAmount.HasValue)
                    query = query.Where(t => t.Amount <= request.MaxAmount);
            }

            var list = await query.ToListAsync();
            var total = list.Count;
            var success = list.Count(t => t.Status == "Success");
            var failed = list.Count(t => t.Status == "Failed" || t.Status == "Bounced");
            var pending = list.Count(t => t.Status != "Success" && t.Status != "Failed" && t.Status != "Bounced" && t.Status != "Cancelled");
            var refundedAmount = list.SelectMany(t => t.Refunds ?? Enumerable.Empty<Infrastructure.Entities.Refund>())
                .Where(r => r.Status == "Success").Sum(r => r.Amount ?? 0);

            return new TransactionSummaryResponse
            {
                TotalPayments = list.Sum(t => t.Amount ?? 0),
                NumberOfTransactions = total,
                SuccessCount = success,
                SuccessRate = total == 0 ? 0 : Math.Round((decimal)success / total * 100, 2),
                FailedCount = failed,
                PendingCount = pending,
                RefundedAmount = refundedAmount
            };
        }

        public async Task<TransactionResponse?> GetAsync(int userId, long transactionId)
        {
            var mid = await GetMidAsync(userId);
            var tx = await _context.Transactions.AsNoTracking()
                .Include(t => t.TransactionCharges)
                .Include(t => t.Refunds)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.Mid == mid);
            return tx == null ? null : MapToResponse(tx);
        }

        public async Task<List<TransactionChargeDetailResponse>> GetChargesAsync(int userId, long transactionId)
        {
            var mid = await GetMidAsync(userId);
            return await _context.TransactionCharges.AsNoTracking()
                .Where(c => c.TransactionId == transactionId && c.Mid == mid)
                .Select(c => new TransactionChargeDetailResponse
                {
                    TransactionChargeId = c.TransactionChargeId,
                    TransactionId = c.TransactionId,
                    Mid = c.Mid,
                    PaymentMethodType = c.PaymentMethodType,
                    NetworkName = c.NetworkName,
                    ChargeType = c.ChargeType,
                    ChargeValue = c.ChargeValue,
                    TransactionAmount = c.TransactionAmount,
                    ChargeAmount = c.ChargeAmount,
                    GstAmount = c.GstAmount,
                    TotalDeduction = c.TotalDeduction,
                    NetAmount = c.NetAmount,
                    CreatedDate = c.CreatedDate
                })
                .ToListAsync();
        }

        public async Task<List<PaymentMethodChargeResponse>> GetMdrRatesAsync()
        {
            return await _context.PaymentMethodCharges.AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.PaymentMethodType).ThenBy(c => c.NetworkName)
                .Select(c => new PaymentMethodChargeResponse
                {
                    PaymentMethodChargeId = c.PaymentMethodChargeId,
                    PaymentMethodType = c.PaymentMethodType,
                    NetworkName = c.NetworkName,
                    ChargeType = c.ChargeType,
                    ChargeValue = c.ChargeValue,
                    MinCharge = c.MinCharge,
                    MaxCharge = c.MaxCharge,
                    GstPercentage = c.GstPercentage,
                    IsActive = c.IsActive
                })
                .ToListAsync();
        }

        private static TransactionResponse MapToResponse(Infrastructure.Entities.Transaction t)
        {
            var charge = t.TransactionCharges?.FirstOrDefault();
            var refunds = t.Refunds?.ToList() ?? new List<Infrastructure.Entities.Refund>();
            var refundedAmount = refunds.Where(r => r.Status == "Success").Sum(r => r.Amount ?? 0);
            var refundType = refunds.Any() ? (refunds.FirstOrDefault()?.RefundType ?? "full") : null;

            return new TransactionResponse
            {
                TransactionId = t.TransactionId,
                Mid = t.Mid,
                PayuId = t.PayuId,
                MerchantReferenceId = t.MerchantReferenceId,
                CustomerEmail = t.CustomerEmail,
                CustomerPhone = t.CustomerPhone,
                CustomerName = t.CustomerName,
                PaymentMode = t.PaymentMode,
                Source = t.Source,
                Amount = t.Amount,
                Status = t.Status,
                UpiReference = t.UpiReference,
                BankReference = t.BankReference,
                PaymentAggregator = refunds.FirstOrDefault()?.PaymentAggregator,
                TransactionDate = t.TransactionDate,
                OrderId = t.OrderId,
                PaymentLinkId = t.PaymentLinkId,
                SubscriptionId = t.SubscriptionId,
                InvoiceId = t.InvoiceId,
                CreatedDate = t.CreatedDate,
                UpdatedDate = t.UpdatedDate,
                RefundCount = refunds.Count(r => r.Status == "Success"),
                RefundType = refundType,
                RefundedAmount = refundedAmount,
                ChargeSummary = charge == null ? null : new TransactionChargeSummary
                {
                    PaymentMethodType = charge.PaymentMethodType,
                    ChargeType = charge.ChargeType,
                    ChargeValue = charge.ChargeValue,
                    MdrAmount = charge.ChargeAmount,
                    GstAmount = charge.GstAmount,
                    TotalDeduction = charge.TotalDeduction,
                    NetAmount = charge.NetAmount
                }
            };
        }

        private static (DateTime? From, DateTime? To) ResolveDateFilter(string? dateFilter, DateTime? fromDate, DateTime? toDate)
        {
            var now = DateTime.UtcNow;
            return dateFilter?.ToLower() switch
            {
                "today" => (now.Date, now.Date.AddDays(1).AddTicks(-1)),
                "yesterday" => (now.Date.AddDays(-1), now.Date.AddTicks(-1)),
                "last1hour" => (now.AddHours(-1), now),
                "last7days" => (now.Date.AddDays(-7), now),
                "last30days" => (now.Date.AddDays(-30), now),
                "custom" => (fromDate, toDate),
                _ => (fromDate, toDate)
            };
        }

        private static IQueryable<Infrastructure.Entities.Transaction> ApplySorting(
            IQueryable<Infrastructure.Entities.Transaction> query, string? sortBy, string? sortDirection)
        {
            var desc = string.IsNullOrEmpty(sortDirection) || sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
            return sortBy?.ToLower() switch
            {
                "amount" => desc ? query.OrderByDescending(t => t.Amount) : query.OrderBy(t => t.Amount),
                "status" => desc ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
                "paymentmode" => desc ? query.OrderByDescending(t => t.PaymentMode) : query.OrderBy(t => t.PaymentMode),
                "email" => desc ? query.OrderByDescending(t => t.CustomerEmail) : query.OrderBy(t => t.CustomerEmail),
                "merchantreferenceid" => desc ? query.OrderByDescending(t => t.MerchantReferenceId) : query.OrderBy(t => t.MerchantReferenceId),
                "transactiondate" => desc ? query.OrderByDescending(t => t.TransactionDate) : query.OrderBy(t => t.TransactionDate),
                _ => desc ? query.OrderByDescending(t => t.CreatedDate) : query.OrderBy(t => t.CreatedDate)
            };
        }
    }
}
