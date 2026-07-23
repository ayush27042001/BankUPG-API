using BankUPG.Application.Interfaces.MerchantColumnPreference;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/merchant-column-preferences")]
    [Authorize(Roles = "SuperAdmin")]
    [Produces("application/json")]
    public class MerchantColumnPreferenceController : ControllerBase
    {
        private readonly IMerchantColumnPreferenceService _service;
        private readonly ILogger<MerchantColumnPreferenceController> _logger;

        public MerchantColumnPreferenceController(IMerchantColumnPreferenceService service, ILogger<MerchantColumnPreferenceController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<MerchantColumnPreferenceResponse>>> Create([FromBody] CreateMerchantColumnPreferenceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<MerchantColumnPreferenceResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            try
            {
                var result = await _service.CreateAsync(request);
                return Ok(new ApiResponse<MerchantColumnPreferenceResponse> { Success = true, Message = "Column preference saved", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving column preference");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPut("{merchantColumnPreferenceId:int}")]
        public async Task<ActionResult<ApiResponse<MerchantColumnPreferenceResponse>>> Update(int merchantColumnPreferenceId, [FromBody] UpdateMerchantColumnPreferenceRequest request)
        {
            if (merchantColumnPreferenceId != request.MerchantColumnPreferenceId)
                return BadRequest(new ApiResponse { Success = false, Message = "ID mismatch" });

            var result = await _service.UpdateAsync(merchantColumnPreferenceId, request);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Column preference not found" });
            return Ok(new ApiResponse<MerchantColumnPreferenceResponse> { Success = true, Message = "Column preference updated", Data = result });
        }

        [HttpGet("{merchantColumnPreferenceId:int}")]
        public async Task<ActionResult<ApiResponse<MerchantColumnPreferenceResponse>>> Get(int merchantColumnPreferenceId)
        {
            var result = await _service.GetAsync(merchantColumnPreferenceId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Column preference not found" });
            return Ok(new ApiResponse<MerchantColumnPreferenceResponse> { Success = true, Message = "Column preference retrieved", Data = result });
        }

        [HttpGet("by-mid/{mid:int}/{gridName}")]
        public async Task<ActionResult<ApiResponse<MerchantColumnPreferenceResponse>>> GetByMidAndGrid(int mid, string gridName)
        {
            var result = await _service.GetByMidAndGridAsync(mid, gridName);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Column preference not found" });
            return Ok(new ApiResponse<MerchantColumnPreferenceResponse> { Success = true, Message = "Column preference retrieved", Data = result });
        }

        [HttpDelete("{merchantColumnPreferenceId:int}")]
        public async Task<ActionResult<ApiResponse>> Delete(int merchantColumnPreferenceId)
        {
            var success = await _service.DeleteAsync(merchantColumnPreferenceId);
            if (!success) return NotFound(new ApiResponse { Success = false, Message = "Column preference not found" });
            return Ok(new ApiResponse { Success = true, Message = "Column preference deleted" });
        }
    }
}
