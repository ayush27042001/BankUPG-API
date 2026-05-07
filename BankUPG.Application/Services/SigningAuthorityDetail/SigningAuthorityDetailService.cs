using BankUPG.Application.Interfaces.SigningAuthorityDetail;
using BankUPG.Application.Services.Auth;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Models;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.SigningAuthorityDetail
{
    public class SigningAuthorityDetailService : ISigningAuthorityDetailService
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly AppSettings _appSettings;
        private readonly ILogger<SigningAuthorityDetailService> _logger;

        public SigningAuthorityDetailService(
            AppDBContext context,
            JwtService jwtService,
            AppSettings appSettings,
            ILogger<SigningAuthorityDetailService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task<SigningAuthorityDetailResponse?> GetSigningAuthorityDetailAsync(int userId)
        {
            var merchant = await _context.Merchants
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null)
                return null;

            var detail = await _context.SigningAuthorityDetails
                .AsNoTracking()
                .Include(s => s.Pepstatus)
                .FirstOrDefaultAsync(s => s.Mid == merchant.Mid);

            if (detail == null)
                return null;

            return new SigningAuthorityDetailResponse
            {
                SigningAuthorityDetailId = detail.SigningAuthorityDetailId,
                Mid = detail.Mid,
                SigningAuthorityName = detail.SigningAuthorityName,
                SigningAuthorityEmail = detail.SigningAuthorityEmail,
                SigningAuthorityPan = detail.SigningAuthorityPan,
                PepstatusId = detail.PepstatusId,
                PepstatusName = detail.Pepstatus?.StatusName
            };
        }

        public async Task<SigningAuthorityDetailSavedResponse> SaveSigningAuthorityDetailAsync(int userId, SaveSigningAuthorityDetailRequest request)
        {
            var merchant = await _context.Merchants
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null || merchant.User == null)
                throw new InvalidOperationException("User or merchant not found. Please ensure you are logged in.");

            // Validate PEP status exists
            var pepStatus = await _context.Pepstatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PepstatusId == request.PepstatusId);

            if (pepStatus == null)
                throw new ArgumentException("Invalid PEP status");

            var mid = merchant.Mid;

            var existingDetail = await _context.SigningAuthorityDetails
                .FirstOrDefaultAsync(s => s.Mid == mid);

            bool isUpdate = existingDetail != null;

            if (isUpdate)
            {
                existingDetail!.SigningAuthorityName = request.SigningAuthorityName;
                existingDetail.SigningAuthorityEmail = request.SigningAuthorityEmail;
                existingDetail.SigningAuthorityPan = request.SigningAuthorityPan.ToUpper();
                existingDetail.PepstatusId = request.PepstatusId;
                existingDetail.UpdatedDate = DateTime.UtcNow;
            }
            else
            {
                _context.SigningAuthorityDetails.Add(new SigningAuthorityDetail
                {
                    Mid = mid,
                    SigningAuthorityName = request.SigningAuthorityName,
                    SigningAuthorityEmail = request.SigningAuthorityEmail,
                    SigningAuthorityPan = request.SigningAuthorityPan.ToUpper(),
                    PepstatusId = request.PepstatusId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(
                merchant.User.Email,
                merchant.User.Email,
                string.Empty,
                merchant.User.UserId
            );

            _logger.LogInformation("Signing authority details {Operation} for userId: {UserId}, mid: {Mid}",
                isUpdate ? "updated" : "saved", userId, mid);

            return new SigningAuthorityDetailSavedResponse
            {
                SigningAuthorityDetailId = isUpdate ? existingDetail!.SigningAuthorityDetailId : (await _context.SigningAuthorityDetails.FirstOrDefaultAsync(s => s.Mid == mid))!.SigningAuthorityDetailId,
                Mid = mid,
                SigningAuthorityName = request.SigningAuthorityName,
                SigningAuthorityEmail = request.SigningAuthorityEmail,
                SigningAuthorityPan = request.SigningAuthorityPan.ToUpper(),
                PepstatusId = request.PepstatusId,
                PepstatusName = pepStatus.StatusName,
                Token = token,
                TokenExpiration = DateTime.UtcNow.AddMinutes(_appSettings.Jwt.ExpirationMinutes),
                Message = isUpdate ? "Signing authority details updated successfully" : "Signing authority details saved successfully",
                OnboardingStatus = await BuildOnboardingStatusAsync(mid)
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
