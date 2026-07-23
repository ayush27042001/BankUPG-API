using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.Settlement
{
    public interface ISettlementService
    {
        Task<PagedResponse<SettlementResponse>> ListAsync(int userId, ListSettlementsRequest request);
        Task<SettlementResponse?> GetAsync(int userId, long settlementId);
        Task<SettlementSummaryResponse> GetSummaryAsync(int userId);
        Task<SettlementConfigResponse?> GetConfigAsync(int userId);
        Task<SettlementConfigResponse> UpdateConfigAsync(int userId, UpdateSettlementConfigRequest request);
    }
}
