using BankUPG.Application.Interfaces.BusinessCategoryMaster;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BankUPG.SharedKernal.Responses;
namespace BankUPG.Application.Services.BusinessCategoryMaster
{
    public class BusinessCategoryMasterService : IBusinessCategoryMasterService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<BusinessCategoryMasterService> _logger;

        public BusinessCategoryMasterService(
            AppDBContext context,
            ILogger<BusinessCategoryMasterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BusinessCategoryResponse> CreateBusinessCategoryAsync(CreateBusinessCategoryRequest request)
        {
            // Check duplicate Category Code
            var existingCode = await _context.BusinessCategories
                .AnyAsync(x => x.CategoryCode == request.CategoryCode);

            if (existingCode)
                throw new InvalidOperationException(
                    $"Business Category Code '{request.CategoryCode}' already exists.");

            // Check duplicate Category Name
            var existingName = await _context.BusinessCategories
                .AnyAsync(x => x.CategoryName == request.CategoryName);

            if (existingName)
                throw new InvalidOperationException(
                    $"Business Category '{request.CategoryName}' already exists.");

            var category = new BankUPG.Infrastructure.Entities.BusinessCategory
            {
                CategoryName = request.CategoryName,
                CategoryCode = request.CategoryCode,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.BusinessCategories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Business Category created successfully: {Id} - {Name}",
                category.BusinessCategoryId,
                category.CategoryName);

            return new BusinessCategoryResponse
            {
                BusinessCategoryId = category.BusinessCategoryId,
                CategoryName = category.CategoryName,
                CategoryCode = category.CategoryCode,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedDate = category.CreatedDate,
                UpdatedDate = category.UpdatedDate
            };
        }

        public async Task<BusinessCategoryResponse?> GetBusinessCategoryByIdAsync(int businessCategoryId)
        {
            var category = await _context.BusinessCategories
                .FirstOrDefaultAsync(x => x.BusinessCategoryId == businessCategoryId);

            if (category == null)
                return null;

            return new BusinessCategoryResponse
            {
                BusinessCategoryId = category.BusinessCategoryId,
                CategoryName = category.CategoryName,
                CategoryCode = category.CategoryCode,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedDate = category.CreatedDate,
                UpdatedDate = category.UpdatedDate
            };
        }

        public async Task<PagedResponse<BusinessCategoryResponse>> GetBusinessCategoryListAsync(GetBusinessCategoryListRequest request)
        {
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var query = _context.BusinessCategories.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();

                query = query.Where(x =>
                    x.CategoryName.ToLower().Contains(search) ||
                    x.CategoryCode.ToLower().Contains(search) ||
                    (x.Description != null && x.Description.ToLower().Contains(search)));
            }

            // Active Filter
            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.CategoryName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BusinessCategoryResponse
                {
                    BusinessCategoryId = x.BusinessCategoryId,
                    CategoryName = x.CategoryName,
                    CategoryCode = x.CategoryCode,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToListAsync();

            return new PagedResponse<BusinessCategoryResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<BusinessCategoryResponse> UpdateBusinessCategoryAsync(UpdateBusinessCategoryRequest request)
        {
            var category = await _context.BusinessCategories
                .FirstOrDefaultAsync(x => x.BusinessCategoryId == request.BusinessCategoryId);

            if (category == null)
                throw new ArgumentException("Business Category not found.");

            // Check duplicate Category Code
            var existingCode = await _context.BusinessCategories
                .AnyAsync(x => x.CategoryCode == request.CategoryCode &&
                               x.BusinessCategoryId != request.BusinessCategoryId);

            if (existingCode)
                throw new InvalidOperationException(
                    $"Business Category Code '{request.CategoryCode}' already exists.");

            // Check duplicate Category Name
            var existingName = await _context.BusinessCategories
                .AnyAsync(x => x.CategoryName == request.CategoryName &&
                               x.BusinessCategoryId != request.BusinessCategoryId);

            if (existingName)
                throw new InvalidOperationException(
                    $"Business Category '{request.CategoryName}' already exists.");

            // Update values
            category.CategoryName = request.CategoryName;
            category.CategoryCode = request.CategoryCode;
            category.Description = request.Description;

            if (request.IsActive.HasValue)
                category.IsActive = request.IsActive.Value;

            category.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Business Category updated successfully: {Id} - {Name}",
                category.BusinessCategoryId,
                category.CategoryName);

            return new BusinessCategoryResponse
            {
                BusinessCategoryId = category.BusinessCategoryId,
                CategoryName = category.CategoryName,
                CategoryCode = category.CategoryCode,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedDate = category.CreatedDate,
                UpdatedDate = category.UpdatedDate
            };
        }
    }
}