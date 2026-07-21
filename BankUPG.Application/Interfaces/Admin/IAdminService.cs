using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.Admin
{
    public interface IAdminService
    {
        Task<UserCompleteDataResponse?> GetUserCompleteDataAsync(UserDetailRequest request);
    }
}
