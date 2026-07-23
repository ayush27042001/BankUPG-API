using BankUPG.Application.Interfaces.MerchantApiKey;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/merchant-api-keys")]
    [Authorize]
    [Produces("application/json")]
    public class MerchantApiKeyController : ControllerBase
    {
        private readonly IMerchantApiKeyService _service;
        private readonly ILogger<MerchantApiKeyController> _logger;

        public MerchantApiKeyController(IMerchantApiKeyService service, ILogger<MerchantApiKeyController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private bool IsSuperAdmin() => User.HasClaim(ClaimTypes.Role, "SuperAdmin");

        private int? GetMidFromToken() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<MerchantApiKeyResponse>>> Create([FromBody] CreateMerchantApiKeyRequest request)
        {
            if (!IsSuperAdmin()) return Forbid();
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<MerchantApiKeyResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            try
            {
                var result = await _service.CreateAsync(request);
                return Ok(new ApiResponse<MerchantApiKeyResponse> { Success = true, Message = "Merchant API keys saved", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving merchant API keys");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPut("{merchantApiKeyId:int}")]
        public async Task<ActionResult<ApiResponse<MerchantApiKeyResponse>>> Update(int merchantApiKeyId, [FromBody] UpdateMerchantApiKeyRequest request)
        {
            if (!IsSuperAdmin()) return Forbid();
            if (merchantApiKeyId != request.MerchantApiKeyId)
                return BadRequest(new ApiResponse { Success = false, Message = "ID mismatch" });

            var result = await _service.UpdateAsync(merchantApiKeyId, request);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "API keys not found" });
            return Ok(new ApiResponse<MerchantApiKeyResponse> { Success = true, Message = "Merchant API keys updated", Data = result });
        }

        [HttpGet("{merchantApiKeyId:int}")]
        public async Task<ActionResult<ApiResponse<MerchantApiKeyResponse>>> Get(int merchantApiKeyId)
        {
            if (!IsSuperAdmin()) return Forbid();
            var result = await _service.GetAsync(merchantApiKeyId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "API keys not found" });
            return Ok(new ApiResponse<MerchantApiKeyResponse> { Success = true, Message = "API keys retrieved", Data = result });
        }

        [HttpGet("by-mid/{mid:int}")]
        public async Task<ActionResult<ApiResponse<MerchantApiKeyResponse>>> GetByMid(int mid)
        {
            if (!IsSuperAdmin() && GetMidFromToken() != mid) return Forbid();
            var result = await _service.GetByMidAsync(mid);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "API keys not found" });
            return Ok(new ApiResponse<MerchantApiKeyResponse> { Success = true, Message = "API keys retrieved", Data = result });
        }

        [HttpDelete("{merchantApiKeyId:int}")]
        public async Task<ActionResult<ApiResponse>> Delete(int merchantApiKeyId)
        {
            if (!IsSuperAdmin()) return Forbid();
            var success = await _service.DeleteAsync(merchantApiKeyId);
            if (!success) return NotFound(new ApiResponse { Success = false, Message = "API keys not found" });
            return Ok(new ApiResponse { Success = true, Message = "Merchant API keys deleted" });
        }
    }
}
