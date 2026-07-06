using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.BusinessProofTypeMaster
{
    public interface IBusinessProofTypeMasterService
    {
        Task<BusinessProofTypeMasterResponse> CreateBusinessProofTypeAsync(CreateBusinessProofTypeMasterRequest request);
        Task<BusinessProofTypeMasterResponse?> GetBusinessProofTypeByIdAsync(int businessProofTypeId);
        Task<List<BusinessProofTypeMasterResponse>> GetAllBusinessProofTypesAsync();
        Task<PagedResponse<BusinessProofTypeMasterResponse>> GetBusinessProofTypeListAsync(GetBusinessProofTypeListRequest request);
        Task<BusinessProofTypeMasterResponse> UpdateBusinessProofTypeAsync(UpdateBusinessProofTypeMasterRequest request);
        Task<bool> DeleteBusinessProofTypeAsync(int businessProofTypeId);
        Task<bool> ToggleBusinessProofTypeStatusAsync(int businessProofTypeId);
    }
}
