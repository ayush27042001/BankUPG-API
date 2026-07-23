using BankUPG.Application.Interfaces.MerchantApiKey;
using BankUPG.Infrastructure.Data;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.MerchantApiKey
{
    public class MerchantApiKeyService : IMerchantApiKeyService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<MerchantApiKeyService> _logger;

        public MerchantApiKeyService(AppDBContext context, ILogger<MerchantApiKeyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MerchantApiKeyResponse> CreateAsync(CreateMerchantApiKeyRequest request)
        {
            var existing = await _context.MerchantApiKeys
                .FirstOrDefaultAsync(k => k.Mid == request.Mid);

            if (existing != null)
            {
                existing.ApiKey = request.ApiKey;
                existing.ApiSalt = request.ApiSalt;
                existing.ClientId = request.ClientId;
                existing.ClientSecretHash = request.ClientSecretHash;
                existing.LastUpdatedDate = DateTime.UtcNow;
                existing.UpdatedDate = DateTime.UtcNow;
            }
            else
            {
                existing = new Infrastructure.Entities.MerchantApiKey
                {
                    Mid = request.Mid,
                    ApiKey = request.ApiKey,
                    ApiSalt = request.ApiSalt,
                    ClientId = request.ClientId,
                    ClientSecretHash = request.ClientSecretHash,
                    LastUpdatedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };
                _context.MerchantApiKeys.Add(existing);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("API keys updated for MID {Mid}", request.Mid);
            return MapToResponse(existing);
        }

        public async Task<MerchantApiKeyResponse?> UpdateAsync(int merchantApiKeyId, UpdateMerchantApiKeyRequest request)
        {
            var entity = await _context.MerchantApiKeys.FindAsync(merchantApiKeyId);
            if (entity == null) return null;

            entity.Mid = request.Mid;
            entity.ApiKey = request.ApiKey;
            entity.ApiSalt = request.ApiSalt;
            entity.ClientId = request.ClientId;
            entity.ClientSecretHash = request.ClientSecretHash;
            entity.LastUpdatedDate = DateTime.UtcNow;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToResponse(entity);
        }

        public async Task<MerchantApiKeyResponse?> GetAsync(int merchantApiKeyId)
        {
            var entity = await _context.MerchantApiKeys.AsNoTracking()
                .FirstOrDefaultAsync(k => k.MerchantApiKeyId == merchantApiKeyId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<MerchantApiKeyResponse?> GetByMidAsync(int mid)
        {
            var entity = await _context.MerchantApiKeys.AsNoTracking()
                .FirstOrDefaultAsync(k => k.Mid == mid);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<bool> DeleteAsync(int merchantApiKeyId)
        {
            var entity = await _context.MerchantApiKeys.FindAsync(merchantApiKeyId);
            if (entity == null) return false;

            _context.MerchantApiKeys.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static MerchantApiKeyResponse MapToResponse(Infrastructure.Entities.MerchantApiKey k) => new()
        {
            MerchantApiKeyId = k.MerchantApiKeyId,
            Mid = k.Mid,
            ApiKey = k.ApiKey,
            ApiSalt = k.ApiSalt,
            ClientId = k.ClientId,
            ClientSecretHash = k.ClientSecretHash,
            LastUpdatedDate = k.LastUpdatedDate,
            CreatedDate = k.CreatedDate,
            UpdatedDate = k.UpdatedDate
        };
    }
}
