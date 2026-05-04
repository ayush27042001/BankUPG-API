using BankUPG.Application.Services.Registration;
using BankUPG.Application.Services.Verification;
using BankUPG.Infrastructure.Data;
using BankUPG.Application.Interfaces.Registration;
using BankUPG.Application.Interfaces.Verification;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    /// <summary>
    /// Merchant Registration Controller - Handles complete onboarding flow
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        private readonly IPanVerificationService _panVerificationService;
        private readonly AppDBContext _context;
        private readonly ILogger<RegistrationController> _logger;

        public RegistrationController(
            IRegistrationService registrationService,
            IPanVerificationService panVerificationService,
            AppDBContext context,
            ILogger<RegistrationController> logger)
        {
            _registrationService = registrationService;
            _panVerificationService = panVerificationService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Step 1: Initiate merchant registration - Sends OTP to mobile
        /// </summary>
        /// <param name="request">Registration details including email, password, mobile, website, business name</param>
        /// <returns>Registration token and OTP expiry time</returns>
        [HttpPost("initiate")]
        [ProducesResponseType(typeof(ApiResponse<RegistrationInitiatedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ApiResponse<RegistrationInitiatedResponse>>> InitiateRegistration(
            [FromBody] InitiateRegistrationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<RegistrationInitiatedResponse>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var ipAddress = GetClientIpAddress();
                
                var (registrationToken, mobileNumber, expirySeconds) = await _registrationService
                    .InitiateRegistrationAsync(request, ipAddress);

                // Mask mobile number for response
                var maskedMobile = MaskMobileNumber(mobileNumber);

                return Ok(new ApiResponse<RegistrationInitiatedResponse>
                {
                    Success = true,
                    Message = $"OTP sent to {maskedMobile}. Valid for 5 minutes.",
                    Data = new RegistrationInitiatedResponse
                    {
                        RegistrationToken = registrationToken,
                        MobileNumber = maskedMobile,
                        OtpExpirySeconds = expirySeconds,
                        Message = "Please enter the 6-digit OTP sent to your mobile"
                    }
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already registered"))
            {
                _logger.LogWarning("Duplicate registration attempt: {Message}", ex.Message);
                return Conflict(new ApiResponse<RegistrationInitiatedResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating registration for email: {Email}", request.Email);
                return StatusCode(500, new ApiResponse<RegistrationInitiatedResponse>
                {
                    Success = false,
                    Message = "An error occurred while processing your registration. Please try again."
                });
            }
        }

        /// <summary>
        /// Step 2: Verify 6-digit OTP sent to mobile
        /// </summary>
        /// <param name="request">Mobile number, OTP code, and registration token</param>
        /// <returns>Verification status and next step instructions</returns>
        [HttpPost("verify-otp")]
        [ProducesResponseType(typeof(ApiResponse<OtpVerificationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status410Gone)]
        public async Task<ActionResult<ApiResponse<OtpVerificationResponse>>> VerifyOtp(
            [FromBody] VerifyRegistrationOtpRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<OtpVerificationResponse>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var result = await _registrationService.VerifyRegistrationOtpAsync(
                    request.MobileNumber, request.Otp, request.RegistrationToken);

                if (!result.IsVerified)
                {
                    if (result.RemainingAttempts == null)
                    {
                        return StatusCode(410, new ApiResponse<OtpVerificationResponse>
                        {
                            Success = false,
                            Message = result.Message,
                            Data = result
                        });
                    }

                    return BadRequest(new ApiResponse<OtpVerificationResponse>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = result
                    });
                }

                return Ok(new ApiResponse<OtpVerificationResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for mobile: {Mobile}", request.MobileNumber);
                return StatusCode(500, new ApiResponse<OtpVerificationResponse>
                {
                    Success = false,
                    Message = "An error occurred while verifying OTP"
                });
            }
        }

        /// <summary>
        /// Resend OTP for registration
        /// </summary>
        /// <param name="mobileNumber">Mobile number (unmasked)</param>
        /// <param name="registrationToken">Registration token received during initiation</param>
        /// <returns>Resend status and cooldown information</returns>
        [HttpPost("resend-otp")]
        [ProducesResponseType(typeof(ApiResponse<ResendOtpResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ApiResponse<ResendOtpResponse>>> ResendOtp(
            [FromQuery, Required, RegularExpression(@"^[6-9]\d{9}$")] string mobileNumber,
            [FromQuery, Required] string registrationToken)
        {
            try
            {
                var result = await _registrationService.ResendRegistrationOtpAsync(mobileNumber, registrationToken);

                if (!result.Success && result.RemainingSeconds > 0)
                {
                    return StatusCode(429, new ApiResponse<ResendOtpResponse>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = result
                    });
                }

                if (!result.Success)
                {
                    return BadRequest(new ApiResponse<ResendOtpResponse>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = result
                    });
                }

                return Ok(new ApiResponse<ResendOtpResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending OTP for mobile: {Mobile}", mobileNumber);
                return StatusCode(500, new ApiResponse<ResendOtpResponse>
                {
                    Success = false,
                    Message = "An error occurred while resending OTP"
                });
            }
        }

        /// <summary>
        /// Step 3: Complete registration with PAN card details (requires JWT authentication)
        /// </summary>
        /// <param name="request">PAN card details</param>
        /// <returns>Complete registration response with JWT token and onboarding status</returns>
        [HttpPost("complete-pan")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<RegistrationCompletedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<RegistrationCompletedResponse>>> CompletePanRegistration(
            [FromBody] CompletePanRegistrationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<RegistrationCompletedResponse>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                // Extract userId from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new ApiResponse<RegistrationCompletedResponse>
                    {
                        Success = false,
                        Message = "Invalid user token"
                    });
                }

                var result = await _registrationService.CompletePanRegistrationAsync(userId, request);

                return Ok(new ApiResponse<RegistrationCompletedResponse>
                {
                    Success = true,
                    Message = "PAN details saved successfully!",
                    Data = result
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning("User or merchant not found: {Message}", ex.Message);
                return Unauthorized(new ApiResponse<RegistrationCompletedResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already registered"))
            {
                _logger.LogWarning("Duplicate PAN registration: {Message}", ex.Message);
                return Conflict(new ApiResponse<RegistrationCompletedResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<RegistrationCompletedResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing PAN registration for userId: {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new ApiResponse<RegistrationCompletedResponse>
                {
                    Success = false,
                    Message = "An error occurred while completing registration"
                });
            }
        }

        /// <summary>
        /// Get PAN details for authenticated user
        /// </summary>
        /// <returns>PAN card details</returns>
        [HttpGet("get-pan-details")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PanDetailsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PanDetailsResponse>>> GetPanDetails()
        {
            try
            {
                // Extract userId from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new ApiResponse<PanDetailsResponse>
                    {
                        Success = false,
                        Message = "Invalid user token"
                    });
                }

                var panDetails = await _registrationService.GetPanDetailsAsync(userId);

                if (panDetails == null)
                {
                    return NotFound(new ApiResponse<PanDetailsResponse>
                    {
                        Success = false,
                        Message = "PAN details not found. Please complete PAN registration first."
                    });
                }

                return Ok(new ApiResponse<PanDetailsResponse>
                {
                    Success = true,
                    Message = "PAN details retrieved successfully",
                    Data = panDetails
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PAN details for userId: {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new ApiResponse<PanDetailsResponse>
                {
                    Success = false,
                    Message = "An error occurred while retrieving PAN details"
                });
            }
        }

        /// <summary>
        /// Check if email is available for registration
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>Availability status</returns>
        [HttpGet("check-email")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<bool>>> CheckEmailAvailability(
            [FromQuery, Required, EmailAddress] string email)
        {
            try
            {
                var exists = await _context.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.Email == email);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = exists ? "Email is already registered" : "Email is available",
                    Data = !exists
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email availability: {Email}", email);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }

        /// <summary>
        /// Check if mobile number is available for registration
        /// </summary>
        /// <param name="mobileNumber">Mobile number to check (10 digits)</param>
        /// <returns>Availability status</returns>
        [HttpGet("check-mobile")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<bool>>> CheckMobileAvailability(
            [FromQuery, Required, RegularExpression(@"^[6-9]\d{9}$")] string mobileNumber)
        {
            try
            {
                var exists = await _context.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.MobileNumber == mobileNumber);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = exists ? "Mobile number is already registered" : "Mobile number is available",
                    Data = !exists
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking mobile availability: {Mobile}", mobileNumber);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }

        /// <summary>
        /// Validate PAN card format without saving
        /// </summary>
        /// <param name="panNumber">PAN card number</param>
        /// <returns>Validation result</returns>
        [HttpGet("validate-pan")]
        [ProducesResponseType(typeof(ApiResponse<PanValidationResult>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PanValidationResult>>> ValidatePan(
            [FromQuery, Required, RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$")] string panNumber)
        {
            try
            {
                panNumber = panNumber.ToUpper();

                // Check format
                var isValidFormat = IsValidPanFormat(panNumber);

                // Check if already registered
                var isAlreadyRegistered = await _context.BusinessPandetails
                    .AsNoTracking()
                    .AnyAsync(p => p.PancardNumber == panNumber);

                return Ok(new ApiResponse<PanValidationResult>
                {
                    Success = true,
                    Message = isValidFormat 
                        ? (isAlreadyRegistered ? "PAN card already registered" : "PAN card format is valid")
                        : "Invalid PAN card format",
                    Data = new PanValidationResult
                    {
                        IsValidFormat = isValidFormat,
                        IsAlreadyRegistered = isAlreadyRegistered,
                        PanNumber = panNumber
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating PAN: {PAN}", panNumber);
                return StatusCode(500, new ApiResponse<PanValidationResult>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }

        /// <summary>
        /// Verify PAN card with Cashfree API
        /// </summary>
        /// <param name="panNumber">PAN card number</param>
        /// <param name="nameOnPan">Name on PAN card (optional, for name matching)</param>
        /// <returns>Cashfree verification result</returns>
        [HttpGet("verify-pan-cashfree")]
        [ProducesResponseType(typeof(ApiResponse<PanVerificationResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PanVerificationResult>>> VerifyPanWithCashfree(
            [FromQuery, Required, RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]{1}$")] string panNumber,
            [FromQuery] string? nameOnPan = null)
        {
            try
            {
                panNumber = panNumber.ToUpper();

                // Validate format first
                if (!IsValidPanFormat(panNumber))
                {
                    return BadRequest(new ApiResponse<PanVerificationResult>
                    {
                        Success = false,
                        Message = "Invalid PAN card format"
                    });
                }

                // Call Cashfree verification
                var result = await _panVerificationService.VerifyPanAsync(panNumber, nameOnPan);

                if (!result.IsValid)
                {
                    return Ok(new ApiResponse<PanVerificationResult>
                    {
                        Success = true,
                        Message = result.Message ?? "PAN verification failed",
                        Data = result
                    });
                }

                return Ok(new ApiResponse<PanVerificationResult>
                {
                    Success = true,
                    Message = result.NameMatches 
                        ? "PAN verified successfully with name match" 
                        : "PAN verified but name does not match",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying PAN with Cashfree: {PAN}", panNumber);
                return StatusCode(500, new ApiResponse<PanVerificationResult>
                {
                    Success = false,
                    Message = "An error occurred during PAN verification"
                });
            }
        }

        #region Private Methods

        private string? GetClientIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                var forwardedFor = Request.Headers["X-Forwarded-For"].ToString();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    ipAddress = forwardedFor.Split(',')[0].Trim();
                }
            }

            return ipAddress;
        }

        private static string MaskMobileNumber(string mobileNumber)
        {
            if (string.IsNullOrEmpty(mobileNumber) || mobileNumber.Length < 10)
                return mobileNumber;

            return $"******{mobileNumber[^4..]}";
        }

        private static bool IsValidPanFormat(string pan)
        {
            if (string.IsNullOrEmpty(pan) || pan.Length != 10)
                return false;

            // PAN format: AAAAA9999A (5 letters, 4 digits, 1 letter)
            for (int i = 0; i < 5; i++)
                if (!char.IsLetter(pan[i])) return false;

            for (int i = 5; i < 9; i++)
                if (!char.IsDigit(pan[i])) return false;

            return char.IsLetter(pan[9]);
        }

        #endregion
    }

    public class PanValidationResult
    {
        public bool IsValidFormat { get; set; }
        public bool IsAlreadyRegistered { get; set; }
        public string PanNumber { get; set; } = string.Empty;
    }
}
