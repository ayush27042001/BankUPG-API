using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.BusinessEntity
{
    public interface IBusinessEntityService
    {
        Task<List<BusinessEntityTypeDto>> GetBusinessEntityTypesAsync();
        Task<BusinessEntityResponse?> GetBusinessEntityAsync(int userId);
        Task<BusinessEntitySavedResponse> SaveBusinessEntityAsync(int userId, SaveBusinessEntityRequest request);
    }
}
