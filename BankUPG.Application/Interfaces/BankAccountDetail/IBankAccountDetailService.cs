using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.BankAccountDetail
{
    public interface IBankAccountDetailService
    {
        Task<BankAccountDetailResponse?> GetBankAccountDetailAsync(int userId);
        Task<BankAccountDetailSavedResponse> SaveBankAccountDetailAsync(int userId, SaveBankAccountDetailRequest request);
        Task<BankAccountVerifyResult> VerifyBankAccountAsync(VerifyBankAccountRequest request);
    }
}
