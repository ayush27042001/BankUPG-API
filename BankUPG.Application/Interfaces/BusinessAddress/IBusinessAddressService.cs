using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.BusinessAddress
{
    public interface IBusinessAddressService
    {
        Task<BusinessAddressResponse?> GetBusinessAddressAsync(int userId);
        Task<BusinessAddressSavedResponse> SaveBusinessAddressAsync(int userId, SaveBusinessAddressRequest request);
    }
}
