using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.BusinessCategory
{
    public interface IBusinessCategoryService
    {
        Task<List<BusinessCategoryDto>> GetAllCategoriesAsync();
        Task<MerchantBusinessCategoryResponse?> GetBusinessCategoryAsync(int userId);
        Task<BusinessCategorySavedResponse> SaveBusinessCategoryAsync(int userId, SaveBusinessCategoryRequest request);
    }
}
