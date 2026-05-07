using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.PhoneCkyc
{
    public interface IPhoneCkycService
    {
        Task<PhoneCkycResponse?> GetPhoneCkycAsync(int userId);
        Task<OtpResponse> SendOtpAsync(int userId);
        Task<OtpVerificationResponse> VerifyOtpAsync(int userId, string otp);
        Task<PhoneCkycSavedResponse> SavePhoneCkycAsync(int userId, SavePhoneCkycRequest request);
    }
}
