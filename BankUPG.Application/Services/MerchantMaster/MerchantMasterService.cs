using BankUPG.Application.Interfaces.MerchantMaster;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.MerchantMaster
{
    public class MerchantMasterService : IMerchantMasterService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<MerchantMasterService> _logger;

        public MerchantMasterService(
            AppDBContext context,
            ILogger<MerchantMasterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MerchantResponse> CreateMerchantAsync(CreateMerchantRequest request)
        {
            var merchant = new Merchant
            {
                UserId = request.UserId,
                BusinessName = request.BusinessName,
                BusinessEntityTypeId = request.BusinessEntityTypeId,
                BusinessCategoryId = request.BusinessCategoryId,
                BusinessSubCategoryId = request.BusinessSubCategoryId,
                ExpectedSalesPerMonth = request.ExpectedSalesPerMonth,
                HasGstin = request.HasGstin,
                Gstin = request.Gstin,
                Ckycidentifier = request.Ckycidentifier,
                CkycconsentGiven = request.CkycconsentGiven,
                CkycconsentDate = request.CkycconsentGiven == true ? DateTime.UtcNow : null,

                OnboardingStatusId = 1,
                IsActive = true,
                IsOnboardingCompleted = false,
                IsOnboardingRejected = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Merchants.Add(merchant);
            await _context.SaveChangesAsync();

            return new MerchantResponse
            {
                Mid = merchant.Mid,
                UserId = merchant.UserId,
                BusinessName = merchant.BusinessName,
                BusinessEntityTypeId = merchant.BusinessEntityTypeId,
                OnboardingStatusId = merchant.OnboardingStatusId,
                Ckycidentifier = merchant.Ckycidentifier,
                ExpectedSalesPerMonth = merchant.ExpectedSalesPerMonth,
                Gstin = merchant.Gstin,
                IsActive = merchant.IsActive,
                CreatedDate = merchant.CreatedDate,
                UpdatedDate = merchant.UpdatedDate
            };
        }

        public async Task<MerchantResponse?> GetMerchantByIdAsync(int merchantId)
        {
            var merchant = await _context.Merchants
                .FirstOrDefaultAsync(x => x.Mid == merchantId);

            if (merchant == null)
                return null;

            return new MerchantResponse
            {
                Mid = merchant.Mid,
                UserId = merchant.UserId,
                BusinessName = merchant.BusinessName,
                BusinessEntityTypeId = merchant.BusinessEntityTypeId,
                OnboardingStatusId = merchant.OnboardingStatusId,
                Ckycidentifier = merchant.Ckycidentifier,
                ExpectedSalesPerMonth = merchant.ExpectedSalesPerMonth,
                Gstin = merchant.Gstin,
                IsActive = merchant.IsActive,
                CreatedDate = merchant.CreatedDate,
                UpdatedDate = merchant.UpdatedDate
            };
        }

        public async Task<PagedResponse<MerchantResponse>> GetMerchantListAsync(GetMerchantListRequest request)
        {
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var query = _context.Merchants.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();

                query = query.Where(x =>
                    (x.BusinessName != null && x.BusinessName.ToLower().Contains(search)) ||
                    (x.Gstin != null && x.Gstin.ToLower().Contains(search)) ||
                    (x.Ckycidentifier != null && x.Ckycidentifier.ToLower().Contains(search)));
            }

            // Active Filter
            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive);
            }

            var totalCount = await query.CountAsync();

            var merchants = await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new MerchantResponse
                {
                    Mid = x.Mid,
                    UserId = x.UserId,
                    BusinessName = x.BusinessName,
                    BusinessEntityTypeId = x.BusinessEntityTypeId,
                    OnboardingStatusId = x.OnboardingStatusId,
                    Ckycidentifier = x.Ckycidentifier,
                    ExpectedSalesPerMonth = x.ExpectedSalesPerMonth,
                    Gstin = x.Gstin,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToListAsync();

            return new PagedResponse<MerchantResponse>
            {
                Items = merchants,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}