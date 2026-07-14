using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.PEPStatusMaster
{
    public interface IPEPStatusMasterService
    {
        Task<PEPStatusMasterResponse> CreatePEPStatusAsync(CreatePEPStatusMasterRequest request);

        Task<PEPStatusMasterResponse?> GetPEPStatusByIdAsync(int pepStatusId);

        Task<List<PEPStatusMasterResponse>> GetAllPEPStatusAsync();

        Task<PagedResponse<PEPStatusMasterResponse>> GetPEPStatusListAsync(GetPEPStatusListRequest request);

        Task<PEPStatusMasterResponse> UpdatePEPStatusAsync(UpdatePEPStatusMasterRequest request);
    }
}