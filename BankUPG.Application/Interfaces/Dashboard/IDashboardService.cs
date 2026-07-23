using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardSummaryResponse> GetSummaryAsync(int userId, GetDashboardSummaryRequest request);
        Task<List<DailyMetricResponse>> GetDailyMetricsAsync(int userId, int days);
    }
}
