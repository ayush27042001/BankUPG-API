using BankUPG.Application.Interfaces.SubscriptionPlan;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.SubscriptionPlan
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<SubscriptionPlanService> _logger;

        public SubscriptionPlanService(AppDBContext context, ILogger<SubscriptionPlanService> logger)
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

        public async Task<SubscriptionPlanResponse> CreatePlanAsync(int userId, CreateSubscriptionPlanRequest request)
        {
            var mid = await GetMidAsync(userId);

            var plan = new Infrastructure.Entities.SubscriptionPlan
            {
                Mid = mid,
                PlanName = request.PlanName,
                Amount = request.Amount,
                Currency = request.Currency ?? "INR",
                Interval = request.Interval,
                IntervalCount = request.IntervalCount,
                TotalBillingCycles = request.TotalBillingCycles,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.SubscriptionPlans.Add(plan);
            await _context.SaveChangesAsync();
            return MapToResponse(plan);
        }

        public async Task<SubscriptionPlanResponse?> GetPlanAsync(int userId, int planId)
        {
            var mid = await GetMidAsync(userId);
            var plan = await _context.SubscriptionPlans.AsNoTracking()
                .FirstOrDefaultAsync(p => p.SubscriptionPlanId == planId && p.Mid == mid);
            return plan == null ? null : MapToResponse(plan);
        }

        public async Task<List<SubscriptionPlanResponse>> ListPlansAsync(int userId)
        {
            var mid = await GetMidAsync(userId);
            return await _context.SubscriptionPlans.AsNoTracking()
                .Where(p => p.Mid == mid)
                .OrderByDescending(p => p.CreatedDate)
                .Select(p => MapToResponse(p))
                .ToListAsync();
        }

        public async Task<SubscriptionPlanResponse?> UpdatePlanAsync(int userId, int planId, UpdateSubscriptionPlanRequest request)
        {
            var mid = await GetMidAsync(userId);
            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.SubscriptionPlanId == planId && p.Mid == mid);

            if (plan == null) return null;

            if (!string.IsNullOrEmpty(request.PlanName))
                plan.PlanName = request.PlanName;
            if (request.IsActive.HasValue)
                plan.IsActive = request.IsActive.Value;
            plan.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToResponse(plan);
        }

        public async Task<bool> DeactivatePlanAsync(int userId, int planId)
        {
            var mid = await GetMidAsync(userId);
            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.SubscriptionPlanId == planId && p.Mid == mid);

            if (plan == null) return false;

            plan.IsActive = false;
            plan.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private static SubscriptionPlanResponse MapToResponse(Infrastructure.Entities.SubscriptionPlan p) => new()
        {
            SubscriptionPlanId = p.SubscriptionPlanId,
            Mid = p.Mid,
            PlanName = p.PlanName,
            Amount = p.Amount,
            Currency = p.Currency,
            Interval = p.Interval,
            IntervalCount = p.IntervalCount,
            TotalBillingCycles = p.TotalBillingCycles,
            IsActive = p.IsActive,
            CreatedDate = p.CreatedDate,
            UpdatedDate = p.UpdatedDate
        };
    }
}
