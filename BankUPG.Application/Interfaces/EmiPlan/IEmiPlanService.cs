using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.EmiPlan
{
    public interface IEmiPlanService
    {
        Task<EmiPlanResponse> CreateEmiPlanAsync(int userId, CreateEmiPlanRequest request);
        Task<List<EmiPlanResponse>> ListEmiPlansAsync(int userId);
        Task<EmiPlanResponse?> UpdateEmiPlanAsync(int userId, int emiPlanId, UpdateEmiPlanRequest request);
        Task<bool> DeactivateEmiPlanAsync(int userId, int emiPlanId);
    }
}
