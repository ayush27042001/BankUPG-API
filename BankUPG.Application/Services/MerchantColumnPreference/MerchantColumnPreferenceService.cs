using BankUPG.Application.Interfaces.MerchantColumnPreference;
using BankUPG.Infrastructure.Data;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.MerchantColumnPreference
{
    public class MerchantColumnPreferenceService : IMerchantColumnPreferenceService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<MerchantColumnPreferenceService> _logger;

        public MerchantColumnPreferenceService(AppDBContext context, ILogger<MerchantColumnPreferenceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MerchantColumnPreferenceResponse> CreateAsync(CreateMerchantColumnPreferenceRequest request)
        {
            var existing = await _context.MerchantColumnPreferences
                .FirstOrDefaultAsync(c => c.Mid == request.Mid && c.GridName == request.GridName);

            if (existing != null)
            {
                existing.SelectedColumns = request.SelectedColumns;
                existing.UpdatedDate = DateTime.UtcNow;
            }
            else
            {
                existing = new Infrastructure.Entities.MerchantColumnPreference
                {
                    Mid = request.Mid,
                    GridName = request.GridName,
                    SelectedColumns = request.SelectedColumns,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };
                _context.MerchantColumnPreferences.Add(existing);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Column preference saved for MID {Mid}, Grid {Grid}", request.Mid, request.GridName);
            return MapToResponse(existing);
        }

        public async Task<MerchantColumnPreferenceResponse?> UpdateAsync(int merchantColumnPreferenceId, UpdateMerchantColumnPreferenceRequest request)
        {
            var entity = await _context.MerchantColumnPreferences.FindAsync(merchantColumnPreferenceId);
            if (entity == null) return null;

            entity.Mid = request.Mid;
            entity.GridName = request.GridName;
            entity.SelectedColumns = request.SelectedColumns;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToResponse(entity);
        }

        public async Task<MerchantColumnPreferenceResponse?> GetAsync(int merchantColumnPreferenceId)
        {
            var entity = await _context.MerchantColumnPreferences.AsNoTracking()
                .FirstOrDefaultAsync(c => c.MerchantColumnPreferenceId == merchantColumnPreferenceId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<MerchantColumnPreferenceResponse?> GetByMidAndGridAsync(int mid, string gridName)
        {
            var entity = await _context.MerchantColumnPreferences.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Mid == mid && c.GridName == gridName);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<bool> DeleteAsync(int merchantColumnPreferenceId)
        {
            var entity = await _context.MerchantColumnPreferences.FindAsync(merchantColumnPreferenceId);
            if (entity == null) return false;

            _context.MerchantColumnPreferences.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static MerchantColumnPreferenceResponse MapToResponse(Infrastructure.Entities.MerchantColumnPreference c) => new()
        {
            MerchantColumnPreferenceId = c.MerchantColumnPreferenceId,
            Mid = c.Mid,
            GridName = c.GridName,
            SelectedColumns = c.SelectedColumns,
            CreatedDate = c.CreatedDate,
            UpdatedDate = c.UpdatedDate
        };
    }
}
