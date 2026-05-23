using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.StatusTracker
{
    public interface IStatusTrackerService
    {
        Task<StatusTrackerResponse?> GetOnboardingStatusAsync(int userId);
    }
}
