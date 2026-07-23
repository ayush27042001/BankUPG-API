using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.Invoice
{
    public interface IInvoiceService
    {
        Task<InvoiceResponse> CreateInvoiceAsync(int userId, CreateInvoiceRequest request);
        Task<InvoiceResponse?> GetInvoiceAsync(int userId, long invoiceId);
        Task<PagedResponse<InvoiceResponse>> ListInvoicesAsync(int userId, ListInvoicesRequest request);
        Task<bool> SendInvoiceAsync(int userId, long invoiceId);
        Task<bool> CancelInvoiceAsync(int userId, long invoiceId);
    }
}
