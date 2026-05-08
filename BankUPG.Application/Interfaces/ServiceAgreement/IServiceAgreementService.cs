using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.ServiceAgreement
{
    public interface IServiceAgreementService
    {
        Task<ServiceAgreementResponse?> GetServiceAgreementAsync(int userId);
        Task<ServiceAgreementSavedResponse> SaveServiceAgreementAsync(int userId, SaveServiceAgreementRequest request);
    }
}
