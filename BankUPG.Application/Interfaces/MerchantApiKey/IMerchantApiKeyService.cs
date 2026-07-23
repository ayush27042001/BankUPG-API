using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.MerchantApiKey
{
    public interface IMerchantApiKeyService
    {
        Task<MerchantApiKeyResponse> CreateAsync(CreateMerchantApiKeyRequest request);
        Task<MerchantApiKeyResponse?> UpdateAsync(int merchantApiKeyId, UpdateMerchantApiKeyRequest request);
        Task<MerchantApiKeyResponse?> GetAsync(int merchantApiKeyId);
        Task<MerchantApiKeyResponse?> GetByMidAsync(int mid);
        Task<bool> DeleteAsync(int merchantApiKeyId);
    }
}
