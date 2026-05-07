using BankUPG.Application.Interfaces.BusinessEntity;
using BankUPG.Application.Services.Auth;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Models;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.BusinessEntity
{
    public class BusinessEntityService : IBusinessEntityService
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly AppSettings _appSettings;
        private readonly ILogger<BusinessEntityService> _logger;

        public BusinessEntityService(
            AppDBContext context,
            JwtService jwtService,
            AppSettings appSettings,
            ILogger<BusinessEntityService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task<List<BusinessEntityTypeDto>> GetBusinessEntityTypesAsync()
        {
            return await _context.BusinessEntityTypes
                .AsNoTracking()
                .Where(b => b.IsActive == true)
                .OrderBy(b => b.BusinessEntityTypeId)
                .Select(b => new BusinessEntityTypeDto
                {
                    BusinessEntityTypeId = b.BusinessEntityTypeId,
                    EntityName = b.EntityName,
                    Description = b.Description
                })
                .ToListAsync();
        }

        public async Task<BusinessEntityResponse?> GetBusinessEntityAsync(int userId)
        {
            var merchant = await _context.Merchants
                .AsNoTracking()
                .Include(m => m.BusinessEntityType)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null || merchant.BusinessEntityTypeId == null)
                return null;

            return new BusinessEntityResponse
            {
                Mid = merchant.Mid,
                BusinessEntityTypeId = merchant.BusinessEntityTypeId.Value,
                EntityName = merchant.BusinessEntityType?.EntityName ?? string.Empty,
                Description = merchant.BusinessEntityType?.Description
            };
        }

        public async Task<BusinessEntitySavedResponse> SaveBusinessEntityAsync(int userId, SaveBusinessEntityRequest request)
        {
            var merchant = await _context.Merchants
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null || merchant.User == null)
                throw new InvalidOperationException("User or merchant not found. Please ensure you are logged in.");

            // Validate the BusinessEntityTypeId exists
            var entityType = await _context.BusinessEntityTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BusinessEntityTypeId == request.BusinessEntityTypeId && b.IsActive == true);

            if (entityType == null)
                throw new ArgumentException("Invalid business entity type selected.");

            var mid = merchant.Mid;
            bool isUpdate = merchant.BusinessEntityTypeId.HasValue;

            // Update merchant's business entity type
            merchant.BusinessEntityTypeId = request.BusinessEntityTypeId;
            merchant.UpdatedDate = DateTime.UtcNow;

            // Update OnboardingStatusId if not already past this step
            if (merchant.OnboardingStatusId < (int)OnboardingStatusEnum.BusinessEntity)
                merchant.OnboardingStatusId = (int)OnboardingStatusEnum.BusinessEntity;

            // Upsert OnboardingStepTracking
            var existingStepTracking = await _context.OnboardingStepTrackings
                .FirstOrDefaultAsync(s => s.Mid == mid && s.StepName == "Business Entity");

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
                    StepName = "Business Entity",
                    StepKey = "BUSINESS_ENTITY",
                    StepStatus = "COMPLETED",
                    IsCompleted = true,
                    CompletedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            // Build onboarding status from OnboardingStepTracking table
            var (currentStepName, formStep, step) = await GetOnboardingStepInfoAsync(mid);
            var onboardingStatus = await BuildOnboardingStatusAsync(mid);

            // Generate new JWT token
            var token = _jwtService.GenerateToken(
                merchant.User.Email,
                merchant.User.Email,
                string.Empty,
                merchant.User.UserId
            );

            _logger.LogInformation("Business entity {Operation} for userId: {UserId}, mid: {Mid}, entityTypeId: {EntityTypeId}",
                isUpdate ? "updated" : "saved", userId, mid, request.BusinessEntityTypeId);

            return new BusinessEntitySavedResponse
            {
                Mid = mid,
                BusinessEntityTypeId = request.BusinessEntityTypeId,
                EntityName = entityType.EntityName,
                Token = token,
                TokenExpiration = DateTime.UtcNow.AddMinutes(_appSettings.Jwt.ExpirationMinutes),
                Message = isUpdate ? "Business entity updated successfully" : "Business entity saved successfully",
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
                "Connect Platform",
                "Upload Documents",
                "Service Agreement"
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
                stepIndex = 9;
            }

            string formStep = currentStepName switch
            {
                "PAN Verification" => "BusinessPANCompletedBusinessEntityPending",
                "Business Entity" => "BusinessEntityCompletedPhoneCKYCPending",
                "Phone CKYC" => "PhoneCKYCCompletedBusinessCategoryPending",
                "Business Category" => "BusinessCategoryCompletedShareDetailsPending",
                "Share Business Details" => "ShareDetailsCompletedConnectPlatformPending",
                "Connect Platform" => "ConnectPlatformCompletedUploadDocsPending",
                "Upload Documents" => "UploadDocsCompletedServiceAgreementPending",
                "Service Agreement" => "ServiceAgreementCompleted",
                "Completed" => "RegistrationCompleted",
                _ => "BusinessEntityCompletedPhoneCKYCPending"
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
                new { StepNumber = 6, StepName = "Connect Platform", StepKey = "CONNECT_PLATFORM" },
                new { StepNumber = 7, StepName = "Upload Documents", StepKey = "UPLOAD_DOCUMENTS" },
                new { StepNumber = 8, StepName = "Service Agreement", StepKey = "SERVICE_AGREEMENT" }
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
                currentStepIndex = 9;
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

            return new OnboardingStatusDto
            {
                StepNumber = currentStepIndex,
                StepName = currentStepName,
                IsCompleted = allCompleted,
                Steps = steps
            };
        }
    }
}
