using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.ConnectPlatform
{
    public interface IConnectPlatformService
    {
        Task<ConnectPlatformResponse?> GetConnectPlatformAsync(int userId);
        Task<ConnectPlatformSavedResponse> SaveConnectPlatformAsync(int userId, SaveConnectPlatformRequest request);
    }
}
