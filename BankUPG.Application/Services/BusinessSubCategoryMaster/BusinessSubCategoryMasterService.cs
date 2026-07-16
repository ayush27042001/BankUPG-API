using BankUPG.Application.Interfaces.BusinessSubCategoryMaster;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.BusinessSubCategoryMaster
{
    public class BusinessSubCategoryMasterService : IBusinessSubCategoryMasterService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<BusinessSubCategoryMasterService> _logger;

        public BusinessSubCategoryMasterService(
            AppDBContext context,
            ILogger<BusinessSubCategoryMasterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BusinessSubCategoryResponse> CreateBusinessSubCategoryAsync(CreateBusinessSubCategoryRequest request)
        {
            // Check duplicate SubCategory Code
            var existingCode = await _context.BusinessSubCategories
                .AnyAsync(x => x.SubCategoryCode == request.SubCategoryCode);

            if (existingCode)
                throw new InvalidOperationException(
                    $"Business Sub Category Code '{request.SubCategoryCode}' already exists.");

            // Check duplicate SubCategory Name
            var existingName = await _context.BusinessSubCategories
                .AnyAsync(x => x.SubCategoryName == request.SubCategoryName);

            if (existingName)
                throw new InvalidOperationException(
                    $"Business Sub Category '{request.SubCategoryName}' already exists.");

            var subCategory = new BusinessSubCategory
            {
                BusinessCategoryId = request.BusinessCategoryId,
                SubCategoryName = request.SubCategoryName,
                SubCategoryCode = request.SubCategoryCode,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.BusinessSubCategories.Add(subCategory);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Business Sub Category created successfully: {Id} - {Name}",
                subCategory.BusinessSubCategoryId,
                subCategory.SubCategoryName);

            return new BusinessSubCategoryResponse
            {
                BusinessSubCategoryId = subCategory.BusinessSubCategoryId,
                BusinessCategoryId = subCategory.BusinessCategoryId,
                CategoryName = string.Empty, // Join later if needed
                SubCategoryName = subCategory.SubCategoryName,
                SubCategoryCode = subCategory.SubCategoryCode,
                Description = subCategory.Description,
                IsActive = subCategory.IsActive,
                CreatedDate = subCategory.CreatedDate,
                UpdatedDate = subCategory.UpdatedDate
            };
        }

        public async Task<BusinessSubCategoryResponse?> GetBusinessSubCategoryByIdAsync(int businessSubCategoryId)
        {
            var subCategory = await _context.BusinessSubCategories
                .FirstOrDefaultAsync(x => x.BusinessSubCategoryId == businessSubCategoryId);

            if (subCategory == null)
                return null;

            return new BusinessSubCategoryResponse
            {
                BusinessSubCategoryId = subCategory.BusinessSubCategoryId,
                BusinessCategoryId = subCategory.BusinessCategoryId,
                CategoryName = string.Empty, // Join later if required
                SubCategoryName = subCategory.SubCategoryName,
                SubCategoryCode = subCategory.SubCategoryCode,
                Description = subCategory.Description,
                IsActive = subCategory.IsActive,
                CreatedDate = subCategory.CreatedDate,
                UpdatedDate = subCategory.UpdatedDate
            };
        }

        public async Task<PagedResponse<BusinessSubCategoryResponse>> GetBusinessSubCategoryListAsync(GetBusinessSubCategoryListRequest request)
        {
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var query = _context.BusinessSubCategories.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();

                query = query.Where(x =>
                    x.SubCategoryName.ToLower().Contains(search) ||
                    x.SubCategoryCode.ToLower().Contains(search) ||
                    (x.Description != null && x.Description.ToLower().Contains(search)));
            }

            // Active Filter
            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive);
            }

            // Business Category Filter
            if (request.BusinessCategoryId.HasValue)
            {
                query = query.Where(x => x.BusinessCategoryId == request.BusinessCategoryId);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SubCategoryName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BusinessSubCategoryResponse
                {
                    BusinessSubCategoryId = x.BusinessSubCategoryId,
                    BusinessCategoryId = x.BusinessCategoryId,
                    CategoryName = string.Empty, // Join later if required
                    SubCategoryName = x.SubCategoryName,
                    SubCategoryCode = x.SubCategoryCode,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToListAsync();

            return new PagedResponse<BusinessSubCategoryResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<BusinessSubCategoryResponse> UpdateBusinessSubCategoryAsync(UpdateBusinessSubCategoryRequest request)
        {
            var subCategory = await _context.BusinessSubCategories
                .FirstOrDefaultAsync(x => x.BusinessSubCategoryId == request.BusinessSubCategoryId);

            if (subCategory == null)
                throw new ArgumentException("Business Sub Category not found.");

            // Check duplicate SubCategory Code
            var existingCode = await _context.BusinessSubCategories
                .AnyAsync(x => x.SubCategoryCode == request.SubCategoryCode &&
                               x.BusinessSubCategoryId != request.BusinessSubCategoryId);

            if (existingCode)
                throw new InvalidOperationException(
                    $"Business Sub Category Code '{request.SubCategoryCode}' already exists.");

            // Check duplicate SubCategory Name
            var existingName = await _context.BusinessSubCategories
                .AnyAsync(x => x.SubCategoryName == request.SubCategoryName &&
                               x.BusinessSubCategoryId != request.BusinessSubCategoryId);

            if (existingName)
                throw new InvalidOperationException(
                    $"Business Sub Category '{request.SubCategoryName}' already exists.");

            // Update Values
            subCategory.BusinessCategoryId = request.BusinessCategoryId;
            subCategory.SubCategoryName = request.SubCategoryName;
            subCategory.SubCategoryCode = request.SubCategoryCode;
            subCategory.Description = request.Description;

            if (request.IsActive.HasValue)
                subCategory.IsActive = request.IsActive.Value;

            subCategory.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Business Sub Category updated successfully: {Id} - {Name}",
                subCategory.BusinessSubCategoryId,
                subCategory.SubCategoryName);

            return new BusinessSubCategoryResponse
            {
                BusinessSubCategoryId = subCategory.BusinessSubCategoryId,
                BusinessCategoryId = subCategory.BusinessCategoryId,
                CategoryName = string.Empty, // Join later if needed
                SubCategoryName = subCategory.SubCategoryName,
                SubCategoryCode = subCategory.SubCategoryCode,
                Description = subCategory.Description,
                IsActive = subCategory.IsActive,
                CreatedDate = subCategory.CreatedDate,
                UpdatedDate = subCategory.UpdatedDate
            };
        }
    }
}