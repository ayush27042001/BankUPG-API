using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.MerchantPaymentMethod
{
    public interface IMerchantPaymentMethodService
    {
        Task<MerchantPaymentMethodResponse> CreateAsync(CreateMerchantPaymentMethodRequest request);
        Task<MerchantPaymentMethodResponse?> UpdateAsync(int merchantPaymentMethodId, UpdateMerchantPaymentMethodRequest request);
        Task<MerchantPaymentMethodResponse?> GetAsync(int merchantPaymentMethodId);
        Task<PagedResponse<MerchantPaymentMethodResponse>> ListByMidAsync(int mid, int pageNumber, int pageSize);
        Task<bool> DeleteAsync(int merchantPaymentMethodId);
    }
}
