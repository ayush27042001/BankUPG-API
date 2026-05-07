using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.SigningAuthorityDetail
{
    public interface ISigningAuthorityDetailService
    {
        Task<SigningAuthorityDetailResponse?> GetSigningAuthorityDetailAsync(int userId);
        Task<SigningAuthorityDetailSavedResponse> SaveSigningAuthorityDetailAsync(int userId, SaveSigningAuthorityDetailRequest request);
    }
}
