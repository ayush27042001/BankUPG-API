using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.IpWhitelist
{
    public interface IIpWhitelistService
    {
        Task<IpWhitelistStatusResponse> GetStatusAsync(int userId);
        Task<IpWhitelistResponse> AddIpAsync(int userId, AddIpWhitelistRequest request);
        Task<bool> RemoveIpAsync(int userId, int ipWhitelistId);
        Task<bool> ToggleWhitelistAsync(int userId, bool enabled);
        Task<bool> IsIpAllowedAsync(int mid, string ipAddress);
    }
}
