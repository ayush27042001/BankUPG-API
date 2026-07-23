using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.BatchRefund
{
    public interface IBatchRefundService
    {
        Task<BatchRefundResponse> CreateBatchRefundAsync(int userId, CreateBatchRefundRequest request);
        Task<BatchRefundResponse?> GetBatchRefundAsync(int userId, long batchRefundId);
        Task<PagedResponse<BatchRefundResponse>> ListBatchRefundsAsync(int userId, ListBatchRefundsRequest request);
    }
}
