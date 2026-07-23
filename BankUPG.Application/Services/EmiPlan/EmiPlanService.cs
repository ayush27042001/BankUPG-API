using BankUPG.Application.Interfaces.EmiPlan;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.EmiPlan
{
    public class EmiPlanService : IEmiPlanService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<EmiPlanService> _logger;

        public EmiPlanService(AppDBContext context, ILogger<EmiPlanService> logger)
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

        public async Task<EmiPlanResponse> CreateEmiPlanAsync(int userId, CreateEmiPlanRequest request)
        {
            var mid = await GetMidAsync(userId);

            var emi = new Infrastructure.Entities.EmiPlan
            {
                Mid = mid,
                BankName = request.BankName,
                CardType = request.CardType,
                Tenure = request.Tenure,
                InterestRate = request.InterestRate,
                MinAmount = request.MinAmount,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.EmiPlans.Add(emi);
            await _context.SaveChangesAsync();
            return MapToResponse(emi);
        }

        public async Task<List<EmiPlanResponse>> ListEmiPlansAsync(int userId)
        {
            var mid = await GetMidAsync(userId);
            return await _context.EmiPlans.AsNoTracking()
                .Where(e => e.Mid == mid)
                .OrderBy(e => e.BankName).ThenBy(e => e.Tenure)
                .Select(e => MapToResponse(e))
                .ToListAsync();
        }

        public async Task<EmiPlanResponse?> UpdateEmiPlanAsync(int userId, int emiPlanId, UpdateEmiPlanRequest request)
        {
            var mid = await GetMidAsync(userId);
            var emi = await _context.EmiPlans
                .FirstOrDefaultAsync(e => e.EmiPlanId == emiPlanId && e.Mid == mid);

            if (emi == null) return null;

            if (request.InterestRate.HasValue) emi.InterestRate = request.InterestRate.Value;
            if (request.MinAmount.HasValue) emi.MinAmount = request.MinAmount.Value;
            if (request.IsActive.HasValue) emi.IsActive = request.IsActive.Value;
            emi.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToResponse(emi);
        }

        public async Task<bool> DeactivateEmiPlanAsync(int userId, int emiPlanId)
        {
            var mid = await GetMidAsync(userId);
            var emi = await _context.EmiPlans
                .FirstOrDefaultAsync(e => e.EmiPlanId == emiPlanId && e.Mid == mid);

            if (emi == null) return false;

            emi.IsActive = false;
            emi.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private static EmiPlanResponse MapToResponse(Infrastructure.Entities.EmiPlan e) => new()
        {
            EmiPlanId = e.EmiPlanId,
            Mid = e.Mid,
            BankName = e.BankName,
            CardType = e.CardType,
            Tenure = e.Tenure,
            InterestRate = e.InterestRate,
            MinAmount = e.MinAmount,
            IsActive = e.IsActive,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        };
    }
}
