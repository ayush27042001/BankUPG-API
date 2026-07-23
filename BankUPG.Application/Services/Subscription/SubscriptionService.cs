using BankUPG.Application.Interfaces.Subscription;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.Subscription
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(AppDBContext context, ILogger<SubscriptionService> logger)
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

        public async Task<SubscriptionResponse> CreateSubscriptionAsync(int userId, CreateSubscriptionRequest request)
        {
            var mid = await GetMidAsync(userId);

            var plan = await _context.SubscriptionPlans.AsNoTracking()
                .FirstOrDefaultAsync(p => p.SubscriptionPlanId == request.PlanId && p.Mid == mid && p.IsActive);
            if (plan == null)
                throw new ArgumentException("Subscription plan not found or inactive.");

            var subscription = new Infrastructure.Entities.Subscription
            {
                Mid = mid,
                PlanId = request.PlanId,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                CustomerName = request.CustomerName,
                Status = "created",
                CurrentCycle = 0,
                TotalCycles = request.TotalCycles ?? plan.TotalBillingCycles,
                NextBillingDate = ComputeNextBillingDate(plan.Interval, plan.IntervalCount),
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Subscription {SubId} created for MID {Mid}, Plan {PlanId}", subscription.SubscriptionId, mid, plan.SubscriptionPlanId);
            return MapToResponse(subscription, plan.PlanName);
        }

        public async Task<SubscriptionResponse?> GetSubscriptionAsync(int userId, long subscriptionId)
        {
            var mid = await GetMidAsync(userId);
            var sub = await _context.Subscriptions.AsNoTracking()
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId && s.Mid == mid);
            return sub == null ? null : MapToResponse(sub, sub.Plan?.PlanName);
        }

        public async Task<PagedResponse<SubscriptionResponse>> ListSubscriptionsAsync(int userId, ListSubscriptionsRequest request)
        {
            var mid = await GetMidAsync(userId);

            var query = _context.Subscriptions.AsNoTracking()
                .Include(s => s.Plan)
                .Where(s => s.Mid == mid);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(s => s.Status == request.Status);
            if (request.PlanId.HasValue)
                query = query.Where(s => s.PlanId == request.PlanId);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(s => s.CreatedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => MapToResponse(s, s.Plan != null ? s.Plan.PlanName : null))
                .ToListAsync();

            return new PagedResponse<SubscriptionResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<bool> CancelSubscriptionAsync(int userId, long subscriptionId)
        {
            var mid = await GetMidAsync(userId);
            var sub = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId && s.Mid == mid);

            if (sub == null || sub.Status is "cancelled" or "completed" or "expired") return false;

            sub.Status = "cancelled";
            sub.EndDate = DateTime.UtcNow;
            sub.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private static DateTime ComputeNextBillingDate(string? interval, int count) => interval?.ToLower() switch
        {
            "daily" => DateTime.UtcNow.AddDays(count),
            "weekly" => DateTime.UtcNow.AddDays(7 * count),
            "yearly" => DateTime.UtcNow.AddYears(count),
            _ => DateTime.UtcNow.AddMonths(count)
        };

        private static SubscriptionResponse MapToResponse(Infrastructure.Entities.Subscription s, string? planName) => new()
        {
            SubscriptionId = s.SubscriptionId,
            Mid = s.Mid,
            PlanId = s.PlanId,
            PlanName = planName,
            CustomerEmail = s.CustomerEmail,
            CustomerPhone = s.CustomerPhone,
            CustomerName = s.CustomerName,
            Status = s.Status,
            CurrentCycle = s.CurrentCycle,
            TotalCycles = s.TotalCycles,
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            NextBillingDate = s.NextBillingDate,
            UpiMandateRef = s.UpiMandateRef,
            NachMandateRef = s.NachMandateRef,
            CreatedDate = s.CreatedDate,
            UpdatedDate = s.UpdatedDate
        };
    }
}
