using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.PaymentLink
{
    public interface IPaymentLinkService
    {
        Task<PaymentLinkResponse> CreateLinkAsync(int userId, CreatePaymentLinkRequest request);
        Task<PaymentLinkResponse?> GetLinkAsync(int userId, long linkId);
        Task<PagedResponse<PaymentLinkResponse>> ListLinksAsync(int userId, ListPaymentLinksRequest request);
        Task<PaymentLinkSummaryResponse> GetSummaryAsync(int userId);
        Task<bool> CancelLinkAsync(int userId, long linkId);
    }
}
