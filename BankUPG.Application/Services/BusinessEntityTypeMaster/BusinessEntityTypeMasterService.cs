using BankUPG.Application.Interfaces.BusinessEntityTypeMaster;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.BusinessEntityTypeMaster
{
    public class BusinessEntityTypeMasterService : IBusinessEntityTypeMasterService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<BusinessEntityTypeMasterService> _logger;

        public BusinessEntityTypeMasterService(
            AppDBContext context,
            ILogger<BusinessEntityTypeMasterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BusinessEntityTypeMasterResponse> CreateBusinessEntityTypeAsync(CreateBusinessEntityTypeMasterRequest request)
        {
            // Check if entity name already exists
            var existingEntity = await _context.BusinessEntityTypes
                .AnyAsync(x => x.EntityName == request.EntityName);

            if (existingEntity)
                throw new InvalidOperationException(
                    $"Business Entity Type '{request.EntityName}' already exists.");

            // Create Entity
            var businessEntityType = new BusinessEntityType
            {
                EntityName = request.EntityName,
                Description = request.Description,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            // Save into database
            _context.BusinessEntityTypes.Add(businessEntityType);
            await _context.SaveChangesAsync();

            // Log Information
            _logger.LogInformation(
                "Business Entity Type created successfully: {BusinessEntityTypeId} - {EntityName}",
                businessEntityType.BusinessEntityTypeId,
                businessEntityType.EntityName);

            // Return Response
            return new BusinessEntityTypeMasterResponse
            {
                BusinessEntityTypeId = businessEntityType.BusinessEntityTypeId,
                EntityName = businessEntityType.EntityName,
                Description = businessEntityType.Description,
                IsActive = businessEntityType.IsActive,
                CreatedDate = businessEntityType.CreatedDate,
                UpdatedDate = businessEntityType.UpdatedDate
            };
        }

        public async Task<BusinessEntityTypeMasterResponse?> GetBusinessEntityTypeByIdAsync(int businessEntityTypeId)
        {
            var businessEntityType = await _context.BusinessEntityTypes
                .FirstOrDefaultAsync(x => x.BusinessEntityTypeId == businessEntityTypeId);

            if (businessEntityType == null)
                return null;

            return new BusinessEntityTypeMasterResponse
            {
                BusinessEntityTypeId = businessEntityType.BusinessEntityTypeId,
                EntityName = businessEntityType.EntityName,
                Description = businessEntityType.Description,
                IsActive = businessEntityType.IsActive,
                CreatedDate = businessEntityType.CreatedDate,
                UpdatedDate = businessEntityType.UpdatedDate
            };
        }

        public async Task<List<BusinessEntityTypeMasterResponse>> GetAllBusinessEntityTypesAsync()
        {
            return await _context.BusinessEntityTypes
                .OrderBy(x => x.EntityName)
                .Select(x => new BusinessEntityTypeMasterResponse
                {
                    BusinessEntityTypeId = x.BusinessEntityTypeId,
                    EntityName = x.EntityName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToListAsync();
        }

        public async Task<PagedResponse<BusinessEntityTypeMasterResponse>> GetBusinessEntityTypeListAsync(GetBusinessEntityTypeListRequest request)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 100 ? 100 : request.PageSize;

            var query = _context.BusinessEntityTypes.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();

                query = query.Where(x =>
                    x.EntityName.ToLower().Contains(search) ||
                    (x.Description != null && x.Description.ToLower().Contains(search)));
            }

            // Active Filter
            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.EntityName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BusinessEntityTypeMasterResponse
                {
                    BusinessEntityTypeId = x.BusinessEntityTypeId,
                    EntityName = x.EntityName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToListAsync();

            return new PagedResponse<BusinessEntityTypeMasterResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<BusinessEntityTypeMasterResponse> UpdateBusinessEntityTypeAsync(UpdateBusinessEntityTypeMasterRequest request)
        {
            var businessEntityType = await _context.BusinessEntityTypes
                .FirstOrDefaultAsync(x => x.BusinessEntityTypeId == request.BusinessEntityTypeId);

            if (businessEntityType == null)
                throw new ArgumentException("Business Entity Type not found.");

            // Check duplicate Entity Name (excluding current record)
            var existingEntity = await _context.BusinessEntityTypes
                .AnyAsync(x =>
                    x.EntityName == request.EntityName &&
                    x.BusinessEntityTypeId != request.BusinessEntityTypeId);

            if (existingEntity)
                throw new InvalidOperationException(
                    $"Business Entity Type '{request.EntityName}' already exists.");

            // Update Values
            businessEntityType.EntityName = request.EntityName;
            businessEntityType.Description = request.Description;

            if (request.IsActive.HasValue)
                businessEntityType.IsActive = request.IsActive.Value;

            businessEntityType.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Business Entity Type updated successfully: {BusinessEntityTypeId} - {EntityName}",
                businessEntityType.BusinessEntityTypeId,
                businessEntityType.EntityName);

            return new BusinessEntityTypeMasterResponse
            {
                BusinessEntityTypeId = businessEntityType.BusinessEntityTypeId,
                EntityName = businessEntityType.EntityName,
                Description = businessEntityType.Description,
                IsActive = businessEntityType.IsActive,
                CreatedDate = businessEntityType.CreatedDate,
                UpdatedDate = businessEntityType.UpdatedDate
            };
        }

        public async Task<bool> DeleteBusinessEntityTypeAsync(int businessEntityTypeId)
        {
            var businessEntityType = await _context.BusinessEntityTypes
                .FirstOrDefaultAsync(x => x.BusinessEntityTypeId == businessEntityTypeId);

            if (businessEntityType == null)
                return false;

            // Check if this Business Entity Type is being used by any Merchant
            var hasMerchants = await _context.Merchants
                .AnyAsync(m => m.BusinessEntityTypeId == businessEntityTypeId);

            if (hasMerchants)
                throw new InvalidOperationException(
                    "Cannot delete Business Entity Type. It is already assigned to one or more merchants.");

            _context.BusinessEntityTypes.Remove(businessEntityType);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Business Entity Type deleted successfully: {BusinessEntityTypeId}",
                businessEntityTypeId);

            return true;
        }

        public async Task<bool> ToggleBusinessEntityTypeStatusAsync(int businessEntityTypeId)
        {
            var businessEntityType = await _context.BusinessEntityTypes
                .FirstOrDefaultAsync(x => x.BusinessEntityTypeId == businessEntityTypeId);

            if (businessEntityType == null)
                return false;

            // Toggle Status
            businessEntityType.IsActive = !businessEntityType.IsActive;
            businessEntityType.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Business Entity Type status toggled successfully: {BusinessEntityTypeId} - IsActive: {IsActive}",
                businessEntityType.BusinessEntityTypeId,
                businessEntityType.IsActive);

            return true;
        }
    }
}