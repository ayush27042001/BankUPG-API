using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.MerchantColumnPreference
{
    public interface IMerchantColumnPreferenceService
    {
        Task<MerchantColumnPreferenceResponse> CreateAsync(CreateMerchantColumnPreferenceRequest request);
        Task<MerchantColumnPreferenceResponse?> UpdateAsync(int merchantColumnPreferenceId, UpdateMerchantColumnPreferenceRequest request);
        Task<MerchantColumnPreferenceResponse?> GetAsync(int merchantColumnPreferenceId);
        Task<MerchantColumnPreferenceResponse?> GetByMidAndGridAsync(int mid, string gridName);
        Task<bool> DeleteAsync(int merchantColumnPreferenceId);
    }
}
