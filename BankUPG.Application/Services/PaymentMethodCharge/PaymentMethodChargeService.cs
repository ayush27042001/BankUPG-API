using BankUPG.Application.Interfaces.PaymentMethodCharge;
using BankUPG.Infrastructure.Data;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.PaymentMethodCharge
{
    public class PaymentMethodChargeService : IPaymentMethodChargeService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<PaymentMethodChargeService> _logger;

        public PaymentMethodChargeService(AppDBContext context, ILogger<PaymentMethodChargeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PaymentMethodChargeResponse> CreateAsync(CreatePaymentMethodChargeRequest request)
        {
            var entity = new Infrastructure.Entities.PaymentMethodCharge
            {
                PaymentMethodType = request.PaymentMethodType,
                NetworkName = request.NetworkName,
                ChargeType = request.ChargeType,
                ChargeValue = request.ChargeValue,
                MinCharge = request.MinCharge,
                MaxCharge = request.MaxCharge,
                GstPercentage = request.GstPercentage,
                IsActive = request.IsActive,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.PaymentMethodCharges.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment method charge {PaymentMethodChargeId} created", entity.PaymentMethodChargeId);
            return MapToResponse(entity);
        }

        public async Task<PaymentMethodChargeResponse?> UpdateAsync(int paymentMethodChargeId, CreatePaymentMethodChargeRequest request)
        {
            var entity = await _context.PaymentMethodCharges.FindAsync(paymentMethodChargeId);
            if (entity == null) return null;

            entity.PaymentMethodType = request.PaymentMethodType;
            entity.NetworkName = request.NetworkName;
            entity.ChargeType = request.ChargeType;
            entity.ChargeValue = request.ChargeValue;
            entity.MinCharge = request.MinCharge;
            entity.MaxCharge = request.MaxCharge;
            entity.GstPercentage = request.GstPercentage;
            entity.IsActive = request.IsActive;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToResponse(entity);
        }

        public async Task<PaymentMethodChargeResponse?> GetAsync(int paymentMethodChargeId)
        {
            var entity = await _context.PaymentMethodCharges.AsNoTracking()
                .FirstOrDefaultAsync(c => c.PaymentMethodChargeId == paymentMethodChargeId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<PagedResponse<PaymentMethodChargeResponse>> ListAsync(int pageNumber, int pageSize)
        {
            var query = _context.PaymentMethodCharges.AsNoTracking();
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => MapToResponse(c))
                .ToListAsync();

            return new PagedResponse<PaymentMethodChargeResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> DeleteAsync(int paymentMethodChargeId)
        {
            var entity = await _context.PaymentMethodCharges.FindAsync(paymentMethodChargeId);
            if (entity == null) return false;

            _context.PaymentMethodCharges.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static PaymentMethodChargeResponse MapToResponse(Infrastructure.Entities.PaymentMethodCharge c) => new()
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
        };
    }
}
