using BankUPG.Application.Interfaces.Payout;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.Payout
{
    public class PayoutService : IPayoutService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<PayoutService> _logger;

        public PayoutService(AppDBContext context, ILogger<PayoutService> logger)
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

        public async Task<PayoutResponse> CreatePayoutAsync(int userId, CreatePayoutRequest request)
        {
            var mid = await GetMidAsync(userId);

            if (request.BeneficiaryId.HasValue)
            {
                var beneficiaryExists = await _context.PayoutBeneficiaries.AnyAsync(
                    b => b.PayoutBeneficiaryId == request.BeneficiaryId && b.Mid == mid && b.IsActive);
                if (!beneficiaryExists)
                    throw new ArgumentException("Beneficiary not found or inactive.");
            }

            var payout = new Infrastructure.Entities.Payout
            {
                Mid = mid,
                BeneficiaryId = request.BeneficiaryId,
                Amount = request.Amount,
                Currency = request.Currency ?? "INR",
                Mode = request.Mode,
                Status = "queued",
                ReferenceId = request.ReferenceId,
                Narration = request.Narration,
                ScheduledDate = request.ScheduledDate,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Payouts.Add(payout);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payout {PayoutId} queued for MID {Mid}, Amount {Amount}", payout.PayoutId, mid, payout.Amount);
            return await MapToResponseAsync(payout);
        }

        public async Task<PayoutResponse?> GetPayoutAsync(int userId, long payoutId)
        {
            var mid = await GetMidAsync(userId);
            var payout = await _context.Payouts.AsNoTracking()
                .Include(p => p.Beneficiary)
                .FirstOrDefaultAsync(p => p.PayoutId == payoutId && p.Mid == mid);
            return payout == null ? null : MapToResponse(payout);
        }

        public async Task<PagedResponse<PayoutResponse>> ListPayoutsAsync(int userId, ListPayoutsRequest request)
        {
            var mid = await GetMidAsync(userId);

            var query = _context.Payouts.AsNoTracking()
                .Include(p => p.Beneficiary)
                .Where(p => p.Mid == mid);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(p => p.Status == request.Status);
            if (!string.IsNullOrEmpty(request.Mode))
                query = query.Where(p => p.Mode == request.Mode);
            if (request.FromDate.HasValue)
                query = query.Where(p => p.CreatedDate >= request.FromDate);
            if (request.ToDate.HasValue)
                query = query.Where(p => p.CreatedDate <= request.ToDate);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.CreatedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => MapToResponse(p))
                .ToListAsync();

            return new PagedResponse<PayoutResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<bool> CancelPayoutAsync(int userId, long payoutId)
        {
            var mid = await GetMidAsync(userId);
            var payout = await _context.Payouts
                .FirstOrDefaultAsync(p => p.PayoutId == payoutId && p.Mid == mid);

            if (payout == null || payout.Status != "queued") return false;

            payout.Status = "cancelled";
            payout.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<PayoutResponse> MapToResponseAsync(Infrastructure.Entities.Payout p)
        {
            string? beneficiaryName = null;
            if (p.BeneficiaryId.HasValue)
            {
                var b = await _context.PayoutBeneficiaries.AsNoTracking()
                    .FirstOrDefaultAsync(b => b.PayoutBeneficiaryId == p.BeneficiaryId);
                beneficiaryName = b?.BeneficiaryName;
            }
            return new PayoutResponse
            {
                PayoutId = p.PayoutId, Mid = p.Mid, BeneficiaryId = p.BeneficiaryId,
                BeneficiaryName = beneficiaryName, Amount = p.Amount, Currency = p.Currency,
                Mode = p.Mode, Status = p.Status, ReferenceId = p.ReferenceId,
                UtrNumber = p.UtrNumber, Narration = p.Narration, FailureReason = p.FailureReason,
                ScheduledDate = p.ScheduledDate, ProcessedDate = p.ProcessedDate, CreatedDate = p.CreatedDate
            };
        }

        private static PayoutResponse MapToResponse(Infrastructure.Entities.Payout p) => new()
        {
            PayoutId = p.PayoutId, Mid = p.Mid, BeneficiaryId = p.BeneficiaryId,
            BeneficiaryName = p.Beneficiary?.BeneficiaryName, Amount = p.Amount, Currency = p.Currency,
            Mode = p.Mode, Status = p.Status, ReferenceId = p.ReferenceId,
            UtrNumber = p.UtrNumber, Narration = p.Narration, FailureReason = p.FailureReason,
            ScheduledDate = p.ScheduledDate, ProcessedDate = p.ProcessedDate, CreatedDate = p.CreatedDate
        };
    }
}
