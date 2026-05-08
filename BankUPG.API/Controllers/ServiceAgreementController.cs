using BankUPG.Application.Interfaces.ServiceAgreement;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceAgreementController : ControllerBase
    {
        private readonly IServiceAgreementService _serviceAgreementService;
        private readonly ILogger<ServiceAgreementController> _logger;

        public ServiceAgreementController(
            IServiceAgreementService serviceAgreementService,
            ILogger<ServiceAgreementController> logger)
        {
            _serviceAgreementService = serviceAgreementService;
            _logger = logger;
        }

        /// <summary>
        /// Get service agreement details for the authenticated user
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ServiceAgreementResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<ServiceAgreementResponse>>> GetServiceAgreement()
        {
            try
            {
                var userIdClaim = User.FindAll(ClaimTypes.NameIdentifier)
                    .FirstOrDefault(c => int.TryParse(c.Value, out _));
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return Unauthorized(new ApiResponse<ServiceAgreementResponse> { Success = false, Message = "Invalid user token" });

                var result = await _serviceAgreementService.GetServiceAgreementAsync(userId);

                if (result == null)
                    return Ok(new ApiResponse<ServiceAgreementResponse> { Success = true, Message = "No service agreement found", Data = null });

                return Ok(new ApiResponse<ServiceAgreementResponse> { Success = true, Message = "Service agreement retrieved successfully", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service agreement for user");
                return StatusCode(500, new ApiResponse<ServiceAgreementResponse> { Success = false, Message = "An error occurred while retrieving service agreement" });
            }
        }

        /// <summary>
        /// Submit or update the service agreement with digital signature
        /// </summary>
        /// <param name="request">Signature data, agreement date, and acceptance flag</param>
        [HttpPost("save")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ServiceAgreementSavedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<ServiceAgreementSavedResponse>>> SaveServiceAgreement(
            [FromBody] SaveServiceAgreementRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<ServiceAgreementSavedResponse>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var userIdClaim = User.FindAll(ClaimTypes.NameIdentifier)
                    .FirstOrDefault(c => int.TryParse(c.Value, out _));
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return Unauthorized(new ApiResponse<ServiceAgreementSavedResponse> { Success = false, Message = "Invalid user token" });

                var result = await _serviceAgreementService.SaveServiceAgreementAsync(userId, request);

                return Ok(new ApiResponse<ServiceAgreementSavedResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning("User or merchant not found: {Message}", ex.Message);
                return Unauthorized(new ApiResponse<ServiceAgreementSavedResponse> { Success = false, Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<ServiceAgreementSavedResponse> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving service agreement for user");
                return StatusCode(500, new ApiResponse<ServiceAgreementSavedResponse> { Success = false, Message = "An error occurred while saving service agreement" });
            }
        }
    }
}
