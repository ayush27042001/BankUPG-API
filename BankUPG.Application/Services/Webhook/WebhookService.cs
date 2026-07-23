using BankUPG.Application.Interfaces.Webhook;
using BankUPG.Infrastructure.Data;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.Webhook
{
    public class WebhookService : IWebhookService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<WebhookService> _logger;

        public WebhookService(AppDBContext context, ILogger<WebhookService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<WebhookResponse> CreateAsync(CreateWebhookRequest request)
        {
            var entity = new Infrastructure.Entities.Webhook
            {
                Mid = request.Mid,
                Type = request.Type,
                Event = request.Event,
                WebhookUrl = request.WebhookUrl,
                Remarks = request.Remarks,
                Status = request.Status ?? "Active",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Webhooks.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Webhook created for MID {Mid}", request.Mid);
            return MapToResponse(entity);
        }

        public async Task<WebhookResponse?> UpdateAsync(int webhookId, UpdateWebhookRequest request)
        {
            var entity = await _context.Webhooks.FindAsync(webhookId);
            if (entity == null) return null;

            entity.Mid = request.Mid;
            entity.Type = request.Type;
            entity.Event = request.Event;
            entity.WebhookUrl = request.WebhookUrl;
            entity.Remarks = request.Remarks;
            entity.Status = request.Status ?? entity.Status;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToResponse(entity);
        }

        public async Task<WebhookResponse?> GetAsync(int webhookId)
        {
            var entity = await _context.Webhooks.AsNoTracking()
                .FirstOrDefaultAsync(w => w.WebhookId == webhookId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<PagedResponse<WebhookResponse>> ListByMidAsync(int mid, int pageNumber, int pageSize)
        {
            var query = _context.Webhooks.AsNoTracking().Where(w => w.Mid == mid);
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(w => w.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(w => MapToResponse(w))
                .ToListAsync();

            return new PagedResponse<WebhookResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PagedResponse<WebhookResponse>> ListAsync(int pageNumber, int pageSize)
        {
            var query = _context.Webhooks.AsNoTracking();
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(w => w.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(w => MapToResponse(w))
                .ToListAsync();

            return new PagedResponse<WebhookResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> DeleteAsync(int webhookId)
        {
            var entity = await _context.Webhooks.FindAsync(webhookId);
            if (entity == null) return false;

            _context.Webhooks.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static WebhookResponse MapToResponse(Infrastructure.Entities.Webhook w) => new()
        {
            WebhookId = w.WebhookId,
            Mid = w.Mid,
            Type = w.Type,
            Event = w.Event,
            WebhookUrl = w.WebhookUrl,
            Remarks = w.Remarks,
            Status = w.Status,
            CreatedDate = w.CreatedDate,
            UpdatedDate = w.UpdatedDate
        };
    }
}
