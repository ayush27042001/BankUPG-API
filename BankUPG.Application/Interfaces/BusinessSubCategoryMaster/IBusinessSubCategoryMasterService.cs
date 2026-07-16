using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.BusinessSubCategoryMaster
{
    public interface IBusinessSubCategoryMasterService
    {
        Task<BusinessSubCategoryResponse> CreateBusinessSubCategoryAsync(CreateBusinessSubCategoryRequest request);

        Task<BusinessSubCategoryResponse?> GetBusinessSubCategoryByIdAsync(int businessSubCategoryId);

        Task<PagedResponse<BusinessSubCategoryResponse>> GetBusinessSubCategoryListAsync(GetBusinessSubCategoryListRequest request);

        Task<BusinessSubCategoryResponse> UpdateBusinessSubCategoryAsync(UpdateBusinessSubCategoryRequest request);
    }
}