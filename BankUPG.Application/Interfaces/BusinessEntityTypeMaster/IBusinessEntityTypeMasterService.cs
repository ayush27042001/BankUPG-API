using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.BusinessEntityTypeMaster
{
    public interface IBusinessEntityTypeMasterService
    {
        Task<BusinessEntityTypeMasterResponse> CreateBusinessEntityTypeAsync(CreateBusinessEntityTypeMasterRequest request);

        Task<BusinessEntityTypeMasterResponse?> GetBusinessEntityTypeByIdAsync(int businessEntityTypeId);

        Task<List<BusinessEntityTypeMasterResponse>> GetAllBusinessEntityTypesAsync();

        Task<PagedResponse<BusinessEntityTypeMasterResponse>> GetBusinessEntityTypeListAsync(GetBusinessEntityTypeListRequest request);

        Task<BusinessEntityTypeMasterResponse> UpdateBusinessEntityTypeAsync(UpdateBusinessEntityTypeMasterRequest request);

        Task<bool> DeleteBusinessEntityTypeAsync(int businessEntityTypeId);

        Task<bool> ToggleBusinessEntityTypeStatusAsync(int businessEntityTypeId);
    }
}
