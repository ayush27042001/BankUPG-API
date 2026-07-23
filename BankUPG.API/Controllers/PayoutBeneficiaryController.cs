using BankUPG.Application.Interfaces.PayoutBeneficiary;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/payout-beneficiaries")]
    [Authorize]
    [Produces("application/json")]
    public class PayoutBeneficiaryController : ControllerBase
    {
        private readonly IPayoutBeneficiaryService _service;
        private readonly ILogger<PayoutBeneficiaryController> _logger;

        public PayoutBeneficiaryController(IPayoutBeneficiaryService service, ILogger<PayoutBeneficiaryController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PayoutBeneficiaryResponse>>> Create([FromBody] CreatePayoutBeneficiaryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<PayoutBeneficiaryResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            try
            {
                var result = await _service.CreateBeneficiaryAsync(userId.Value, request);
                return Ok(new ApiResponse<PayoutBeneficiaryResponse> { Success = true, Message = "Beneficiary added", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating beneficiary");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpGet("{beneficiaryId:long}")]
        public async Task<ActionResult<ApiResponse<PayoutBeneficiaryResponse>>> Get(long beneficiaryId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetBeneficiaryAsync(userId.Value, beneficiaryId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Beneficiary not found" });
            return Ok(new ApiResponse<PayoutBeneficiaryResponse> { Success = true, Message = "Beneficiary retrieved", Data = result });
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<PayoutBeneficiaryResponse>>>> List([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.ListBeneficiariesAsync(userId.Value, pageNumber, pageSize);
            return Ok(new ApiResponse<PagedResponse<PayoutBeneficiaryResponse>> { Success = true, Message = "Beneficiaries retrieved", Data = result });
        }

        [HttpDelete("{beneficiaryId:long}")]
        public async Task<ActionResult<ApiResponse>> Delete(long beneficiaryId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var success = await _service.DeactivateBeneficiaryAsync(userId.Value, beneficiaryId);
            if (!success) return NotFound(new ApiResponse { Success = false, Message = "Beneficiary not found" });
            return Ok(new ApiResponse { Success = true, Message = "Beneficiary removed" });
        }
    }
}
