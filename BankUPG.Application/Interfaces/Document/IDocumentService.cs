using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.Document
{
    public interface IDocumentService
    {
        Task<DocumentUploadResponse> UploadDocumentAsync(int userId, UploadDocumentRequest request);
        Task<DocumentUploadResponse> UpdateDocumentAsync(int userId, UpdateDocumentRequest request);
        Task<DocumentResponse?> GetDocumentAsync(int userId, int documentUploadId);
        Task<DocumentListResponse> GetDocumentsByMerchantAsync(int userId);
        Task<DocumentListResponse> GetDocumentsByTypeAsync(int userId, int documentTypeId);
        Task<(byte[] fileData, string fileName, string mimeType)> DownloadDocumentAsync(int userId, int documentUploadId);
        Task<List<DocumentTypeResponse>> GetDocumentTypesAsync();
        Task<List<BusinessProofTypeResponse>> GetBusinessProofTypesAsync();
        Task<bool> DeleteDocumentAsync(int userId, int documentUploadId);
    }
}
