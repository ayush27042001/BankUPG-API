using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.Transaction
{
    public interface ITransactionService
    {
        Task<PagedResponse<TransactionResponse>> ListAsync(int userId, ListTransactionsRequest request);
        Task<TransactionResponse?> GetAsync(int userId, long transactionId);
        Task<TransactionSummaryResponse> GetSummaryAsync(int userId, ListTransactionsRequest? request);
        Task<List<TransactionChargeDetailResponse>> GetChargesAsync(int userId, long transactionId);
        Task<List<PaymentMethodChargeResponse>> GetMdrRatesAsync();
    }
}
