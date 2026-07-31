using BankUPG.Application.Interfaces.CheckoutCustomization;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/checkout-customizations")]
    [Authorize]
    [Produces("application/json")]
    public class CheckoutCustomizationController : ControllerBase
    {
        private readonly ICheckoutCustomizationService _service;
        private readonly ILogger<CheckoutCustomizationController> _logger;
        private readonly IWebHostEnvironment _env;

        public CheckoutCustomizationController(
            ICheckoutCustomizationService service,
            ILogger<CheckoutCustomizationController> logger,
            IWebHostEnvironment env)
        {
            _service = service;
            _logger = logger;
            _env = env;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        // ─────────────────────────────────────────────────────────────────────
        // MERCHANT SELF-SERVICE (any authenticated user)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Get own checkout customization (merchant).</summary>
        [HttpGet("my")]
        [ProducesResponseType(typeof(ApiResponse<CheckoutCustomizationResponse>), 200)]
        public async Task<ActionResult<ApiResponse<CheckoutCustomizationResponse>>> GetMy()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetByUserIdAsync(userId.Value);
            if (result == null)
                return Ok(new ApiResponse<CheckoutCustomizationResponse> { Success = true, Message = "No customization configured yet", Data = null });
            return Ok(new ApiResponse<CheckoutCustomizationResponse> { Success = true, Message = "Checkout customization retrieved", Data = result });
        }

        /// <summary>Create or update own checkout customization (merchant).</summary>
        [HttpPut("my")]
        [ProducesResponseType(typeof(ApiResponse<CheckoutCustomizationResponse>), 200)]
        public async Task<ActionResult<ApiResponse<CheckoutCustomizationResponse>>> UpsertMy([FromBody] MerchantCheckoutCustomizationRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            try
            {
                var result = await _service.UpsertByUserIdAsync(userId.Value, request);
                return Ok(new ApiResponse<CheckoutCustomizationResponse> { Success = true, Message = "Checkout customization saved", Data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting checkout customization");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        /// <summary>Upload brand logo (PNG/JPG, max 2 MB). Returns the hosted URL.</summary>
        [HttpPost("my/upload-logo")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<UploadAssetResponse>), 200)]
        public async Task<ActionResult<ApiResponse<UploadAssetResponse>>> UploadLogo(IFormFile file)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });
            if (file == null || file.Length == 0)
                return BadRequest(new ApiResponse { Success = false, Message = "No file provided" });

            try
            {
                var url = await _service.UploadAssetAsync(userId.Value, file, _env.WebRootPath, "logo");
                return Ok(new ApiResponse<UploadAssetResponse> { Success = true, Message = "Logo uploaded", Data = new UploadAssetResponse { Url = url } });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading logo");
                return StatusCode(500, new ApiResponse { Success = false, Message = "Upload failed" });
            }
        }

        /// <summary>Upload owner/authorised signatory signature (PNG/JPG, max 2 MB).</summary>
        [HttpPost("my/upload-signature")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<UploadAssetResponse>), 200)]
        public async Task<ActionResult<ApiResponse<UploadAssetResponse>>> UploadSignature(IFormFile file)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });
            if (file == null || file.Length == 0)
                return BadRequest(new ApiResponse { Success = false, Message = "No file provided" });

            try
            {
                var url = await _service.UploadAssetAsync(userId.Value, file, _env.WebRootPath, "signature");
                return Ok(new ApiResponse<UploadAssetResponse> { Success = true, Message = "Signature uploaded", Data = new UploadAssetResponse { Url = url } });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading signature");
                return StatusCode(500, new ApiResponse { Success = false, Message = "Upload failed" });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // SUPERADMIN ENDPOINTS
        // ─────────────────────────────────────────────────────────────────────

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<CheckoutCustomizationResponse>>> Create([FromBody] CreateCheckoutCustomizationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<CheckoutCustomizationResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            try
            {
                var result = await _service.CreateAsync(request);
                return Ok(new ApiResponse<CheckoutCustomizationResponse> { Success = true, Message = "Checkout customization created", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating checkout customization");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPut("{checkoutCustomizationId:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<CheckoutCustomizationResponse>>> Update(int checkoutCustomizationId, [FromBody] UpdateCheckoutCustomizationRequest request)
        {
            if (checkoutCustomizationId != request.CheckoutCustomizationId)
                return BadRequest(new ApiResponse { Success = false, Message = "ID mismatch" });

            var result = await _service.UpdateAsync(checkoutCustomizationId, request);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Checkout customization not found" });
            return Ok(new ApiResponse<CheckoutCustomizationResponse> { Success = true, Message = "Checkout customization updated", Data = result });
        }

        [HttpGet("{checkoutCustomizationId:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<CheckoutCustomizationResponse>>> Get(int checkoutCustomizationId)
        {
            var result = await _service.GetAsync(checkoutCustomizationId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Checkout customization not found" });
            return Ok(new ApiResponse<CheckoutCustomizationResponse> { Success = true, Message = "Checkout customization retrieved", Data = result });
        }

        [HttpGet("by-mid/{mid:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<CheckoutCustomizationResponse>>> GetByMid(int mid)
        {
            var result = await _service.GetByMidAsync(mid);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Checkout customization not found" });
            return Ok(new ApiResponse<CheckoutCustomizationResponse> { Success = true, Message = "Checkout customization retrieved", Data = result });
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<PagedResponse<CheckoutCustomizationResponse>>>> List(int pageNumber = 1, int pageSize = 20)
        {
            var result = await _service.ListAsync(pageNumber, pageSize);
            return Ok(new ApiResponse<PagedResponse<CheckoutCustomizationResponse>> { Success = true, Message = "Checkout customizations retrieved", Data = result });
        }

        [HttpDelete("{checkoutCustomizationId:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse>> Delete(int checkoutCustomizationId)
        {
            var success = await _service.DeleteAsync(checkoutCustomizationId);
            if (!success) return NotFound(new ApiResponse { Success = false, Message = "Checkout customization not found" });
            return Ok(new ApiResponse { Success = true, Message = "Checkout customization deleted" });
        }
    }
}
