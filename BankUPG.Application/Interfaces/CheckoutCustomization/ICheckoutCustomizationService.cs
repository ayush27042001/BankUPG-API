using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.CheckoutCustomization
{
    public interface ICheckoutCustomizationService
    {
        Task<CheckoutCustomizationResponse> CreateAsync(CreateCheckoutCustomizationRequest request);
        Task<CheckoutCustomizationResponse?> UpdateAsync(int checkoutCustomizationId, UpdateCheckoutCustomizationRequest request);
        Task<CheckoutCustomizationResponse?> GetAsync(int checkoutCustomizationId);
        Task<CheckoutCustomizationResponse?> GetByMidAsync(int mid);
        Task<PagedResponse<CheckoutCustomizationResponse>> ListAsync(int pageNumber, int pageSize);
        Task<bool> DeleteAsync(int checkoutCustomizationId);

        /// <summary>Merchant self-service: get own customization by userId (resolves Mid from JWT).</summary>
        Task<CheckoutCustomizationResponse?> GetByUserIdAsync(int userId);

        /// <summary>Merchant self-service: create or update own customization.</summary>
        Task<CheckoutCustomizationResponse> UpsertByUserIdAsync(int userId, MerchantCheckoutCustomizationRequest request);

        /// <summary>Save uploaded logo/signature file to wwwroot and return its URL. assetType: "logo" or "signature".</summary>
        Task<string> UploadAssetAsync(int userId, Microsoft.AspNetCore.Http.IFormFile file, string webRootPath, string assetType);
    }
}
