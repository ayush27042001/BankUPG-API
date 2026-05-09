using BankUPG.Application.Interfaces.BankAccountDetail;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Models;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace BankUPG.Application.Services.BankAccountDetail
{
    public class BankAccountDetailService : IBankAccountDetailService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<BankAccountDetailService> _logger;
        private readonly HttpClient _httpClient;
        private readonly AppSettings _appSettings;

        public BankAccountDetailService(
            AppDBContext context,
            ILogger<BankAccountDetailService> logger,
            HttpClient httpClient,
            AppSettings appSettings)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClient;
            _appSettings = appSettings;
        }

        public async Task<BankAccountDetailResponse?> GetBankAccountDetailAsync(int userId)
        {
            var merchant = await _context.Merchants
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null)
                return null;

            var bankAccountDetail = await _context.BankAccountDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Mid == merchant.Mid);

            if (bankAccountDetail == null)
                return null;

            var isServiceAgreementSubmitted = await _context.ServiceAgreements.AnyAsync(sa => sa.Mid == merchant.Mid);

            return new BankAccountDetailResponse
            {
                BankAccountDetailId = bankAccountDetail.BankAccountDetailId,
                Mid = bankAccountDetail.Mid,
                BankHolderName = bankAccountDetail.BankHolderName,
                BankAccountNumber = bankAccountDetail.BankAccountNumber,
                Ifsccode = bankAccountDetail.Ifsccode,
                BankName = bankAccountDetail.BankName,
                AccountType = bankAccountDetail.AccountType,
                IsVerified = bankAccountDetail.IsVerified,
                VerifiedDate = bankAccountDetail.VerifiedDate,
                IsOnboardingCompleted = merchant.IsOnboardingCompleted ?? false,
                IsOnboardingRejected = merchant.IsOnboardingRejected ?? false,
                IsServiceAgreementSubmitted = isServiceAgreementSubmitted
            };
        }

        public async Task<BankAccountDetailSavedResponse> SaveBankAccountDetailAsync(int userId, SaveBankAccountDetailRequest request)
        {
            var merchant = await _context.Merchants
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null || merchant.User == null)
                throw new InvalidOperationException("User or merchant not found. Please ensure you are logged in.");

            var mid = merchant.Mid;

            var existingBankAccount = await _context.BankAccountDetails
                .FirstOrDefaultAsync(b => b.Mid == mid);

            bool isUpdate = existingBankAccount != null;

            if (isUpdate)
            {
                existingBankAccount!.BankHolderName = request.BankHolderName;
                existingBankAccount.BankAccountNumber = request.BankAccountNumber;
                existingBankAccount.Ifsccode = request.Ifsccode;
                existingBankAccount.AccountType = request.AccountType;
                existingBankAccount.UpdatedDate = DateTime.UtcNow;
            }
            else
            {
                _context.BankAccountDetails.Add(new BankUPG.Infrastructure.Entities.BankAccountDetail
                {
                    Mid = mid,
                    BankHolderName = request.BankHolderName,
                    BankAccountNumber = request.BankAccountNumber,
                    Ifsccode = request.Ifsccode,
                    AccountType = request.AccountType,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                });
            }

            var existingStepTracking = await _context.OnboardingStepTrackings
                .FirstOrDefaultAsync(s => s.Mid == mid && s.StepName == "Share Bank Account Details");

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
                    StepName = "Share Bank Account Details",
                    StepKey = "SHARE_BANK_ACCOUNT_DETAILS",
                    StepStatus = "COMPLETED",
                    IsCompleted = true,
                    CompletedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Bank account detail {Operation} for userId: {UserId}, mid: {Mid}",
                isUpdate ? "updated" : "saved", userId, mid);

            var savedBankAccount = await _context.BankAccountDetails
                .FirstOrDefaultAsync(b => b.Mid == mid);

            return new BankAccountDetailSavedResponse
            {
                BankAccountDetailId = savedBankAccount!.BankAccountDetailId,
                Mid = mid,
                BankHolderName = savedBankAccount.BankHolderName,
                BankAccountNumber = savedBankAccount.BankAccountNumber,
                Ifsccode = savedBankAccount.Ifsccode,
                BankName = savedBankAccount.BankName,
                AccountType = savedBankAccount.AccountType,
                Message = isUpdate ? "Bank account details updated successfully" : "Bank account details saved successfully",
                OnboardingStatus = await BuildOnboardingStatusAsync(mid)
            };
        }

        public async Task<BankAccountVerifyResult> VerifyBankAccountAsync(VerifyBankAccountRequest request)
        {
            var phone = request.PhoneNumber ?? string.Empty;
            var bankHolderName = request.AccountHolderName ?? string.Empty;

            var result = await VerifyBankAccountWithCashfreeAsync(
                request.AccountNumber,
                request.IFSCCode,
                bankHolderName,
                phone,
                null,
                null
            );

            return result;
        }

        private async Task<BankAccountVerifyResult> VerifyBankAccountWithCashfreeAsync(
            string bankAccount,
            string ifsc,
            string name,
            string phone,
            string? businessName,
            string? panName)
        {
            try
            {
                var url = $"{_appSettings.Cashfree.BaseUrl}verification/bank-account/sync";
                var requestBody = new
                {
                    bank_account = bankAccount,
                    ifsc = ifsc,
                    name = name,
                    phone = phone
                };

                var jsonBody = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-client-id", _appSettings.Cashfree.ClientId);
                _httpClient.DefaultRequestHeaders.Add("x-client-secret", _appSettings.Cashfree.ClientSecret);
                _httpClient.DefaultRequestHeaders.Add("cache-control", "no-cache");
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Cashfree bank verification API call - Request: {Request}, Response: {Response}",
                    jsonBody, responseContent);

                var json = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var status = json.TryGetProperty("account_status", out var statusProp) ? statusProp.GetString() : null;
                var nameAtBank = json.TryGetProperty("name_at_bank", out var nameAtBankProp) ? nameAtBankProp.GetString() : null;
                var bankName = json.TryGetProperty("bank_name", out var bankNameProp) ? bankNameProp.GetString() : null;

                if (status == "VALID" && nameAtBank != null)
                {
                    var nameAtBankUpper = nameAtBank.ToUpper();
                    var businessNameUpper = businessName?.ToUpper();
                    var panNameUpper = panName?.ToUpper();
                    var nameUpper = name.ToUpper();

                    if (nameAtBankUpper == businessNameUpper || 
                        nameAtBankUpper == panNameUpper || 
                        nameAtBankUpper == nameUpper)
                    {
                        return new BankAccountVerifyResult
                        {
                            IsValid = true,
                            IsNameMatched = true,
                            AccountStatus = status,
                            NameAtBank = nameAtBank,
                            BankName = bankName,
                            Message = "Bank account verified successfully"
                        };
                    }
                    else
                    {
                        return new BankAccountVerifyResult
                        {
                            IsValid = false,
                            IsNameMatched = false,
                            AccountStatus = status,
                            NameAtBank = nameAtBank,
                            BankName = bankName,
                            Message = "Bank account name does not match with business or PAN name"
                        };
                    }
                }
                else
                {
                    return new BankAccountVerifyResult
                    {
                        IsValid = false,
                        IsNameMatched = false,
                        AccountStatus = status,
                        NameAtBank = nameAtBank,
                        BankName = bankName,
                        Message = "Bank account verification failed"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying bank account with Cashfree API");
                return new BankAccountVerifyResult
                {
                    IsValid = false,
                    IsNameMatched = false,
                    Message = "Error occurred during bank account verification"
                };
            }
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
