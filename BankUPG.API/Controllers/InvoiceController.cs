using BankUPG.Application.Interfaces.Invoice;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/invoices")]
    [Authorize]
    [Produces("application/json")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _service;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(IInvoiceService service, ILogger<InvoiceController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? GetUserId() =>
            int.TryParse(User.FindAll(ClaimTypes.NameIdentifier)
                .FirstOrDefault(c => int.TryParse(c.Value, out _))?.Value, out var id) ? id : null;

        [HttpPost]
        public async Task<ActionResult<ApiResponse<InvoiceResponse>>> Create([FromBody] CreateInvoiceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<InvoiceResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            try
            {
                var result = await _service.CreateInvoiceAsync(userId.Value, request);
                return Ok(new ApiResponse<InvoiceResponse> { Success = true, Message = "Invoice created", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpGet("{invoiceId:long}")]
        public async Task<ActionResult<ApiResponse<InvoiceResponse>>> Get(long invoiceId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.GetInvoiceAsync(userId.Value, invoiceId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Invoice not found" });
            return Ok(new ApiResponse<InvoiceResponse> { Success = true, Message = "Invoice retrieved", Data = result });
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<InvoiceResponse>>>> List([FromQuery] ListInvoicesRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var result = await _service.ListInvoicesAsync(userId.Value, request);
            return Ok(new ApiResponse<PagedResponse<InvoiceResponse>> { Success = true, Message = "Invoices retrieved", Data = result });
        }

        [HttpPost("{invoiceId:long}/send")]
        public async Task<ActionResult<ApiResponse>> Send(long invoiceId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var success = await _service.SendInvoiceAsync(userId.Value, invoiceId);
            if (!success) return BadRequest(new ApiResponse { Success = false, Message = "Invoice cannot be sent (already paid/cancelled or not found)" });
            return Ok(new ApiResponse { Success = true, Message = "Invoice marked as sent" });
        }

        [HttpPost("{invoiceId:long}/cancel")]
        public async Task<ActionResult<ApiResponse>> Cancel(long invoiceId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new ApiResponse { Success = false, Message = "Invalid token" });

            var success = await _service.CancelInvoiceAsync(userId.Value, invoiceId);
            if (!success) return BadRequest(new ApiResponse { Success = false, Message = "Invoice cannot be cancelled (already paid or not found)" });
            return Ok(new ApiResponse { Success = true, Message = "Invoice cancelled" });
        }
    }
}
