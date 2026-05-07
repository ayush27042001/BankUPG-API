using BankUPG.Application.Interfaces.BusinessDetails;
using BankUPG.Application.Services.Auth;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Models;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BankUPG.Application.Services.BusinessDetails
{
    public class BusinessDetailsService : IBusinessDetailsService
    {
        private readonly AppDBContext _context;
        private readonly IHttpClientFactory _http;
        private readonly ILogger<BusinessDetailsService> _logger;

        public BusinessDetailsService(
            AppDBContext context,
            IHttpClientFactory http,
            ILogger<BusinessDetailsService> logger)
        {
            _context = context;
            _http = http;
            _logger = logger;
        }

        public async Task<BusinessDetailsResponse?> GetBusinessDetailsAsync(int userId)
        {
            var merchant = await _context.Merchants
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null)
                return null;

            if (merchant.ExpectedSalesPerMonth == null && merchant.HasGstin == null)
                return null;

            return new BusinessDetailsResponse
            {
                Mid = merchant.Mid,
                ExpectedSalesPerMonth = merchant.ExpectedSalesPerMonth,
                HasGstin = merchant.HasGstin,
                Gstin = merchant.Gstin
            };
        }

        public async Task<BusinessDetailsSavedResponse> SaveBusinessDetailsAsync(int userId, SaveBusinessDetailsRequest request)
        {
            var merchant = await _context.Merchants
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null || merchant.User == null)
                throw new InvalidOperationException("User or merchant not found. Please ensure you are logged in.");

            if (request.HasGstin && string.IsNullOrWhiteSpace(request.Gstin))
                throw new ArgumentException("GSTIN is required when HasGSTIN is true.");

            var mid = merchant.Mid;
            bool isUpdate = merchant.ExpectedSalesPerMonth.HasValue;

            merchant.ExpectedSalesPerMonth = request.ExpectedSalesPerMonth;
            merchant.HasGstin = request.HasGstin;
            merchant.Gstin = request.HasGstin ? request.Gstin : null;
            merchant.UpdatedDate = DateTime.UtcNow;

            if (merchant.OnboardingStatusId < (int)OnboardingStatusEnum.ShareBusinessDetails)
                merchant.OnboardingStatusId = (int)OnboardingStatusEnum.ShareBusinessDetails;

            var existingStepTracking = await _context.OnboardingStepTrackings
                .FirstOrDefaultAsync(s => s.Mid == mid && s.StepName == "Share Business Details");

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
                    StepName = "Share Business Details",
                    StepKey = "SHARE_BUSINESS_DETAILS",
                    StepStatus = "COMPLETED",
                    IsCompleted = true,
                    CompletedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            var (_, formStep, step) = await GetOnboardingStepInfoAsync(mid);
            var onboardingStatus = await BuildOnboardingStatusAsync(mid);

            _logger.LogInformation("Business details {Operation} for userId: {UserId}, mid: {Mid}",
                isUpdate ? "updated" : "saved", userId, mid);

            return new BusinessDetailsSavedResponse
            {
                Mid = mid,
                ExpectedSalesPerMonth = merchant.ExpectedSalesPerMonth,
                HasGstin = merchant.HasGstin,
                Gstin = merchant.Gstin,
                Message = isUpdate ? "Business details updated successfully" : "Business details saved successfully",
                FormStep = formStep,
                Step = step,
                OnboardingStatus = onboardingStatus
            };
        }

        public async Task<GstVerifyResult> VerifyGstAsync(string gstin, string? businessName)
        {
            var client = _http.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{_appSettings.Cashfree.BaseUrl}verification/gstin");

            request.Headers.Add("x-client-id", _appSettings.Cashfree.ClientId);
            request.Headers.Add("x-client-secret", _appSettings.Cashfree.ClientSecret);

            request.Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    GSTIN = gstin,
                    business_name = businessName ?? string.Empty
                }),
                Encoding.UTF8,
                "application/json");

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Cashfree GST verify response for GSTIN {Gstin}: {StatusCode}", gstin, response.StatusCode);

            var json = JsonNode.Parse(content);

            return new GstVerifyResult
            {
                IsValid = json?["valid"]?.ToString() == "True",
                LegalName = json?["legal_name_of_business"]?.ToString(),
                TradeName = json?["trade_name_of_business"]?.ToString()
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
                _ => "ShareDetailsCompletedConnectPlatformPending"
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
