using BankUPG.Application.Interfaces.Refund;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.Refund
{
    public class RefundService : IRefundService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<RefundService> _logger;

        public RefundService(AppDBContext context, ILogger<RefundService> logger)
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

        public async Task<PagedResponse<RefundDetailResponse>> ListAsync(int userId, ListRefundsRequest request)
        {
            var mid = await GetMidAsync(userId);

            var query = _context.Refunds.AsNoTracking().Where(r => r.Mid == mid);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(r => r.Status == request.Status);
            if (!string.IsNullOrEmpty(request.RefundType))
                query = query.Where(r => r.RefundType == request.RefundType);
            if (request.FromDate.HasValue)
                query = query.Where(r => r.CreatedDate >= request.FromDate);
            if (request.ToDate.HasValue)
                query = query.Where(r => r.CreatedDate <= request.ToDate);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(r => MapToResponse(r))
                .ToListAsync();

            return new PagedResponse<RefundDetailResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<RefundDetailResponse?> GetAsync(int userId, long refundId)
        {
            var mid = await GetMidAsync(userId);
            var r = await _context.Refunds.AsNoTracking()
                .FirstOrDefaultAsync(r => r.RefundId == refundId && r.Mid == mid);
            return r == null ? null : MapToResponse(r);
        }

        public async Task<RefundDetailResponse> InitiateRefundAsync(int userId, InitiateRefundRequest request)
        {
            var mid = await GetMidAsync(userId);

            var transaction = await _context.Transactions.AsNoTracking()
                .Include(t => t.Refunds)
                .FirstOrDefaultAsync(t => t.TransactionId == request.TransactionId && t.Mid == mid);

            if (transaction == null)
                throw new ArgumentException("Transaction not found.");
            if (transaction.Status != "Success")
                throw new ArgumentException("Only successful transactions can be refunded.");

            var alreadyRefunded = transaction.Refunds?.Where(r => r.Status == "Success").Sum(r => r.Amount ?? 0) ?? 0;
            if (alreadyRefunded + request.Amount > (transaction.Amount ?? 0))
                throw new ArgumentException($"Refund amount exceeds refundable balance. Already refunded: ₹{alreadyRefunded}");

            var refund = new Infrastructure.Entities.Refund
            {
                Mid = mid,
                TransactionId = request.TransactionId,
                MerchantReferenceId = request.MerchantReferenceId,
                RefundType = request.RefundType ?? "full",
                Amount = request.Amount,
                Status = "pending",
                Source = "merchant_dashboard",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Refunds.Add(refund);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Refund {RefundId} initiated for Transaction {TransactionId}, Amount ₹{Amount}", refund.RefundId, request.TransactionId, request.Amount);
            return MapToResponse(refund);
        }

        private static RefundDetailResponse MapToResponse(Infrastructure.Entities.Refund r) => new()
        {
            RefundId = r.RefundId,
            Mid = r.Mid,
            TransactionId = r.TransactionId,
            PayuId = r.PayuId,
            MerchantReferenceId = r.MerchantReferenceId,
            RefundType = r.RefundType,
            Source = r.Source,
            BankArn = r.BankArn,
            Amount = r.Amount,
            Status = r.Status,
            PaymentAggregator = r.PaymentAggregator,
            RefundDate = r.RefundDate,
            CreatedDate = r.CreatedDate,
            UpdatedDate = r.UpdatedDate
        };
    }
}
