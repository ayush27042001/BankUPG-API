using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.UserMaster
{
    public interface IUserMasterService
    {
        Task<UserResponse> CreateUserAsync(CreateUserRequest request);

        Task<UserResponse?> GetUserByIdAsync(int userId);

        Task<PagedResponse<UserResponse>> GetUserListAsync(GetUserListRequest request);
    }
}