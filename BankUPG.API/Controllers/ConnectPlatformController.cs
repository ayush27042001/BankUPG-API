using BankUPG.Application.Interfaces.ConnectPlatform;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    /// <summary>
    /// Connect Platform Controller - Handles mobile app / website connection details
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ConnectPlatformController : ControllerBase
    {
        private readonly IConnectPlatformService _connectPlatformService;
        private readonly ILogger<ConnectPlatformController> _logger;

        public ConnectPlatformController(
            IConnectPlatformService connectPlatformService,
            ILogger<ConnectPlatformController> logger)
        {
            _connectPlatformService = connectPlatformService;
            _logger = logger;
        }

        /// <summary>
        /// Get connect platform details for the authenticated user
        /// </summary>
        /// <returns>Current website/app connection details</returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ConnectPlatformResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ConnectPlatformResponse>>> GetConnectPlatform()
        {
            try
            {
                var userIdClaim = User.FindAll(ClaimTypes.NameIdentifier)
                    .FirstOrDefault(c => int.TryParse(c.Value, out _));
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new ApiResponse<ConnectPlatformResponse>
                    {
                        Success = false,
                        Message = "Invalid user token"
                    });
                }

                var result = await _connectPlatformService.GetConnectPlatformAsync(userId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<ConnectPlatformResponse>
                    {
                        Success = false,
                        Message = "Connect platform details not found. Please complete this step."
                    });
                }

                return Ok(new ApiResponse<ConnectPlatformResponse>
                {
                    Success = true,
                    Message = "Connect platform details retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving connect platform details for userId: {UserId}",
                    User.FindAll(ClaimTypes.NameIdentifier).FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value);
                return StatusCode(500, new ApiResponse<ConnectPlatformResponse>
                {
                    Success = false,
                    Message = "An error occurred while retrieving connect platform details"
                });
            }
        }

        /// <summary>
        /// Save or update connect platform details for the authenticated user
        /// </summary>
        /// <param name="request">Website/app connection details</param>
        /// <returns>Saved response with updated onboarding status and new JWT token</returns>
        [HttpPost("save")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ConnectPlatformSavedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<ConnectPlatformSavedResponse>>> SaveConnectPlatform(
            [FromBody] SaveConnectPlatformRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<ConnectPlatformSavedResponse>
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
                    return Unauthorized(new ApiResponse<ConnectPlatformSavedResponse>
                    {
                        Success = false,
                        Message = "Invalid user token"
                    });
                }

                var result = await _connectPlatformService.SaveConnectPlatformAsync(userId, request);

                return Ok(new ApiResponse<ConnectPlatformSavedResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning("User or merchant not found: {Message}", ex.Message);
                return Unauthorized(new ApiResponse<ConnectPlatformSavedResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<ConnectPlatformSavedResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving connect platform details for userId: {UserId}",
                    User.FindAll(ClaimTypes.NameIdentifier).FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value);
                return StatusCode(500, new ApiResponse<ConnectPlatformSavedResponse>
                {
                    Success = false,
                    Message = "An error occurred while saving connect platform details"
                });
            }
        }
    }
}
