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
    }
}
