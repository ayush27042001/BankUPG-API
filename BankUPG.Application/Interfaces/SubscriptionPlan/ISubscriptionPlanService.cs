using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.SubscriptionPlan
{
    public interface ISubscriptionPlanService
    {
        Task<SubscriptionPlanResponse> CreatePlanAsync(int userId, CreateSubscriptionPlanRequest request);
        Task<SubscriptionPlanResponse?> GetPlanAsync(int userId, int planId);
        Task<List<SubscriptionPlanResponse>> ListPlansAsync(int userId);
        Task<SubscriptionPlanResponse?> UpdatePlanAsync(int userId, int planId, UpdateSubscriptionPlanRequest request);
        Task<bool> DeactivatePlanAsync(int userId, int planId);
    }
}
