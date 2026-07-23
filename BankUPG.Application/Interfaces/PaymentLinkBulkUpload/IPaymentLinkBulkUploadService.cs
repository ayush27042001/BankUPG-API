using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.PaymentLinkBulkUpload
{
    public interface IPaymentLinkBulkUploadService
    {
        Task<PaymentLinkBulkUploadResponse> CreateAsync(int userId, CreatePaymentLinkBulkUploadRequest request);
        Task<PagedResponse<PaymentLinkBulkUploadResponse>> ListAsync(int userId, ListPaymentLinkBulkUploadsRequest request);
        Task<PaymentLinkBulkUploadResponse?> GetAsync(int userId, long bulkUploadId);
        Task<PaymentLinkBulkUploadFileResponse> AddFileAsync(int userId, CreatePaymentLinkBulkUploadFileRequest request);
        Task<PagedResponse<PaymentLinkBulkUploadFileResponse>> ListFilesAsync(int userId, int pageNumber, int pageSize);
    }
}
