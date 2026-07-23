using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.PaymentMethodCharge
{
    public interface IPaymentMethodChargeService
    {
        Task<PaymentMethodChargeResponse> CreateAsync(CreatePaymentMethodChargeRequest request);
        Task<PaymentMethodChargeResponse?> UpdateAsync(int paymentMethodChargeId, CreatePaymentMethodChargeRequest request);
        Task<PaymentMethodChargeResponse?> GetAsync(int paymentMethodChargeId);
        Task<PagedResponse<PaymentMethodChargeResponse>> ListAsync(int pageNumber, int pageSize);
        Task<bool> DeleteAsync(int paymentMethodChargeId);
    }
}
