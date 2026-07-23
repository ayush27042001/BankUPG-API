using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.PayoutBeneficiary
{
    public interface IPayoutBeneficiaryService
    {
        Task<PayoutBeneficiaryResponse> CreateBeneficiaryAsync(int userId, CreatePayoutBeneficiaryRequest request);
        Task<PayoutBeneficiaryResponse?> GetBeneficiaryAsync(int userId, long beneficiaryId);
        Task<PagedResponse<PayoutBeneficiaryResponse>> ListBeneficiariesAsync(int userId, int pageNumber, int pageSize);
        Task<bool> DeactivateBeneficiaryAsync(int userId, long beneficiaryId);
    }
}
