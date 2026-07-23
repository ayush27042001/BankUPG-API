using BankUPG.Application.Interfaces.MerchantPaymentMethod;
using BankUPG.Infrastructure.Data;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.MerchantPaymentMethod
{
    public class MerchantPaymentMethodService : IMerchantPaymentMethodService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<MerchantPaymentMethodService> _logger;

        public MerchantPaymentMethodService(AppDBContext context, ILogger<MerchantPaymentMethodService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MerchantPaymentMethodResponse> CreateAsync(CreateMerchantPaymentMethodRequest request)
        {
            var entity = new Infrastructure.Entities.MerchantPaymentMethod
            {
                Mid = request.Mid,
                PaymentMethodType = request.PaymentMethodType,
                SubMethodName = request.SubMethodName,
                IsActive = request.IsActive ?? true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.MerchantPaymentMethods.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Merchant payment method created for MID {Mid}", request.Mid);
            return MapToResponse(entity);
        }

        public async Task<MerchantPaymentMethodResponse?> UpdateAsync(int merchantPaymentMethodId, UpdateMerchantPaymentMethodRequest request)
        {
            var entity = await _context.MerchantPaymentMethods.FindAsync(merchantPaymentMethodId);
            if (entity == null) return null;

            entity.Mid = request.Mid;
            entity.PaymentMethodType = request.PaymentMethodType;
            entity.SubMethodName = request.SubMethodName;
            entity.IsActive = request.IsActive ?? entity.IsActive;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToResponse(entity);
        }

        public async Task<MerchantPaymentMethodResponse?> GetAsync(int merchantPaymentMethodId)
        {
            var entity = await _context.MerchantPaymentMethods.AsNoTracking()
                .FirstOrDefaultAsync(m => m.MerchantPaymentMethodId == merchantPaymentMethodId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<PagedResponse<MerchantPaymentMethodResponse>> ListByMidAsync(int mid, int pageNumber, int pageSize)
        {
            var query = _context.MerchantPaymentMethods.AsNoTracking().Where(m => m.Mid == mid);
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(m => m.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(m => MapToResponse(m))
                .ToListAsync();

            return new PagedResponse<MerchantPaymentMethodResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> DeleteAsync(int merchantPaymentMethodId)
        {
            var entity = await _context.MerchantPaymentMethods.FindAsync(merchantPaymentMethodId);
            if (entity == null) return false;

            _context.MerchantPaymentMethods.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static MerchantPaymentMethodResponse MapToResponse(Infrastructure.Entities.MerchantPaymentMethod m) => new()
        {
            MerchantPaymentMethodId = m.MerchantPaymentMethodId,
            Mid = m.Mid,
            PaymentMethodType = m.PaymentMethodType,
            SubMethodName = m.SubMethodName,
            IsActive = m.IsActive,
            CreatedDate = m.CreatedDate,
            UpdatedDate = m.UpdatedDate
        };
    }
}
