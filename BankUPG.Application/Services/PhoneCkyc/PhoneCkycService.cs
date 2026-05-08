using BankUPG.Application.Interfaces.PhoneCkyc;
using BankUPG.Application.Services.Auth;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Models;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.PhoneCkyc
{
    public class PhoneCkycService : IPhoneCkycService
    {
        private readonly AppDBContext _context;
        private readonly OtpService _otpService;
        private readonly JwtService _jwtService;
        private readonly AppSettings _appSettings;
        private readonly ILogger<PhoneCkycService> _logger;

        public PhoneCkycService(
            AppDBContext context,
            OtpService otpService,
            JwtService jwtService,
            AppSettings appSettings,
            ILogger<PhoneCkycService> logger)
        {
            _context = context;
            _otpService = otpService;
            _jwtService = jwtService;
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task<OtpResponse> SendOtpAsync(int userId)
        {
            var merchant = await _context.Merchants
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null || merchant.User == null)
                throw new InvalidOperationException("User or merchant not found.");

            var mobileNumber = merchant.User.MobileNumber;
            if (string.IsNullOrEmpty(mobileNumber))
                throw new InvalidOperationException("Mobile number not found for user.");

            await _otpService.GenerateOtpAsync(
                mobileNumber,
                "PHONE_CKYC",
                userId,
                merchant.Mid
            );

            var remainingSeconds = await _otpService.GetRemainingTimeAsync(mobileNumber);

            _logger.LogInformation("OTP sent for Phone CKYC to userId: {UserId}, mobile: {Mobile}", userId, mobileNumber);

            return new OtpResponse
            {
                Success = true,
                Message = "OTP sent successfully to your registered mobile number",
                RemainingSeconds = remainingSeconds
            };
        }

        public async Task<OtpVerificationResponse> VerifyOtpAsync(int userId, string otp)
        {
            var merchant = await _context.Merchants
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null || merchant.User == null)
                throw new InvalidOperationException("User or merchant not found.");

            var mobileNumber = merchant.User.MobileNumber;
            if (string.IsNullOrEmpty(mobileNumber))
                throw new InvalidOperationException("Mobile number not found for user.");

            var isVerified = await _otpService.VerifyOtpAsync(mobileNumber, otp);

            _logger.LogInformation("OTP verification for Phone CKYC: {Result} for userId: {UserId}",
                isVerified ? "Success" : "Failed", userId);

            return new OtpVerificationResponse
            {
                IsVerified = isVerified,
                Message = isVerified ? "OTP verified successfully" : "Invalid OTP or OTP expired"
            };
        }

        public async Task<PhoneCkycResponse?> GetPhoneCkycAsync(int userId)
        {
            var merchant = await _context.Merchants
                .Include(m => m.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null || merchant.User == null)
                return null;

            return new PhoneCkycResponse
            {
                Mid = merchant.Mid,
                MobileNumber = $"+91{merchant.User.MobileNumber}",
                CkycIdentifier = merchant.Ckycidentifier,
                ConsentGiven = merchant.CkycconsentGiven ?? false,
                ConsentDate = merchant.CkycconsentDate
            };
        }

        public async Task<PhoneCkycSavedResponse> SavePhoneCkycAsync(int userId, SavePhoneCkycRequest request)
        {
            var merchant = await _context.Merchants
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null || merchant.User == null)
                throw new InvalidOperationException("User or merchant not found. Please ensure you are logged in.");

            var mid = merchant.Mid;
            bool isUpdate = merchant.CkycconsentGiven.HasValue || merchant.Ckycidentifier != null;

            merchant.Ckycidentifier = request.CkycIdentifier;
            merchant.CkycconsentGiven = request.ConsentGiven;
            merchant.CkycconsentDate = request.ConsentGiven ? DateTime.UtcNow : null;
            merchant.UpdatedDate = DateTime.UtcNow;

            if (merchant.OnboardingStatusId < (int)OnboardingStatusEnum.PhoneCKYC)
                merchant.OnboardingStatusId = (int)OnboardingStatusEnum.PhoneCKYC;

            var existingStepTracking = await _context.OnboardingStepTrackings
                .FirstOrDefaultAsync(s => s.Mid == mid && s.StepName == "Phone CKYC");

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
                    StepName = "Phone CKYC",
                    StepKey = "PHONE_CKYC",
                    StepStatus = "COMPLETED",
                    IsCompleted = true,
                    CompletedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            var (currentStepName, formStep, step) = await GetOnboardingStepInfoAsync(mid);
            var onboardingStatus = await BuildOnboardingStatusAsync(mid);

            var token = _jwtService.GenerateToken(
                merchant.User.Email,
                merchant.User.Email,
                string.Empty,
                merchant.User.UserId
            );

            _logger.LogInformation("Phone CKYC {Operation} for userId: {UserId}, mid: {Mid}",
                isUpdate ? "updated" : "saved", userId, mid);

            return new PhoneCkycSavedResponse
            {
                Mid = mid,
                CkycIdentifier = merchant.Ckycidentifier,
                ConsentGiven = merchant.CkycconsentGiven ?? false,
                ConsentDate = merchant.CkycconsentDate,
                Token = token,
                TokenExpiration = DateTime.UtcNow.AddMinutes(_appSettings.Jwt.ExpirationMinutes),
                Message = isUpdate ? "Phone CKYC updated successfully" : "Phone CKYC saved successfully",
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

            foreach (var stepName in stepOrder)
            {
                if (!completedSteps.Contains(stepName))
                {
                    currentStepName = stepName;
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
                _ => "PhoneCKYCCompletedBusinessCategoryPending"
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
