using BankUPG.Application.Interfaces.Verification;
using BankUPG.SharedKernal.Models;
using BankUPG.SharedKernal.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace BankUPG.Application.Services.Verification
{
    public class PanVerificationService : IPanVerificationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _appSettings;
        private readonly ILogger<PanVerificationService> _logger;

        public PanVerificationService(
            IHttpClientFactory httpClientFactory,
            AppSettings appSettings,
            ILogger<PanVerificationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task<PanVerificationResult> VerifyPanAsync(string panNumber, string? nameOnPan = null)
        {
            try
            {
                var clientId = _appSettings.Cashfree.ClientId;
                var clientSecret = _appSettings.Cashfree.ClientSecret;
                var baseUrl = _appSettings.Cashfree.BaseUrl ?? "https://api.cashfree.com/";

                var client = _httpClientFactory.CreateClient();
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}verification/pan");
                request.Headers.Add("x-client-id", clientId);
                request.Headers.Add("x-client-secret", clientSecret);
                request.Headers.Add("Accept", "application/json");

                var requestBody = new { pan = panNumber };
                request.Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json");

                _logger.LogInformation("Verifying PAN: {Pan}", panNumber);

                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogDebug("Cashfree PAN verification response: {Response}", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("PAN verification failed: {StatusCode}, Response: {Response}", 
                        response.StatusCode, content);
                    return new PanVerificationResult
                    {
                        IsValid = false,
                        Message = $"Verification failed: {response.StatusCode}"
                    };
                }

                var result = JsonSerializer.Deserialize<CashfreePanResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    return new PanVerificationResult
                    {
                        IsValid = false,
                        Message = "Invalid response from verification service"
                    };
                }

                // Check if name matches (if provided)
                bool nameMatches = true;
                int? nameMatchScore = result.name_match_score;
                string? nameMatchResult = result.name_match_result;

                if (!string.IsNullOrEmpty(nameOnPan) && !string.IsNullOrEmpty(result.registered_name))
                {
                    nameMatches = result.registered_name.Equals(nameOnPan, StringComparison.OrdinalIgnoreCase) ||
                                  (result.name_match_score.HasValue && result.name_match_score.Value >= 80);
                }

                return new PanVerificationResult
                {
                    IsValid = result.valid == true && result.pan_status == "VALID",
                    PanNumber = result.pan,
                    Name = result.registered_name,
                    NameOnPanCard = result.name_pan_card,
                    Type = result.type,
                    ReferenceId = result.reference_id,
                    Message = result.message,
                    PanStatus = result.pan_status,
                    AadhaarSeedingStatus = result.aadhaar_seeding_status,
                    AadhaarSeedingStatusDesc = result.aadhaar_seeding_status_desc,
                    NameMatchScore = nameMatchScore,
                    NameMatchResult = nameMatchResult,
                    NameMatches = nameMatches,
                    LastUpdatedAt = result.last_updated_at
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error during PAN verification for: {Pan}", panNumber);
                return new PanVerificationResult
                {
                    IsValid = false,
                    Message = "Network error during verification. Please try again."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying PAN: {Pan}", panNumber);
                return new PanVerificationResult
                {
                    IsValid = false,
                    Message = "An error occurred during verification"
                };
            }
        }
    }
}
