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
        private readonly ILogger<ConnectPlatformService> _logger;

        public ConnectPlatformService(
            AppDBContext context,
            ILogger<ConnectPlatformService> logger)
        {
            _context = context;
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

            var isServiceAgreementSubmitted = await _context.ServiceAgreements.AnyAsync(sa => sa.Mid == merchant.Mid);

            return new ConnectPlatformResponse
            {
                Mid = merchant.Mid,
                PaymentCollectionPreference = detail.PaymentCollectionPreference,
                WebsiteAppUrl = detail.WebsiteAppUrl,
                AndroidAppUrl = detail.AndroidAppUrl,
                IosAppUrl = detail.IOsappUrl,
                IsOnboardingCompleted = merchant.IsOnboardingCompleted ?? false,
                IsOnboardingRejected = merchant.IsOnboardingRejected ?? false,
                IsServiceAgreementSubmitted = isServiceAgreementSubmitted
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

            _logger.LogInformation("Connect platform details {Operation} for userId: {UserId}, mid: {Mid}",
                isUpdate ? "updated" : "saved", userId, mid);

            return new ConnectPlatformSavedResponse
            {
                Mid = mid,
                PaymentCollectionPreference = request.PaymentCollectionPreference,
                WebsiteAppUrl = request.WebsiteAppUrl,
                AndroidAppUrl = request.AndroidAppUrl,
                IosAppUrl = request.IosAppUrl,
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
                _ => "ConnectPlatformCompleted"
            };

            return (currentStepName, formStep, stepIndex);
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
                IsOnboardingCompleted = merchant.IsOnboardingCompleted ?? false,
                IsOnboardingRejected = merchant.IsOnboardingRejected ?? false,
                IsServiceAgreementSubmitted = isServiceAgreementSubmitted,
                Steps = steps
            };
        }
    }
}
