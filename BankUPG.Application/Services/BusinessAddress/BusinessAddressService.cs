using BankUPG.Application.Interfaces.BusinessAddress;
using BankUPG.Application.Services.Auth;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Models;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.BusinessAddress
{
    public class BusinessAddressService : IBusinessAddressService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<BusinessAddressService> _logger;

        public BusinessAddressService(
            AppDBContext context,
            ILogger<BusinessAddressService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BusinessAddressResponse?> GetBusinessAddressAsync(int userId)
        {
            var merchant = await _context.Merchants
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null)
                return null;

            var addressDetail = await _context.BusinessAddressDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Mid == merchant.Mid);

            if (addressDetail == null)
                return null;

            return new BusinessAddressResponse
            {
                BusinessAddressDetailId = addressDetail.BusinessAddressDetailId,
                Mid = addressDetail.Mid,
                AddressLine1 = addressDetail.AddressLine1,
                AddressLine2 = addressDetail.AddressLine2,
                City = addressDetail.City,
                State = addressDetail.State,
                PostalCode = addressDetail.PostalCode,
                Country = addressDetail.Country,
                HasDifferentOperatingAddress = addressDetail.HasDifferentOperatingAddress,
                OperatingAddressLine1 = addressDetail.OperatingAddressLine1,
                OperatingAddressLine2 = addressDetail.OperatingAddressLine2,
                OperatingCity = addressDetail.OperatingCity,
                OperatingState = addressDetail.OperatingState,
                OperatingPostalCode = addressDetail.OperatingPostalCode,
                OperatingCountry = addressDetail.OperatingCountry
            };
        }

        public async Task<BusinessAddressSavedResponse> SaveBusinessAddressAsync(int userId, SaveBusinessAddressRequest request)
        {
            var merchant = await _context.Merchants
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null || merchant.User == null)
                throw new InvalidOperationException("User or merchant not found. Please ensure you are logged in.");

            // Validate operating address fields if HasDifferentOperatingAddress is true
            if (request.HasDifferentOperatingAddress)
            {
                if (string.IsNullOrWhiteSpace(request.OperatingAddressLine1))
                    throw new ArgumentException("Operating address line 1 is required when HasDifferentOperatingAddress is true.");
                if (string.IsNullOrWhiteSpace(request.OperatingCity))
                    throw new ArgumentException("Operating city is required when HasDifferentOperatingAddress is true.");
                if (string.IsNullOrWhiteSpace(request.OperatingState))
                    throw new ArgumentException("Operating state is required when HasDifferentOperatingAddress is true.");
                if (string.IsNullOrWhiteSpace(request.OperatingPostalCode))
                    throw new ArgumentException("Operating postal code is required when HasDifferentOperatingAddress is true.");
            }

            var mid = merchant.Mid;

            var existingAddress = await _context.BusinessAddressDetails
                .FirstOrDefaultAsync(b => b.Mid == mid);

            bool isUpdate = existingAddress != null;

            if (isUpdate)
            {
                existingAddress!.AddressLine1 = request.AddressLine1;
                existingAddress.AddressLine2 = request.AddressLine2;
                existingAddress.City = request.City;
                existingAddress.State = request.State;
                existingAddress.PostalCode = request.PostalCode;
                existingAddress.Country = request.Country;
                existingAddress.HasDifferentOperatingAddress = request.HasDifferentOperatingAddress;
                existingAddress.OperatingAddressLine1 = request.OperatingAddressLine1;
                existingAddress.OperatingAddressLine2 = request.OperatingAddressLine2;
                existingAddress.OperatingCity = request.OperatingCity;
                existingAddress.OperatingState = request.OperatingState;
                existingAddress.OperatingPostalCode = request.OperatingPostalCode;
                existingAddress.OperatingCountry = request.OperatingCountry;
                existingAddress.UpdatedDate = DateTime.UtcNow;
            }
            else
            {
                _context.BusinessAddressDetails.Add(new BusinessAddressDetail
                {
                    Mid = mid,
                    AddressLine1 = request.AddressLine1,
                    AddressLine2 = request.AddressLine2,
                    City = request.City,
                    State = request.State,
                    PostalCode = request.PostalCode,
                    Country = request.Country,
                    HasDifferentOperatingAddress = request.HasDifferentOperatingAddress,
                    OperatingAddressLine1 = request.OperatingAddressLine1,
                    OperatingAddressLine2 = request.OperatingAddressLine2,
                    OperatingCity = request.OperatingCity,
                    OperatingState = request.OperatingState,
                    OperatingPostalCode = request.OperatingPostalCode,
                    OperatingCountry = request.OperatingCountry,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                });
            }

            var existingStepTracking = await _context.OnboardingStepTrackings
                .FirstOrDefaultAsync(s => s.Mid == mid && s.StepName == "Verify Business Address");

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
                    StepName = "Verify Business Address",
                    StepKey = "VERIFY_BUSINESS_ADDRESS",
                    StepStatus = "COMPLETED",
                    IsCompleted = true,
                    CompletedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Business address {Operation} for userId: {UserId}, mid: {Mid}",
                isUpdate ? "updated" : "saved", userId, mid);

            var savedAddress = await _context.BusinessAddressDetails
                .FirstOrDefaultAsync(b => b.Mid == mid);

            return new BusinessAddressSavedResponse
            {
                BusinessAddressDetailId = savedAddress!.BusinessAddressDetailId,
                Mid = mid,
                AddressLine1 = savedAddress.AddressLine1,
                AddressLine2 = savedAddress.AddressLine2,
                City = savedAddress.City,
                State = savedAddress.State,
                PostalCode = savedAddress.PostalCode,
                Country = savedAddress.Country,
                HasDifferentOperatingAddress = savedAddress.HasDifferentOperatingAddress,
                OperatingAddressLine1 = savedAddress.OperatingAddressLine1,
                OperatingAddressLine2 = savedAddress.OperatingAddressLine2,
                OperatingCity = savedAddress.OperatingCity,
                OperatingState = savedAddress.OperatingState,
                OperatingPostalCode = savedAddress.OperatingPostalCode,
                OperatingCountry = savedAddress.OperatingCountry,
                Message = isUpdate ? "Business address updated successfully" : "Business address saved successfully",
                OnboardingStatus = await BuildOnboardingStatusAsync(mid)
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
    }
}
