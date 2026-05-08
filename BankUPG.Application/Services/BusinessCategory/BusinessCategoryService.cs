using BankUPG.Application.Interfaces.BusinessCategory;
using BankUPG.Application.Services.Auth;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Models;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.BusinessCategory
{
    public class BusinessCategoryService : IBusinessCategoryService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<BusinessCategoryService> _logger;

        public BusinessCategoryService(
            AppDBContext context,
            ILogger<BusinessCategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<BusinessCategoryDto>> GetAllCategoriesAsync()
        {
            return await _context.BusinessCategories
                .AsNoTracking()
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.CategoryName)
                .Select(c => new BusinessCategoryDto
                {
                    BusinessCategoryId = c.BusinessCategoryId,
                    CategoryName = c.CategoryName,
                    CategoryCode = c.CategoryCode,
                    Description = c.Description,
                    SubCategories = c.BusinessSubCategories
                        .Where(s => s.IsActive == true)
                        .OrderBy(s => s.SubCategoryName)
                        .Select(s => new BusinessSubCategoryDto
                        {
                            BusinessSubCategoryId = s.BusinessSubCategoryId,
                            BusinessCategoryId = s.BusinessCategoryId,
                            SubCategoryName = s.SubCategoryName,
                            SubCategoryCode = s.SubCategoryCode,
                            Description = s.Description
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<MerchantBusinessCategoryResponse?> GetBusinessCategoryAsync(int userId)
        {
            var merchant = await _context.Merchants
                .AsNoTracking()
                .Include(m => m.BusinessCategory)
                .Include(m => m.BusinessSubCategory)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null || merchant.BusinessCategoryId == null)
                return null;

            return new MerchantBusinessCategoryResponse
            {
                Mid = merchant.Mid,
                BusinessCategoryId = merchant.BusinessCategoryId,
                CategoryName = merchant.BusinessCategory?.CategoryName,
                BusinessSubCategoryId = merchant.BusinessSubCategoryId,
                SubCategoryName = merchant.BusinessSubCategory?.SubCategoryName
            };
        }

        public async Task<BusinessCategorySavedResponse> SaveBusinessCategoryAsync(int userId, SaveBusinessCategoryRequest request)
        {
            var merchant = await _context.Merchants
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null || merchant.User == null)
                throw new InvalidOperationException("User or merchant not found. Please ensure you are logged in.");

            var category = await _context.BusinessCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.BusinessCategoryId == request.BusinessCategoryId && c.IsActive == true);

            if (category == null)
                throw new ArgumentException("Invalid business category selected.");

            var subCategory = await _context.BusinessSubCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.BusinessSubCategoryId == request.BusinessSubCategoryId
                    && s.BusinessCategoryId == request.BusinessCategoryId
                    && s.IsActive == true);

            if (subCategory == null)
                throw new ArgumentException("Invalid business sub-category selected, or it does not belong to the selected category.");

            var mid = merchant.Mid;
            bool isUpdate = merchant.BusinessCategoryId.HasValue;

            merchant.BusinessCategoryId = request.BusinessCategoryId;
            merchant.BusinessSubCategoryId = request.BusinessSubCategoryId;
            merchant.UpdatedDate = DateTime.UtcNow;

            if (merchant.OnboardingStatusId < (int)OnboardingStatusEnum.BusinessCategory)
                merchant.OnboardingStatusId = (int)OnboardingStatusEnum.BusinessCategory;

            var existingStepTracking = await _context.OnboardingStepTrackings
                .FirstOrDefaultAsync(s => s.Mid == mid && s.StepName == "Business Category");

            if (existingStepTracking != null)
            {
                existingStepTracking.StepStatus = "COMPLETED";
                existingStepTracking.CompletedDate = DateTime.UtcNow;
                existingStepTracking.UpdatedDate = DateTime.UtcNow;
            }
            else
            {
                _context.OnboardingStepTrackings.Add(new OnboardingStepTracking
                {
                    Mid = mid,
                    StepName = "Business Category",
                    StepKey = "BUSINESS_CATEGORY",
                    StepStatus = "COMPLETED",
                    IsCompleted = true,
                    CompletedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            var (currentStepName, formStep, step) = await GetOnboardingStepInfoAsync(mid);
            var onboardingStatus = await BuildOnboardingStatusAsync(mid);

            _logger.LogInformation("Business category {Operation} for userId: {UserId}, mid: {Mid}, categoryId: {CategoryId}, subCategoryId: {SubCategoryId}",
                isUpdate ? "updated" : "saved", userId, mid, request.BusinessCategoryId, request.BusinessSubCategoryId);

            return new BusinessCategorySavedResponse
            {
                Mid = mid,
                BusinessCategoryId = request.BusinessCategoryId,
                CategoryName = category.CategoryName,
                BusinessSubCategoryId = request.BusinessSubCategoryId,
                SubCategoryName = subCategory.SubCategoryName,
                Message = isUpdate ? "Business category updated successfully" : "Business category saved successfully",
                FormStep = formStep,
                Step = step,
                OnboardingStatus = onboardingStatus
            };
        }

        private async Task<(string stepName, string formStep, int step)> GetOnboardingStepInfoAsync(int mid)
        {
            var stepOrder = new[]
            {
                "PAN Verification",
                "Business Entity",
                "Phone CKYC",
                "Business Category",
                "Share Business Details",
                "Connect Platform"
            };

            var completedSteps = await _context.OnboardingStepTrackings
                .Where(s => s.Mid == mid && s.StepStatus == "COMPLETED")
                .Select(s => s.StepName)
                .ToListAsync();

            string currentStepName = "PAN Verification";
            int stepIndex = 1;
            bool foundIncomplete = false;

            foreach (var step in stepOrder)
            {
                if (!completedSteps.Contains(step))
                {
                    currentStepName = step;
                    foundIncomplete = true;
                    break;
                }
                stepIndex++;
            }

            if (!foundIncomplete)
            {
                currentStepName = "Completed";
                stepIndex = 7;
            }

            string formStep = currentStepName switch
            {
                "PAN Verification" => "BusinessPANCompletedBusinessEntityPending",
                "Business Entity" => "BusinessEntityCompletedPhoneCKYCPending",
                "Phone CKYC" => "PhoneCKYCCompletedBusinessCategoryPending",
                "Business Category" => "BusinessCategoryCompletedShareDetailsPending",
                "Share Business Details" => "ShareDetailsCompletedConnectPlatformPending",
                "Connect Platform" => "ConnectPlatformCompleted",
                "Completed" => "RegistrationCompleted",
                _ => "BusinessCategoryCompletedShareDetailsPending"
            };

            return (currentStepName, formStep, stepIndex);
        }

        private async Task<OnboardingStatusDto> BuildOnboardingStatusAsync(int mid)
        {
            var stepOrder = new[]
            {
                new { StepNumber = 1, StepName = "PAN Verification", StepKey = "PAN_VERIFICATION" },
                new { StepNumber = 2, StepName = "Business Entity", StepKey = "BUSINESS_ENTITY" },
                new { StepNumber = 3, StepName = "Phone CKYC", StepKey = "PHONE_CKYC" },
                new { StepNumber = 4, StepName = "Business Category", StepKey = "BUSINESS_CATEGORY" },
                new { StepNumber = 5, StepName = "Share Business Details", StepKey = "SHARE_BUSINESS_DETAILS" },
                new { StepNumber = 6, StepName = "Connect Platform", StepKey = "CONNECT_PLATFORM" }
            };

            var completedSteps = await _context.OnboardingStepTrackings
                .Where(s => s.Mid == mid && s.StepStatus == "COMPLETED")
                .Select(s => s.StepName)
                .ToListAsync();

            int currentStepIndex = 1;
            string currentStepName = "PAN Verification";
            bool allCompleted = true;

            foreach (var step in stepOrder)
            {
                if (!completedSteps.Contains(step.StepName))
                {
                    currentStepIndex = step.StepNumber;
                    currentStepName = step.StepName;
                    allCompleted = false;
                    break;
                }
            }

            if (allCompleted)
            {
                currentStepIndex = 7;
                currentStepName = "Completed";
            }

            var steps = stepOrder.Select(step => new OnboardingStepDto
            {
                StepNumber = step.StepNumber,
                StepName = step.StepName,
                StepKey = step.StepKey,
                IsCompleted = completedSteps.Contains(step.StepName),
                IsActive = step.StepName == currentStepName
            }).ToList();

            var isServiceAgreementSubmitted = await _context.ServiceAgreements.AnyAsync(sa => sa.Mid == mid);
            var connectPlatformSteps = await BuildConnectPlatformStepsAsync(mid);
            var merchant = await _context.Merchants.AsNoTracking().FirstOrDefaultAsync(m => m.Mid == mid);

            steps = stepOrder.Select(step => new OnboardingStepDto
            {
                StepNumber = step.StepNumber,
                StepName = step.StepName,
                StepKey = step.StepKey,
                IsCompleted = step.StepName == "Connect Platform" ? connectPlatformSteps.Steps.All(s => s.IsCompleted) : completedSteps.Contains(step.StepName),
                IsActive = step.StepName == currentStepName,
                ConnectPlatformSteps = step.StepName == "Connect Platform" ? connectPlatformSteps : null
            }).ToList();

            return new OnboardingStatusDto
            {
                StepNumber = currentStepIndex,
                StepName = currentStepName,
                IsCompleted = allCompleted,
                IsOnboardingCompleted = merchant?.IsOnboardingCompleted ?? false,
                IsServiceAgreementSubmitted = isServiceAgreementSubmitted,
                Steps = steps
            };
        }

        private async Task<ConnectPlatformStepsDto> BuildConnectPlatformStepsAsync(int mid)
        {
            var connectPlatformStepOrder = new[]
            {
                new { StepNumber = 1, StepName = "Connect Mobile App or Website", StepKey = "CONNECT_MOBILE_APP_OR_WEBSITE" },
                new { StepNumber = 2, StepName = "Share Bank Account Details", StepKey = "SHARE_BANK_ACCOUNT_DETAILS" },
                new { StepNumber = 3, StepName = "Signing Authority Details", StepKey = "SIGNING_AUTHORITY_DETAILS" },
                new { StepNumber = 4, StepName = "Verify Business Address", StepKey = "VERIFY_BUSINESS_ADDRESS" },
                new { StepNumber = 5, StepName = "Complete Video KYC", StepKey = "COMPLETE_VIDEO_KYC" },
                new { StepNumber = 6, StepName = "Service Agreement", StepKey = "SERVICE_AGREEMENT" }
            };

            var completionMap = new Dictionary<string, bool>
            {
                { "CONNECT_MOBILE_APP_OR_WEBSITE", await _context.WebsiteAppDetails.AnyAsync(w => w.Mid == mid) },
                { "SHARE_BANK_ACCOUNT_DETAILS", await _context.BankAccountDetails.AnyAsync(b => b.Mid == mid) },
                { "SIGNING_AUTHORITY_DETAILS", await _context.SigningAuthorityDetails.AnyAsync(s => s.Mid == mid) },
                { "VERIFY_BUSINESS_ADDRESS", await _context.BusinessAddressDetails.AnyAsync(b => b.Mid == mid) },
                { "COMPLETE_VIDEO_KYC", await _context.VideoKycdetails.AnyAsync(v => v.Mid == mid && v.VideoKycstatus == "Completed") },
                { "SERVICE_AGREEMENT", await _context.ServiceAgreements.AnyAsync(sa => sa.Mid == mid) }
            };

            int currentStepNumber = 7;
            bool allCompleted = true;

            foreach (var step in connectPlatformStepOrder)
            {
                if (!completionMap[step.StepKey])
                {
                    currentStepNumber = step.StepNumber;
                    allCompleted = false;
                    break;
                }
            }

            return new ConnectPlatformStepsDto
            {
                CurrentStep = currentStepNumber,
                TotalSteps = 6,
                Steps = connectPlatformStepOrder.Select(s => new ConnectPlatformStepDto
                {
                    StepNumber = s.StepNumber,
                    StepName = s.StepName,
                    StepKey = s.StepKey,
                    IsCompleted = completionMap[s.StepKey],
                    IsActive = s.StepNumber == currentStepNumber && !allCompleted
                }).ToList()
            };
        }
    }
}
