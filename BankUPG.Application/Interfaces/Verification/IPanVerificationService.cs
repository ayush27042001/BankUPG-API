using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.Verification
{
    public interface IPanVerificationService
    {
        Task<PanVerificationResult> VerifyPanAsync(string panNumber, string? nameOnPan = null);
    }
}
