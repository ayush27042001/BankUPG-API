using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.Checkout
{
    public interface ICheckoutService
    {
        Task<CheckoutOrderResponse> InitiateOrderAsync(string apiKey, CheckoutInitiateRequest request, string baseUrl);
        Task<CheckoutSessionResponse?> GetSessionAsync(string checkoutToken);
        Task<CheckoutPayResponse> ProcessPaymentAsync(CheckoutPayCardRequest request, string baseUrl);
        Task<CheckoutVerifyResponse> VerifyPaymentAsync(string apiKey, CheckoutVerifyRequest request);
        Task<CheckoutStatusResponse?> GetOrderStatusAsync(string apiKey, string orderId);
    }
}
