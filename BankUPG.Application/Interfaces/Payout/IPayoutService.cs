using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.Payout
{
    public interface IPayoutService
    {
        Task<PayoutResponse> CreatePayoutAsync(int userId, CreatePayoutRequest request);
        Task<PayoutResponse?> GetPayoutAsync(int userId, long payoutId);
        Task<PagedResponse<PayoutResponse>> ListPayoutsAsync(int userId, ListPayoutsRequest request);
        Task<bool> CancelPayoutAsync(int userId, long payoutId);
    }
}
