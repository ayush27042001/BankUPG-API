using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.PaymentOrder
{
    public interface IPaymentOrderService
    {
        Task<PaymentOrderResponse> CreateOrderAsync(int userId, CreatePaymentOrderRequest request);
        Task<PaymentOrderResponse?> GetOrderAsync(int userId, long orderId);
        Task<PagedResponse<PaymentOrderResponse>> ListOrdersAsync(int userId, ListPaymentOrdersRequest request);
        Task<bool> CancelOrderAsync(int userId, long orderId);
        Task<List<PaymentAttemptResponse>> GetOrderAttemptsAsync(int userId, long orderId);
    }
}
