using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.Application.Interfaces.Webhook
{
    public interface IWebhookService
    {
        Task<WebhookResponse> CreateAsync(CreateWebhookRequest request);
        Task<WebhookResponse?> UpdateAsync(int webhookId, UpdateWebhookRequest request);
        Task<WebhookResponse?> GetAsync(int webhookId);
        Task<PagedResponse<WebhookResponse>> ListByMidAsync(int mid, int pageNumber, int pageSize);
        Task<PagedResponse<WebhookResponse>> ListAsync(int pageNumber, int pageSize);
        Task<bool> DeleteAsync(int webhookId);
    }
}
