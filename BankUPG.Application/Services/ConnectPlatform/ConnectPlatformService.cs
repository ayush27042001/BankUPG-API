using BankUPG.Application.Interfaces.ConnectPlatform;
using BankUPG.Application.Services.Auth;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Models;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.ConnectPlatform
{
    public class ConnectPlatformService : IConnectPlatformService
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly AppSettings _appSettings;
        private readonly ILogger<ConnectPlatformService> _logger;

        public ConnectPlatformService(
            AppDBContext context,
            JwtService jwtService,
            AppSettings appSettings,
            ILogger<ConnectPlatformService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task<ConnectPlatformResponse?> GetConnectPlatformAsync(int userId)
        {
            var merchant = await _context.Merchants
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null)
            {
                _logger.LogWarning("Merchant not found for userId: {UserId}", userId);
                return null;
            }

            var detail = await _context.WebsiteAppDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Mid == merchant.Mid);

            if (detail == null)
            {
                _logger.LogInformation("Connect platform details not found for merchant: {Mid}", merchant.Mid);
                return null;
            }

            return new ConnectPlatformResponse
            {
                Mid = merchant.Mid,
                PaymentCollectionPreference = detail.PaymentCollectionPreference,
                WebsiteAppUrl = detail.WebsiteAppUrl,
                AndroidAppUrl = detail.AndroidAppUrl,
                IosAppUrl = detail.IOsappUrl
            };
        }

        public async Task<ConnectPlatformSavedResponse> SaveConnectPlatformAsync(int userId, SaveConnectPlatformRequest request)
        {
            var merchant = await _context.Merchants
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null || merchant.User == null)
                throw new InvalidOperationException("User or merchant not found. Please ensure you are logged in.");

            if (request.PaymentCollectionPreference == "ON_MY_WEBSITE_APP" &&
                string.IsNullOrWhiteSpace(request.WebsiteAppUrl))
            {
                throw new ArgumentException("Website/App URL is required when payment collection preference is 'On my website/app'.");
            }

            var mid = merchant.Mid;

            var existingDetail = await _context.WebsiteAppDetails
                .FirstOrDefaultAsync(w => w.Mid == mid);

            bool isUpdate = existingDetail != null;

            if (isUpdate)
            {
                existingDetail!.PaymentCollectionPreference = request.PaymentCollectionPreference;
                existingDetail.WebsiteAppUrl = request.WebsiteAppUrl;
                existingDetail.AndroidAppUrl = request.AndroidAppUrl;
                existingDetail.IOsappUrl = request.IosAppUrl;
                existingDetail.UpdatedDate = DateTime.UtcNow;
            }
            else
            {
                _context.WebsiteAppDetails.Add(new WebsiteAppDetail
                {
                    Mid = mid,
                    PaymentCollectionPreference = request.PaymentCollectionPreference,
                    WebsiteAppUrl = request.WebsiteAppUrl,
                    AndroidAppUrl = request.AndroidAppUrl,
                    IOsappUrl = request.IosAppUrl,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                });
            }

            // Upsert OnboardingStepTracking
            var existingStepTracking = await _context.OnboardingStepTrackings
                .FirstOrDefaultAsync(s => s.Mid == mid && s.StepName == "Connect Platform");

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
                    StepName = "Connect Platform",
                    StepKey = "CONNECT_PLATFORM",
                    StepStatus = "COMPLETED",
                    IsCompleted = true,
                    CompletedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                });
            }

            // Update merchant onboarding status if not already past this step
            if (merchant.OnboardingStatusId < (int)OnboardingStatusEnum.ConnectPlatform)
                merchant.OnboardingStatusId = (int)OnboardingStatusEnum.ConnectPlatform;

            merchant.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var (currentStepName, formStep, step) = await GetOnboardingStepInfoAsync(mid);
            var onboardingStatus = await BuildOnboardingStatusAsync(mid);

            var token = _jwtService.GenerateToken(
                merchant.User.Email,
                merchant.User.Email,
                string.Empty,
                merchant.User.UserId
            );

            _logger.LogInformation("Connect platform details {Operation} for userId: {UserId}, mid: {Mid}",
                isUpdate ? "updated" : "saved", userId, mid);

            return new ConnectPlatformSavedResponse
            {
                Mid = mid,
                PaymentCollectionPreference = request.PaymentCollectionPreference,
                WebsiteAppUrl = request.WebsiteAppUrl,
                AndroidAppUrl = request.AndroidAppUrl,
                IosAppUrl = request.IosAppUrl,
                Token = token,
                TokenExpiration = DateTime.UtcNow.AddMinutes(_appSettings.Jwt.ExpirationMinutes),
                Message = isUpdate ? "Connect platform details updated successfully" : "Connect platform details saved successfully",
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
                _ => "ConnectPlatformCompletedUploadDocsPending"
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
