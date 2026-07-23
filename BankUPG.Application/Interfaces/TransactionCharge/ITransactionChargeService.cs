using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.TransactionCharge
{
    public interface ITransactionChargeService
    {
        Task<TransactionChargeResponse> CreateAsync(CreateTransactionChargeRequest request);
        Task<TransactionChargeResponse?> UpdateAsync(long transactionChargeId, UpdateTransactionChargeRequest request);
        Task<TransactionChargeResponse?> GetAsync(long transactionChargeId);
        Task<TransactionChargeResponse?> RecalculateAsync(long transactionId);
        Task<bool> DeleteAsync(long transactionChargeId);
    }
}
