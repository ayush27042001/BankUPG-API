using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.Subscription
{
    public interface ISubscriptionService
    {
        Task<SubscriptionResponse> CreateSubscriptionAsync(int userId, CreateSubscriptionRequest request);
        Task<SubscriptionResponse?> GetSubscriptionAsync(int userId, long subscriptionId);
        Task<PagedResponse<SubscriptionResponse>> ListSubscriptionsAsync(int userId, ListSubscriptionsRequest request);
        Task<bool> CancelSubscriptionAsync(int userId, long subscriptionId);
    }
}
