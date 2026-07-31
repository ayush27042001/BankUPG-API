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
            var transaction = await _context.Transactions
                .Include(t => t.TransactionCharges)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            if (transaction == null) return null;

            var existing = transaction.TransactionCharges.FirstOrDefault();
            var paymentMode = transaction.PaymentMode;
            var networkName = existing?.NetworkName;

            var charge = await _context.PaymentMethodCharges
                .Where(c => c.IsActive
                    && c.PaymentMethodType == paymentMode
                    && c.NetworkName == networkName)
                .FirstOrDefaultAsync()
                ?? await _context.PaymentMethodCharges
                .Where(c => c.IsActive
                    && c.PaymentMethodType == paymentMode
                    && c.NetworkName == null)
                .FirstOrDefaultAsync();

            if (charge == null)
            {
                _logger.LogWarning("No active PaymentMethodCharge for mode '{Mode}' / network '{Network}'.", paymentMode, networkName);
                return null;
            }

            var amount = transaction.Amount ?? 0m;
            decimal chargeAmount;

            if (string.Equals(charge.ChargeType, "Percentage", StringComparison.OrdinalIgnoreCase))
                chargeAmount = Math.Round(amount * charge.ChargeValue / 100m, 2);
            else
                chargeAmount = charge.ChargeValue;

            if (charge.MinCharge.HasValue && chargeAmount < charge.MinCharge.Value) chargeAmount = charge.MinCharge.Value;
            if (charge.MaxCharge.HasValue && chargeAmount > charge.MaxCharge.Value) chargeAmount = charge.MaxCharge.Value;

            var gstAmount   = Math.Round(chargeAmount * charge.GstPercentage / 100m, 2);
            var totalDeduct = chargeAmount + gstAmount;
            var netAmount   = amount - totalDeduct;

            if (existing != null)
            {
                existing.PaymentMethodChargeId = charge.PaymentMethodChargeId;
                existing.ChargeType    = charge.ChargeType;
                existing.ChargeValue   = charge.ChargeValue;
                existing.ChargeAmount  = chargeAmount;
                existing.GstAmount     = gstAmount;
                existing.TotalDeduction = totalDeduct;
                existing.NetAmount     = netAmount;
            }
            else
            {
                existing = new Infrastructure.Entities.TransactionCharge
                {
                    TransactionId         = transactionId,
                    Mid                   = transaction.Mid,
                    PaymentMethodChargeId = charge.PaymentMethodChargeId,
                    PaymentMethodType     = paymentMode,
                    NetworkName           = networkName,
                    ChargeType            = charge.ChargeType,
                    ChargeValue           = charge.ChargeValue,
                    TransactionAmount     = amount,
                    ChargeAmount          = chargeAmount,
                    GstAmount             = gstAmount,
                    TotalDeduction        = totalDeduct,
                    NetAmount             = netAmount,
                    CreatedDate           = DateTime.UtcNow
                };
                _context.TransactionCharges.Add(existing);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("TransactionCharge recalculated for Txn={TxnId}, Net={Net}", transactionId, netAmount);
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
