using BankUPG.Application.Interfaces.BankAccountDetail;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    /// <summary>
    /// Bank Account Detail Controller - Handles bank account details onboarding step
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BankAccountDetailController : ControllerBase
    {
        private readonly IBankAccountDetailService _bankAccountDetailService;
        private readonly ILogger<BankAccountDetailController> _logger;

        public BankAccountDetailController(
            IBankAccountDetailService bankAccountDetailService,
            ILogger<BankAccountDetailController> logger)
        {
            _bankAccountDetailService = bankAccountDetailService;
            _logger = logger;
        }

        /// <summary>
        /// Get bank account details for authenticated user
        /// </summary>
        /// <returns>Current bank account details</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<BankAccountDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BankAccountDetailResponse>>> GetBankAccountDetail()
        {
            try
            {
                var userIdClaim = User.FindAll(ClaimTypes.NameIdentifier)
                    .FirstOrDefault(c => int.TryParse(c.Value, out _));
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new ApiResponse<BankAccountDetailResponse>
                    {
                        Success = false,
                        Message = "Invalid user token"
                    });
                }

                var result = await _bankAccountDetailService.GetBankAccountDetailAsync(userId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<BankAccountDetailResponse>
                    {
                        Success = false,
                        Message = "Bank account details not found. Please complete this step."
                    });
                }

                return Ok(new ApiResponse<BankAccountDetailResponse>
                {
                    Success = true,
                    Message = "Bank account details retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bank account details for userId: {UserId}",
                    User.FindAll(ClaimTypes.NameIdentifier).FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value);
                return StatusCode(500, new ApiResponse<BankAccountDetailResponse>
                {
                    Success = false,
                    Message = "An error occurred while retrieving bank account details"
                });
            }
        }

        /// <summary>
        /// Save or update bank account details for authenticated user
        /// </summary>
        /// <param name="request">Bank account details</param>
        /// <returns>Saved response with updated onboarding status</returns>
        [HttpPost("save")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<BankAccountDetailSavedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<BankAccountDetailSavedResponse>>> SaveBankAccountDetail(
            [FromBody] SaveBankAccountDetailRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<BankAccountDetailSavedResponse>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var userIdClaim = User.FindAll(ClaimTypes.NameIdentifier)
                    .FirstOrDefault(c => int.TryParse(c.Value, out _));
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new ApiResponse<BankAccountDetailSavedResponse>
                    {
                        Success = false,
                        Message = "Invalid user token"
                    });
                }

                var result = await _bankAccountDetailService.SaveBankAccountDetailAsync(userId, request);

                return Ok(new ApiResponse<BankAccountDetailSavedResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning("User or merchant not found: {Message}", ex.Message);
                return Unauthorized(new ApiResponse<BankAccountDetailSavedResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<BankAccountDetailSavedResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving bank account details for userId: {UserId}",
                    User.FindAll(ClaimTypes.NameIdentifier).FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value);
                return StatusCode(500, new ApiResponse<BankAccountDetailSavedResponse>
                {
                    Success = false,
                    Message = "An error occurred while saving bank account details"
                });
            }
        }

        /// <summary>
        /// Verify bank account details using Cashfree API
        /// </summary>
        /// <param name="bankAccountNumber">Bank account number</param>
        /// <param name="ifscCode">IFSC code</param>
        /// <param name="bankHolderName">Bank holder name (optional)</param>
        /// <param name="phone">Phone number (optional)</param>
        /// <returns>Bank account verification result</returns>
        [HttpGet("verify")]
        [ProducesResponseType(typeof(ApiResponse<BankAccountVerifyResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<BankAccountVerifyResult>>> VerifyBankAccount(
            [FromQuery, Required] string bankAccountNumber,
            [FromQuery, Required] string ifscCode,
            [FromQuery] string? bankHolderName = null,
            [FromQuery] string? phone = null)
        {
            try
            {
                var request = new VerifyBankAccountRequest
                {
                    AccountNumber = bankAccountNumber,
                    IFSCCode = ifscCode,
                    AccountHolderName = bankHolderName,
                    PhoneNumber = phone
                };

                var result = await _bankAccountDetailService.VerifyBankAccountAsync(request);

                if (!result.IsValid)
                {
                    return Ok(new ApiResponse<BankAccountVerifyResult>
                    {
                        Success = true,
                        Message = result.Message ?? "Bank account verification failed",
                        Data = result
                    });
                }

                return Ok(new ApiResponse<BankAccountVerifyResult>
                {
                    Success = true,
                    Message = result.IsNameMatched 
                        ? "Bank account verified successfully with name match" 
                        : "Bank account verified but name does not match",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying bank account");
                return StatusCode(500, new ApiResponse<BankAccountVerifyResult>
                {
                    Success = false,
                    Message = "An error occurred during bank account verification"
                });
            }
        }
    }
}
