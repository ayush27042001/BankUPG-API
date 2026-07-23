using BankUPG.Application.Interfaces.PaymentOrder;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.PaymentOrder
{
    public class PaymentOrderService : IPaymentOrderService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<PaymentOrderService> _logger;

        public PaymentOrderService(AppDBContext context, ILogger<PaymentOrderService> logger)
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

        public async Task<PaymentOrderResponse> CreateOrderAsync(int userId, CreatePaymentOrderRequest request)
        {
            var mid = await GetMidAsync(userId);

            var order = new Infrastructure.Entities.PaymentOrder
            {
                Mid = mid,
                OrderRef = request.OrderRef,
                Amount = request.Amount,
                Currency = request.Currency ?? "INR",
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                CustomerName = request.CustomerName,
                Notes = request.Notes,
                Status = "created",
                ExpiryDate = request.ExpiryDate ?? DateTime.UtcNow.AddMinutes(30),
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.PaymentOrders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("PaymentOrder {OrderId} created for MID {Mid}", order.PaymentOrderId, mid);
            return MapToResponse(order);
        }

        public async Task<PaymentOrderResponse?> GetOrderAsync(int userId, long orderId)
        {
            var mid = await GetMidAsync(userId);

            var order = await _context.PaymentOrders
                .AsNoTracking()
                .Include(o => o.PaymentAttempts)
                .FirstOrDefaultAsync(o => o.PaymentOrderId == orderId && o.Mid == mid);

            return order == null ? null : MapToResponse(order, includeAttempts: true);
        }

        public async Task<PagedResponse<PaymentOrderResponse>> ListOrdersAsync(int userId, ListPaymentOrdersRequest request)
        {
            var mid = await GetMidAsync(userId);

            var query = _context.PaymentOrders.AsNoTracking()
                .Where(o => o.Mid == mid);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(o => o.Status == request.Status);
            if (request.FromDate.HasValue)
                query = query.Where(o => o.CreatedDate >= request.FromDate);
            if (request.ToDate.HasValue)
                query = query.Where(o => o.CreatedDate <= request.ToDate);

            var total = await query.CountAsync();
            var items = (await query
                .OrderByDescending(o => o.CreatedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync())
                .Select(o => MapToResponse(o))
                .ToList();

            return new PagedResponse<PaymentOrderResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<bool> CancelOrderAsync(int userId, long orderId)
        {
            var mid = await GetMidAsync(userId);

            var order = await _context.PaymentOrders
                .FirstOrDefaultAsync(o => o.PaymentOrderId == orderId && o.Mid == mid);

            if (order == null || order.Status == "paid")
                return false;

            order.Status = "expired";
            order.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PaymentAttemptResponse>> GetOrderAttemptsAsync(int userId, long orderId)
        {
            var mid = await GetMidAsync(userId);

            return await _context.PaymentAttempts.AsNoTracking()
                .Where(a => a.OrderId == orderId && a.Mid == mid)
                .OrderByDescending(a => a.CreatedDate)
                .Select(a => new PaymentAttemptResponse
                {
                    PaymentAttemptId = a.PaymentAttemptId,
                    OrderId = a.OrderId,
                    TransactionId = a.TransactionId,
                    PaymentMode = a.PaymentMode,
                    Amount = a.Amount,
                    Status = a.Status,
                    FailureReason = a.FailureReason,
                    AttemptDate = a.AttemptDate,
                    CreatedDate = a.CreatedDate
                })
                .ToListAsync();
        }

        private static PaymentOrderResponse MapToResponse(Infrastructure.Entities.PaymentOrder o, bool includeAttempts = false)
        {
            return new PaymentOrderResponse
            {
                PaymentOrderId = o.PaymentOrderId,
                Mid = o.Mid,
                OrderRef = o.OrderRef,
                Amount = o.Amount,
                Currency = o.Currency,
                CustomerEmail = o.CustomerEmail,
                CustomerPhone = o.CustomerPhone,
                CustomerName = o.CustomerName,
                Notes = o.Notes,
                Status = o.Status,
                ExpiryDate = o.ExpiryDate,
                PaidDate = o.PaidDate,
                CreatedDate = o.CreatedDate,
                UpdatedDate = o.UpdatedDate,
                Attempts = includeAttempts ? o.PaymentAttempts?.Select(a => new PaymentAttemptResponse
                {
                    PaymentAttemptId = a.PaymentAttemptId,
                    OrderId = a.OrderId,
                    TransactionId = a.TransactionId,
                    PaymentMode = a.PaymentMode,
                    Amount = a.Amount,
                    Status = a.Status,
                    FailureReason = a.FailureReason,
                    AttemptDate = a.AttemptDate,
                    CreatedDate = a.CreatedDate
                }).ToList() : null
            };
        }
    }
}
