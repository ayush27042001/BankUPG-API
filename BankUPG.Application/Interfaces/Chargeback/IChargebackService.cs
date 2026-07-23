using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.Chargeback
{
    public interface IChargebackService
    {
        Task<PagedResponse<ChargebackResponse>> ListAsync(int userId, ListChargebacksRequest request);
        Task<ChargebackResponse?> GetAsync(int userId, long chargebackId);
        Task<ChargebackResponse?> UpdateAsync(int userId, long chargebackId, UpdateChargebackRequest request);
    }
}
