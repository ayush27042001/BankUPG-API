using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankUPG.Application.Interfaces.DocumentTypeMaster
{
    public interface IDocumentTypeMasterService
    {
        Task<DocumentTypeMasterResponse> CreateDocumentTypeAsync(CreateDocumentTypeMasterRequest request);
        Task<DocumentTypeMasterResponse?> GetDocumentTypeByIdAsync(int documentTypeId);
        Task<List<DocumentTypeMasterResponse>> GetAllDocumentTypesAsync();
        Task<PagedResponse<DocumentTypeMasterResponse>> GetDocumentTypeListAsync(GetDocumentTypeListRequest request);
        Task<DocumentTypeMasterResponse> UpdateDocumentTypeAsync(UpdateDocumentTypeMasterRequest request);
    }
}