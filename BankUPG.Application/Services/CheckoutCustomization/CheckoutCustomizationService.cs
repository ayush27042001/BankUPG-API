using BankUPG.Application.Interfaces.CheckoutCustomization;
using BankUPG.Infrastructure.Data;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.CheckoutCustomization
{
    public class CheckoutCustomizationService : ICheckoutCustomizationService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<CheckoutCustomizationService> _logger;

        public CheckoutCustomizationService(AppDBContext context, ILogger<CheckoutCustomizationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CheckoutCustomizationResponse> CreateAsync(CreateCheckoutCustomizationRequest request)
        {
            var entity = new Infrastructure.Entities.CheckoutCustomization
            {
                Mid = request.Mid,
                BrandLogoUrl = request.BrandLogoUrl,
                PrimaryColor = request.PrimaryColor,
                SecondaryColor = request.SecondaryColor,
                Language = request.Language,
                OwnerSignatureUrl = request.OwnerSignatureUrl,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.CheckoutCustomizations.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Checkout customization created for MID {Mid}", request.Mid);
            return MapToResponse(entity);
        }

        public async Task<CheckoutCustomizationResponse?> UpdateAsync(int checkoutCustomizationId, UpdateCheckoutCustomizationRequest request)
        {
            var entity = await _context.CheckoutCustomizations.FindAsync(checkoutCustomizationId);
            if (entity == null) return null;

            entity.Mid = request.Mid;
            entity.BrandLogoUrl = request.BrandLogoUrl;
            entity.PrimaryColor = request.PrimaryColor;
            entity.SecondaryColor = request.SecondaryColor;
            entity.Language = request.Language;
            entity.OwnerSignatureUrl = request.OwnerSignatureUrl;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToResponse(entity);
        }

        public async Task<CheckoutCustomizationResponse?> GetAsync(int checkoutCustomizationId)
        {
            var entity = await _context.CheckoutCustomizations.AsNoTracking()
                .FirstOrDefaultAsync(c => c.CheckoutCustomizationId == checkoutCustomizationId);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<CheckoutCustomizationResponse?> GetByMidAsync(int mid)
        {
            var entity = await _context.CheckoutCustomizations.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Mid == mid);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<PagedResponse<CheckoutCustomizationResponse>> ListAsync(int pageNumber, int pageSize)
        {
            var query = _context.CheckoutCustomizations.AsNoTracking();
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => MapToResponse(c))
                .ToListAsync();

            return new PagedResponse<CheckoutCustomizationResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> DeleteAsync(int checkoutCustomizationId)
        {
            var entity = await _context.CheckoutCustomizations.FindAsync(checkoutCustomizationId);
            if (entity == null) return false;

            _context.CheckoutCustomizations.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static CheckoutCustomizationResponse MapToResponse(Infrastructure.Entities.CheckoutCustomization c) => new()
        {
            CheckoutCustomizationId = c.CheckoutCustomizationId,
            Mid = c.Mid,
            BrandLogoUrl = c.BrandLogoUrl,
            PrimaryColor = c.PrimaryColor,
            SecondaryColor = c.SecondaryColor,
            Language = c.Language,
            OwnerSignatureUrl = c.OwnerSignatureUrl,
            CreatedDate = c.CreatedDate,
            UpdatedDate = c.UpdatedDate
        };
    }
}
