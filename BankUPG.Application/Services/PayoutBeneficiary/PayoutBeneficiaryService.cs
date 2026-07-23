using BankUPG.Application.Interfaces.PayoutBeneficiary;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.PayoutBeneficiary
{
    public class PayoutBeneficiaryService : IPayoutBeneficiaryService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<PayoutBeneficiaryService> _logger;

        public PayoutBeneficiaryService(AppDBContext context, ILogger<PayoutBeneficiaryService> logger)
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

        public async Task<PayoutBeneficiaryResponse> CreateBeneficiaryAsync(int userId, CreatePayoutBeneficiaryRequest request)
        {
            var mid = await GetMidAsync(userId);

            var beneficiary = new Infrastructure.Entities.PayoutBeneficiary
            {
                Mid = mid,
                BeneficiaryName = request.BeneficiaryName,
                AccountNumber = request.AccountNumber,
                Ifsccode = request.Ifsccode,
                BankName = request.BankName,
                AccountType = request.AccountType,
                UpiId = request.UpiId,
                Email = request.Email,
                Phone = request.Phone,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.PayoutBeneficiaries.Add(beneficiary);
            await _context.SaveChangesAsync();
            return MapToResponse(beneficiary);
        }

        public async Task<PayoutBeneficiaryResponse?> GetBeneficiaryAsync(int userId, long beneficiaryId)
        {
            var mid = await GetMidAsync(userId);
            var b = await _context.PayoutBeneficiaries.AsNoTracking()
                .FirstOrDefaultAsync(b => b.PayoutBeneficiaryId == beneficiaryId && b.Mid == mid);
            return b == null ? null : MapToResponse(b);
        }

        public async Task<PagedResponse<PayoutBeneficiaryResponse>> ListBeneficiariesAsync(int userId, int pageNumber, int pageSize)
        {
            var mid = await GetMidAsync(userId);

            var query = _context.PayoutBeneficiaries.AsNoTracking()
                .Where(b => b.Mid == mid && b.IsActive);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(b => b.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => MapToResponse(b))
                .ToListAsync();

            return new PagedResponse<PayoutBeneficiaryResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> DeactivateBeneficiaryAsync(int userId, long beneficiaryId)
        {
            var mid = await GetMidAsync(userId);
            var b = await _context.PayoutBeneficiaries
                .FirstOrDefaultAsync(b => b.PayoutBeneficiaryId == beneficiaryId && b.Mid == mid);

            if (b == null) return false;

            b.IsActive = false;
            b.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private static PayoutBeneficiaryResponse MapToResponse(Infrastructure.Entities.PayoutBeneficiary b) => new()
        {
            PayoutBeneficiaryId = b.PayoutBeneficiaryId,
            Mid = b.Mid,
            BeneficiaryName = b.BeneficiaryName,
            AccountNumber = b.AccountNumber,
            Ifsccode = b.Ifsccode,
            BankName = b.BankName,
            AccountType = b.AccountType,
            UpiId = b.UpiId,
            Email = b.Email,
            Phone = b.Phone,
            IsActive = b.IsActive,
            CreatedDate = b.CreatedDate
        };
    }
}
