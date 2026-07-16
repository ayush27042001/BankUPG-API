using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.BusinessCategoryMaster
{
    public interface IBusinessCategoryMasterService
    {
        Task<BusinessCategoryResponse> CreateBusinessCategoryAsync(CreateBusinessCategoryRequest request);

        Task<BusinessCategoryResponse?> GetBusinessCategoryByIdAsync(int businessCategoryId);

        Task<PagedResponse<BusinessCategoryResponse>> GetBusinessCategoryListAsync(GetBusinessCategoryListRequest request);

        Task<BusinessCategoryResponse> UpdateBusinessCategoryAsync(UpdateBusinessCategoryRequest request);
    }
}