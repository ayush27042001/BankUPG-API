using BankUPG.Application.Interfaces.BatchRefund;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.BatchRefund
{
    public class BatchRefundService : IBatchRefundService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<BatchRefundService> _logger;

        public BatchRefundService(AppDBContext context, ILogger<BatchRefundService> logger)
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

        public async Task<BatchRefundResponse> CreateBatchRefundAsync(int userId, CreateBatchRefundRequest request)
        {
            var mid = await GetMidAsync(userId);

            var transactionIds = request.Items.Select(i => i.TransactionId).ToList();
            var validTransactions = await _context.Transactions
                .Where(t => transactionIds.Contains(t.TransactionId) && t.Mid == mid && t.Status == "Success")
                .Select(t => t.TransactionId)
                .ToListAsync();

            var invalidItems = request.Items.Where(i => !validTransactions.Contains(i.TransactionId)).ToList();
            if (invalidItems.Any())
                throw new ArgumentException($"Invalid or non-success transactions: {string.Join(", ", invalidItems.Select(i => i.TransactionId))}");

            var batch = new Infrastructure.Entities.BatchRefund
            {
                Mid = mid,
                BatchReferenceId = request.BatchReferenceId ?? $"BATCH-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TotalItems = request.Items.Count,
                TotalAmount = request.Items.Sum(i => i.Amount),
                ProcessedItems = 0,
                SuccessCount = 0,
                FailedCount = 0,
                Status = "pending",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.BatchRefunds.Add(batch);
            await _context.SaveChangesAsync();

            var items = request.Items.Select(i => new Infrastructure.Entities.BatchRefundItem
            {
                BatchRefundId = batch.BatchRefundId,
                TransactionId = i.TransactionId,
                Amount = i.Amount,
                Status = "pending",
                CreatedDate = DateTime.UtcNow
            }).ToList();

            _context.BatchRefundItems.AddRange(items);
            await _context.SaveChangesAsync();

            _logger.LogInformation("BatchRefund {BatchId} created for MID {Mid}, {Count} items", batch.BatchRefundId, mid, items.Count);

            batch.BatchRefundItems = items;
            return MapToResponse(batch, includeItems: true);
        }

        public async Task<BatchRefundResponse?> GetBatchRefundAsync(int userId, long batchRefundId)
        {
            var mid = await GetMidAsync(userId);
            var batch = await _context.BatchRefunds.AsNoTracking()
                .Include(b => b.BatchRefundItems)
                .FirstOrDefaultAsync(b => b.BatchRefundId == batchRefundId && b.Mid == mid);
            return batch == null ? null : MapToResponse(batch, includeItems: true);
        }

        public async Task<PagedResponse<BatchRefundResponse>> ListBatchRefundsAsync(int userId, ListBatchRefundsRequest request)
        {
            var mid = await GetMidAsync(userId);

            var query = _context.BatchRefunds.AsNoTracking().Where(b => b.Mid == mid);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(b => b.Status == request.Status);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(b => b.CreatedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(b => MapToResponse(b, false))
                .ToListAsync();

            return new PagedResponse<BatchRefundResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        private static BatchRefundResponse MapToResponse(Infrastructure.Entities.BatchRefund b, bool includeItems = false) => new()
        {
            BatchRefundId = b.BatchRefundId,
            Mid = b.Mid,
            BatchReferenceId = b.BatchReferenceId,
            TotalItems = b.TotalItems,
            TotalAmount = b.TotalAmount,
            ProcessedItems = b.ProcessedItems,
            SuccessCount = b.SuccessCount,
            FailedCount = b.FailedCount,
            Status = b.Status,
            CreatedDate = b.CreatedDate,
            UpdatedDate = b.UpdatedDate,
            Items = includeItems ? b.BatchRefundItems?.Select(i => new BatchRefundItemResponse
            {
                BatchRefundItemId = i.BatchRefundItemId,
                TransactionId = i.TransactionId,
                RefundId = i.RefundId,
                Amount = i.Amount,
                Status = i.Status,
                FailureReason = i.FailureReason,
                CreatedDate = i.CreatedDate
            }).ToList() : null
        };
    }
}
