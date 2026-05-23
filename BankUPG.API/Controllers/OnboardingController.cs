using BankUPG.Application.Interfaces.StatusTracker;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    /// <summary>
    /// Onboarding Controller - Handles onboarding status tracking for the merchant
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OnboardingController : ControllerBase
    {
        private readonly IStatusTrackerService _statusTrackerService;
        private readonly ILogger<OnboardingController> _logger;

        public OnboardingController(
            IStatusTrackerService statusTrackerService,
            ILogger<OnboardingController> logger)
        {
            _statusTrackerService = statusTrackerService;
            _logger = logger;
        }

        /// <summary>
        /// Get onboarding status tracker for the authenticated merchant.
        /// Returns all 6 verification steps with individual statuses and overall progress.
        /// </summary>
        /// <returns>Status tracker with step details, overall progress, and application ID</returns>
        [HttpGet("status")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<StatusTrackerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<StatusTrackerResponse>>> GetOnboardingStatus()
        {
            try
            {
                var userIdClaim = User.FindAll(ClaimTypes.NameIdentifier)
                    .FirstOrDefault(c => int.TryParse(c.Value, out _));

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new ApiResponse<StatusTrackerResponse>
                    {
                        Success = false,
                        Message = "Invalid user token"
                    });
                }

                var result = await _statusTrackerService.GetOnboardingStatusAsync(userId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<StatusTrackerResponse>
                    {
                        Success = false,
                        Message = "Application not found. Please contact support."
                    });
                }

                return Ok(new ApiResponse<StatusTrackerResponse>
                {
                    Success = true,
                    Message = "Onboarding status retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving onboarding status for userId: {UserId}",
                    User.FindAll(ClaimTypes.NameIdentifier).FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value);
                return StatusCode(500, new ApiResponse<StatusTrackerResponse>
                {
                    Success = false,
                    Message = "An error occurred while retrieving onboarding status"
                });
            }
        }
    }
}
