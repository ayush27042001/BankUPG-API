using BankUPG.Application.Interfaces.IpWhitelist;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/ip-whitelist")]
    [Authorize]
    [Produces("application/json")]
    public class IpWhitelistController : ControllerBase
    {
        private readonly IIpWhitelistService _service;
        private readonly ILogger<IpWhitelistController> _logger;

        public IpWhitelistController(IIpWhitelistService service, ILogger<IpWhitelistController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        /// <summary>Get IP whitelist status — enabled flag + all whitelisted IPs</summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IpWhitelistStatusResponse>>> GetStatus()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetStatusAsync(userId.Value);
            return Ok(new ApiResponse<IpWhitelistStatusResponse> { Success = true, Message = "IP whitelist retrieved", Data = result });
        }

        /// <summary>Add an IP address or CIDR range (e.g. 192.168.1.100 or 10.0.0.0/24)</summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<IpWhitelistResponse>>> AddIp([FromBody] AddIpWhitelistRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<IpWhitelistResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            try
            {
                var result = await _service.AddIpAsync(userId.Value, request);
                return Ok(new ApiResponse<IpWhitelistResponse> { Success = true, Message = "IP address whitelisted", Data = result });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        /// <summary>Remove an IP from the whitelist</summary>
        [HttpDelete("{ipWhitelistId:int}")]
        public async Task<ActionResult<ApiResponse>> RemoveIp(int ipWhitelistId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var success = await _service.RemoveIpAsync(userId.Value, ipWhitelistId);
            if (!success) return NotFound(new ApiResponse { Success = false, Message = "IP address not found" });
            return Ok(new ApiResponse { Success = true, Message = "IP address removed" });
        }

        /// <summary>Enable or disable IP whitelist enforcement for this merchant</summary>
        [HttpPut("toggle")]
        public async Task<ActionResult<ApiResponse>> Toggle([FromBody] ToggleIpWhitelistRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            try
            {
                await _service.ToggleWhitelistAsync(userId.Value, request.Enabled);
                return Ok(new ApiResponse { Success = true, Message = request.Enabled ? "IP whitelist enabled" : "IP whitelist disabled" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }
    }
}
