using BankUPG.Application.Interfaces.Webhook;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    [Authorize(Roles = "SuperAdmin")]
    [Produces("application/json")]
    public class WebhookController : ControllerBase
    {
        private readonly IWebhookService _service;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IWebhookService service, ILogger<WebhookController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<WebhookResponse>>> Create([FromBody] CreateWebhookRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<WebhookResponse> { Success = false, Message = "Validation failed", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });

            try
            {
                var result = await _service.CreateAsync(request);
                return Ok(new ApiResponse<WebhookResponse> { Success = true, Message = "Webhook created", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating webhook");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred" });
            }
        }

        [HttpPut("{webhookId:int}")]
        public async Task<ActionResult<ApiResponse<WebhookResponse>>> Update(int webhookId, [FromBody] UpdateWebhookRequest request)
        {
            if (webhookId != request.WebhookId)
                return BadRequest(new ApiResponse { Success = false, Message = "ID mismatch" });

            var result = await _service.UpdateAsync(webhookId, request);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Webhook not found" });
            return Ok(new ApiResponse<WebhookResponse> { Success = true, Message = "Webhook updated", Data = result });
        }

        [HttpGet("{webhookId:int}")]
        public async Task<ActionResult<ApiResponse<WebhookResponse>>> Get(int webhookId)
        {
            var result = await _service.GetAsync(webhookId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Webhook not found" });
            return Ok(new ApiResponse<WebhookResponse> { Success = true, Message = "Webhook retrieved", Data = result });
        }

        [HttpGet("by-mid/{mid:int}")]
        public async Task<ActionResult<ApiResponse<PagedResponse<WebhookResponse>>>> ListByMid(int mid, int pageNumber = 1, int pageSize = 20)
        {
            var result = await _service.ListByMidAsync(mid, pageNumber, pageSize);
            return Ok(new ApiResponse<PagedResponse<WebhookResponse>> { Success = true, Message = "Webhooks retrieved", Data = result });
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<WebhookResponse>>>> List(int pageNumber = 1, int pageSize = 20)
        {
            var result = await _service.ListAsync(pageNumber, pageSize);
            return Ok(new ApiResponse<PagedResponse<WebhookResponse>> { Success = true, Message = "Webhooks retrieved", Data = result });
        }

        [HttpDelete("{webhookId:int}")]
        public async Task<ActionResult<ApiResponse>> Delete(int webhookId)
        {
            var success = await _service.DeleteAsync(webhookId);
            if (!success) return NotFound(new ApiResponse { Success = false, Message = "Webhook not found" });
            return Ok(new ApiResponse { Success = true, Message = "Webhook deleted" });
        }
    }
}
