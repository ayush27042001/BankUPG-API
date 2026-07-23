using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.Refund
{
    public interface IRefundService
    {
        Task<PagedResponse<RefundDetailResponse>> ListAsync(int userId, ListRefundsRequest request);
        Task<RefundDetailResponse?> GetAsync(int userId, long refundId);
        Task<RefundDetailResponse> InitiateRefundAsync(int userId, InitiateRefundRequest request);
    }
}
