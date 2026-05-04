using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.Registration
{
    public interface IRegistrationService
    {
        Task<(string registrationToken, string mobileNumber, int expirySeconds)> InitiateRegistrationAsync(InitiateRegistrationRequest request, string? ipAddress);
        Task<OtpVerificationResponse> VerifyRegistrationOtpAsync(string mobileNumber, string otp, string registrationToken);
        Task<RegistrationCompletedResponse> CompletePanRegistrationAsync(int userId, CompletePanRegistrationRequest request);
        Task<ResendOtpResponse> ResendRegistrationOtpAsync(string mobileNumber, string registrationToken);
        Task<PanDetailsResponse?> GetPanDetailsAsync(int userId);
    }
}
