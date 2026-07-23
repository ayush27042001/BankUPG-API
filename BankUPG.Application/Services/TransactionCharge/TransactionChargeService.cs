using BankUPG.Application.Interfaces.TransactionCharge;
using BankUPG.Infrastructure.Data;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.TransactionCharge
{
    public class TransactionChargeService : ITransactionChargeService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<TransactionChargeService> _logger;

        public TransactionChargeService(AppDBContext context, ILogger<TransactionChargeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TransactionChargeResponse> CreateAsync(CreateTransactionChargeRequest request)
        {
            var entity = new Infrastructure.Entities.TransactionCharge
            {
                TransactionId = request.TransactionId,
                Mid = request.Mid,
                PaymentMethodChargeId = request.PaymentMethodChargeId,
                PaymentMethodType = request.PaymentMethodType,
                NetworkName = request.NetworkName,
                ChargeType = request.ChargeType,
                ChargeValue = request.ChargeValue,
                TransactionAmount = request.TransactionAmount,
                ChargeAmount = request.ChargeAmount,
                GstAmount = request.GstAmount,
                TotalDeduction = request.TotalDeduction,
                NetAmount = request.NetAmount,
                CreatedDate = DateTime.UtcNow
            };

            _context.TransactionCharges.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Transaction charge {TransactionChargeId} created for tx {TransactionId}", entity.TransactionChargeId, request.TransactionId);
            return MapToResponse(entity);
        }

        public async Task<TransactionChargeResponse?> UpdateAsync(long transactionChargeId, UpdateTransactionChargeRequest request)
        {
            var entity = await _context.TransactionCharges.FindAsync(transactionChargeId);
            if (entity == null) return null;

            entity.TransactionId = request.TransactionId;
            entity.Mid = request.Mid;
            entity.PaymentMethodChargeId = request.PaymentMethodChargeId;
            entity.PaymentMethodType = request.PaymentMethodType;
            entity.NetworkName = request.NetworkName;
            entity.ChargeType = request.ChargeType;
            entity.ChargeValue = request.ChargeValue;
            entity.TransactionAmount = request.TransactionAmount;
            entity.ChargeAmount = request.ChargeAmount;
            entity.GstAmount = request.GstAmount;
            entity.TotalDeduction = request.TotalDeduction;
            entity.NetAmount = request.NetAmount;

            await _context.SaveChangesAsync();
            return MapToResponse(entity);
        }

        public async Task<TransactionChargeResponse?> GetAsync(long transactionChargeId)
        {
            var entity = await _context.TransactionCharges.AsNoTracking()
                .FirstOrDefaultAsync(c => c.TransactionChargeId == transactionChargeId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<TransactionChargeResponse?> RecalculateAsync(long transactionId)
        {
            var transaction = await _context.Transactions.AsNoTracking()
                .Include(t => t.TransactionCharges)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null) return null;

            // TODO: implement actual recalculation using PaymentMethodCharges
            var existing = transaction.TransactionCharges.FirstOrDefault();
            if (existing == null) return null;

            return MapToResponse(existing);
        }

        public async Task<bool> DeleteAsync(long transactionChargeId)
        {
            var entity = await _context.TransactionCharges.FindAsync(transactionChargeId);
            if (entity == null) return false;

            _context.TransactionCharges.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static TransactionChargeResponse MapToResponse(Infrastructure.Entities.TransactionCharge c) => new()
        {
            TransactionChargeId = c.TransactionChargeId,
            TransactionId = c.TransactionId,
            Mid = c.Mid,
            PaymentMethodChargeId = c.PaymentMethodChargeId,
            PaymentMethodType = c.PaymentMethodType,
            NetworkName = c.NetworkName,
            ChargeType = c.ChargeType,
            ChargeValue = c.ChargeValue,
            TransactionAmount = c.TransactionAmount,
            ChargeAmount = c.ChargeAmount,
            GstAmount = c.GstAmount,
            TotalDeduction = c.TotalDeduction,
            NetAmount = c.NetAmount,
            CreatedDate = c.CreatedDate
        };
    }
}
