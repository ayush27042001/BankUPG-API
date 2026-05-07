using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.DocumentMaster
{
    public interface IDocumentMasterService
    {
        // Document Type operations
        Task<DocumentTypeDetailResponse> CreateDocumentTypeAsync(CreateDocumentTypeRequest request);
        Task<DocumentTypeDetailResponse?> GetDocumentTypeByIdAsync(int documentTypeId);
        Task<List<DocumentTypeDetailResponse>> GetAllDocumentTypesAsync();
        Task<DocumentTypeDetailResponse> UpdateDocumentTypeAsync(UpdateDocumentTypeRequest request);
        Task<bool> DeleteDocumentTypeAsync(int documentTypeId);
        Task<bool> ToggleDocumentTypeStatusAsync(int documentTypeId);

        // Business Proof Type operations
        Task<BusinessProofTypeDetailResponse> CreateBusinessProofTypeAsync(CreateBusinessProofTypeRequest request);
        Task<BusinessProofTypeDetailResponse?> GetBusinessProofTypeByIdAsync(int businessProofTypeId);
        Task<List<BusinessProofTypeDetailResponse>> GetAllBusinessProofTypesAsync();
        Task<BusinessProofTypeDetailResponse> UpdateBusinessProofTypeAsync(UpdateBusinessProofTypeRequest request);
        Task<bool> DeleteBusinessProofTypeAsync(int businessProofTypeId);
        Task<bool> ToggleBusinessProofTypeStatusAsync(int businessProofTypeId);
    }
}
