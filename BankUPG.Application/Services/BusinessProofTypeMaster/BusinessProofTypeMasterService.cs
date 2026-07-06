using BankUPG.Application.Interfaces.BusinessProofTypeMaster;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.BusinessProofTypeMaster
{
    public class BusinessProofTypeMasterService : IBusinessProofTypeMasterService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<BusinessProofTypeMasterService> _logger;

        public BusinessProofTypeMasterService(
            AppDBContext context,
            ILogger<BusinessProofTypeMasterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BusinessProofTypeMasterResponse> CreateBusinessProofTypeAsync(CreateBusinessProofTypeMasterRequest request)
        {
            // Check if proof code already exists
            var existingCode = await _context.BusinessProofTypes
                .AnyAsync(bpt => bpt.ProofCode == request.ProofCode);

            if (existingCode)
                throw new InvalidOperationException($"Business proof type with code '{request.ProofCode}' already exists.");

            // Check if proof name already exists
            var existingName = await _context.BusinessProofTypes
                .AnyAsync(bpt => bpt.ProofName == request.ProofName);

            if (existingName)
                throw new InvalidOperationException($"Business proof type with name '{request.ProofName}' already exists.");

            var businessProofType = new BusinessProofType
            {
                ProofName = request.ProofName,
                ProofCode = request.ProofCode,
                Description = request.Description,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.BusinessProofTypes.Add(businessProofType);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Business proof type created successfully: {BusinessProofTypeId} - {ProofName}", 
                businessProofType.BusinessProofTypeId, businessProofType.ProofName);

            return new BusinessProofTypeMasterResponse
            {
                BusinessProofTypeId = businessProofType.BusinessProofTypeId,
                ProofName = businessProofType.ProofName,
                ProofCode = businessProofType.ProofCode,
                Description = businessProofType.Description,
                IsActive = businessProofType.IsActive,
                CreatedDate = businessProofType.CreatedDate,
                UpdatedDate = businessProofType.UpdatedDate
            };
        }

        public async Task<BusinessProofTypeMasterResponse?> GetBusinessProofTypeByIdAsync(int businessProofTypeId)
        {
            var businessProofType = await _context.BusinessProofTypes
                .FirstOrDefaultAsync(bpt => bpt.BusinessProofTypeId == businessProofTypeId);

            if (businessProofType == null)
                return null;

            return new BusinessProofTypeMasterResponse
            {
                BusinessProofTypeId = businessProofType.BusinessProofTypeId,
                ProofName = businessProofType.ProofName,
                ProofCode = businessProofType.ProofCode,
                Description = businessProofType.Description,
                IsActive = businessProofType.IsActive,
                CreatedDate = businessProofType.CreatedDate,
                UpdatedDate = businessProofType.UpdatedDate
            };
        }

        public async Task<List<BusinessProofTypeMasterResponse>> GetAllBusinessProofTypesAsync()
        {
            return await _context.BusinessProofTypes
                .OrderBy(bpt => bpt.ProofName)
                .Select(bpt => new BusinessProofTypeMasterResponse
                {
                    BusinessProofTypeId = bpt.BusinessProofTypeId,
                    ProofName = bpt.ProofName,
                    ProofCode = bpt.ProofCode,
                    Description = bpt.Description,
                    IsActive = bpt.IsActive,
                    CreatedDate = bpt.CreatedDate,
                    UpdatedDate = bpt.UpdatedDate
                })
                .ToListAsync();
        }

        public async Task<PagedResponse<BusinessProofTypeMasterResponse>> GetBusinessProofTypeListAsync(GetBusinessProofTypeListRequest request)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 100 ? 100 : request.PageSize;

            var query = _context.BusinessProofTypes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(bpt =>
                    bpt.ProofName.ToLower().Contains(search) ||
                    bpt.ProofCode.ToLower().Contains(search) ||
                    (bpt.Description != null && bpt.Description.ToLower().Contains(search)));
            }

            if (request.IsActive.HasValue)
                query = query.Where(bpt => bpt.IsActive == request.IsActive.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(bpt => bpt.ProofName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(bpt => new BusinessProofTypeMasterResponse
                {
                    BusinessProofTypeId = bpt.BusinessProofTypeId,
                    ProofName = bpt.ProofName,
                    ProofCode = bpt.ProofCode,
                    Description = bpt.Description,
                    IsActive = bpt.IsActive,
                    CreatedDate = bpt.CreatedDate,
                    UpdatedDate = bpt.UpdatedDate
                })
                .ToListAsync();

            return new PagedResponse<BusinessProofTypeMasterResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<BusinessProofTypeMasterResponse> UpdateBusinessProofTypeAsync(UpdateBusinessProofTypeMasterRequest request)
        {
            var businessProofType = await _context.BusinessProofTypes
                .FirstOrDefaultAsync(bpt => bpt.BusinessProofTypeId == request.BusinessProofTypeId);

            if (businessProofType == null)
                throw new ArgumentException("Business proof type not found.");

            // Check if proof code already exists (excluding current record)
            var existingCode = await _context.BusinessProofTypes
                .AnyAsync(bpt => bpt.ProofCode == request.ProofCode && bpt.BusinessProofTypeId != request.BusinessProofTypeId);

            if (existingCode)
                throw new InvalidOperationException($"Business proof type with code '{request.ProofCode}' already exists.");

            // Check if proof name already exists (excluding current record)
            var existingName = await _context.BusinessProofTypes
                .AnyAsync(bpt => bpt.ProofName == request.ProofName && bpt.BusinessProofTypeId != request.BusinessProofTypeId);

            if (existingName)
                throw new InvalidOperationException($"Business proof type with name '{request.ProofName}' already exists.");

            businessProofType.ProofName = request.ProofName;
            businessProofType.ProofCode = request.ProofCode;
            businessProofType.Description = request.Description;
            if (request.IsActive.HasValue)
                businessProofType.IsActive = request.IsActive;
            businessProofType.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Business proof type updated successfully: {BusinessProofTypeId} - {ProofName}", 
                businessProofType.BusinessProofTypeId, businessProofType.ProofName);

            return new BusinessProofTypeMasterResponse
            {
                BusinessProofTypeId = businessProofType.BusinessProofTypeId,
                ProofName = businessProofType.ProofName,
                ProofCode = businessProofType.ProofCode,
                Description = businessProofType.Description,
                IsActive = businessProofType.IsActive,
                CreatedDate = businessProofType.CreatedDate,
                UpdatedDate = businessProofType.UpdatedDate
            };
        }

        public async Task<bool> DeleteBusinessProofTypeAsync(int businessProofTypeId)
        {
            var businessProofType = await _context.BusinessProofTypes
                .FirstOrDefaultAsync(bpt => bpt.BusinessProofTypeId == businessProofTypeId);

            if (businessProofType == null)
                return false;

            // Check if any documents are uploaded with this proof type
            var hasUploads = await _context.DocumentUploads
                .AnyAsync(du => du.BusinessProofTypeId == businessProofTypeId);

            if (hasUploads)
                throw new InvalidOperationException("Cannot delete business proof type. Documents are already uploaded with this type.");

            _context.BusinessProofTypes.Remove(businessProofType);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Business proof type deleted successfully: {BusinessProofTypeId}", businessProofTypeId);

            return true;
        }

        public async Task<bool> ToggleBusinessProofTypeStatusAsync(int businessProofTypeId)
        {
            var businessProofType = await _context.BusinessProofTypes
                .FirstOrDefaultAsync(bpt => bpt.BusinessProofTypeId == businessProofTypeId);

            if (businessProofType == null)
                return false;

            businessProofType.IsActive = !businessProofType.IsActive;
            businessProofType.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Business proof type status toggled: {BusinessProofTypeId} - IsActive: {IsActive}", 
                businessProofTypeId, businessProofType.IsActive);

            return true;
        }
    }
}
