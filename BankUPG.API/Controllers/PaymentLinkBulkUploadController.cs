using BankUPG.Application.Interfaces.PaymentLinkBulkUpload;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/payment-link-bulk-uploads")]
    [Authorize]
    [Produces("application/json")]
    public class PaymentLinkBulkUploadController : ControllerBase
    {
        private readonly IPaymentLinkBulkUploadService _service;
        private readonly ILogger<PaymentLinkBulkUploadController> _logger;

        public PaymentLinkBulkUploadController(IPaymentLinkBulkUploadService service, ILogger<PaymentLinkBulkUploadController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<PaymentLinkBulkUploadResponse>>> Create([FromBody] CreatePaymentLinkBulkUploadRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<PaymentLinkBulkUploadResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            try
            {
                var result = await _service.CreateAsync(userId.Value, request);
                return Ok(new ApiResponse<PaymentLinkBulkUploadResponse> { Success = true, Message = "Bulk upload created", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk upload");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<PaymentLinkBulkUploadResponse>>>> List([FromQuery] ListPaymentLinkBulkUploadsRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.ListAsync(userId.Value, request);
            return Ok(new ApiResponse<PagedResponse<PaymentLinkBulkUploadResponse>> { Success = true, Message = "Bulk uploads retrieved", Data = result });
        }

        [HttpGet("{bulkUploadId:long}")]
        public async Task<ActionResult<ApiResponse<PaymentLinkBulkUploadResponse>>> Get(long bulkUploadId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetAsync(userId.Value, bulkUploadId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Bulk upload not found" });
            return Ok(new ApiResponse<PaymentLinkBulkUploadResponse> { Success = true, Message = "Bulk upload retrieved", Data = result });
        }

        [HttpPost("files")]
        public async Task<ActionResult<ApiResponse<PaymentLinkBulkUploadFileResponse>>> AddFile([FromBody] CreatePaymentLinkBulkUploadFileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<PaymentLinkBulkUploadFileResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.AddFileAsync(userId.Value, request);
            return Ok(new ApiResponse<PaymentLinkBulkUploadFileResponse> { Success = true, Message = "File added", Data = result });
        }

        [HttpGet("files")]
        public async Task<ActionResult<ApiResponse<PagedResponse<PaymentLinkBulkUploadFileResponse>>>> ListFiles(int pageNumber = 1, int pageSize = 20)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.ListFilesAsync(userId.Value, pageNumber, pageSize);
            return Ok(new ApiResponse<PagedResponse<PaymentLinkBulkUploadFileResponse>> { Success = true, Message = "Files retrieved", Data = result });
        }
    }
}
