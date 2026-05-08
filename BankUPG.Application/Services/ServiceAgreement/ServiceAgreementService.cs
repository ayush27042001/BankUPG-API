using BankUPG.Application.Interfaces.ServiceAgreement;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.ServiceAgreement
{
    public class ServiceAgreementService : IServiceAgreementService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<ServiceAgreementService> _logger;

        public ServiceAgreementService(
            AppDBContext context,
            ILogger<ServiceAgreementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceAgreementResponse?> GetServiceAgreementAsync(int userId)
        {
            var merchant = await _context.Merchants
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null)
            {
                _logger.LogWarning("Merchant not found for userId: {UserId}", userId);
                return null;
            }

            var agreement = await _context.ServiceAgreements
                .AsNoTracking()
                .FirstOrDefaultAsync(sa => sa.Mid == merchant.Mid);

            if (agreement == null)
            {
                _logger.LogInformation("Service agreement not found for merchant: {Mid}", merchant.Mid);
                return null;
            }

            return new ServiceAgreementResponse
            {
                ServiceAgreementId = agreement.ServiceAgreementId,
                Mid = agreement.Mid,
                SignatureData = agreement.SignatureData,
                AgreementDate = agreement.AgreementDate,
                IsAccepted = agreement.IsAccepted,
                SubmittedDate = agreement.SubmittedDate
            };
        }

        public async Task<ServiceAgreementSavedResponse> SaveServiceAgreementAsync(int userId, SaveServiceAgreementRequest request)
        {
            var merchant = await _context.Merchants
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null || merchant.User == null)
                throw new InvalidOperationException("User or merchant not found. Please ensure you are logged in.");

            if (!request.IsAccepted)
                throw new ArgumentException("You must accept the service agreement terms and conditions.");

            var mid = merchant.Mid;

            var existingAgreement = await _context.ServiceAgreements
                .FirstOrDefaultAsync(sa => sa.Mid == mid);

            bool isUpdate = existingAgreement != null;

            if (isUpdate)
            {
                existingAgreement!.SignatureData = request.SignatureData;
                existingAgreement.AgreementDate = request.AgreementDate;
                existingAgreement.IsAccepted = request.IsAccepted;
                existingAgreement.SubmittedDate = DateTime.UtcNow;
                existingAgreement.UpdatedDate = DateTime.UtcNow;
            }
            else
            {
                _context.ServiceAgreements.Add(new BankUPG.Infrastructure.Entities.ServiceAgreement
                {
                    Mid = mid,
                    SignatureData = request.SignatureData,
                    AgreementDate = request.AgreementDate,
                    IsAccepted = request.IsAccepted,
                    SubmittedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                });
            }

            var existingStepTracking = await _context.OnboardingStepTrackings
                .FirstOrDefaultAsync(s => s.Mid == mid && s.StepName == "Service Agreement");

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
                    StepName = "Service Agreement",
                    StepKey = "SERVICE_AGREEMENT",
                    StepStatus = "COMPLETED",
                    IsCompleted = true,
                    CompletedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                });
            }

            if (merchant.OnboardingStatusId < (int)OnboardingStatusEnum.ConnectPlatform)
                merchant.OnboardingStatusId = (int)OnboardingStatusEnum.ConnectPlatform;

            merchant.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var savedAgreement = await _context.ServiceAgreements.FirstOrDefaultAsync(sa => sa.Mid == mid);
            var onboardingStatus = await BuildOnboardingStatusAsync(mid);

            _logger.LogInformation("Service agreement {Operation} for userId: {UserId}, mid: {Mid}",
                isUpdate ? "updated" : "saved", userId, mid);

            return new ServiceAgreementSavedResponse
            {
                ServiceAgreementId = savedAgreement!.ServiceAgreementId,
                Mid = mid,
                SignatureData = savedAgreement.SignatureData,
                AgreementDate = savedAgreement.AgreementDate,
                IsAccepted = savedAgreement.IsAccepted,
                SubmittedDate = savedAgreement.SubmittedDate,
                Message = isUpdate ? "Service agreement updated successfully" : "Service agreement submitted successfully",
                OnboardingStatus = onboardingStatus
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

            return new OnboardingStatusDto
            {
                StepNumber = currentStepIndex,
                StepName = currentStepName,
                IsCompleted = allCompleted,
                IsOnboardingCompleted = merchant?.IsOnboardingCompleted ?? false,
                IsServiceAgreementSubmitted = isServiceAgreementSubmitted,
                Steps = steps,
                ConnectPlatformSteps = connectPlatformSteps
            };
        }
    }
}
