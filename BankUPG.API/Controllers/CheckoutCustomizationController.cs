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
    [Authorize(Roles = "SuperAdmin")]
    [Produces("application/json")]
    public class CheckoutCustomizationController : ControllerBase
    {
        private readonly ICheckoutCustomizationService _service;
        private readonly ILogger<CheckoutCustomizationController> _logger;

        public CheckoutCustomizationController(ICheckoutCustomizationService service, ILogger<CheckoutCustomizationController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
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
        public async Task<ActionResult<ApiResponse<CheckoutCustomizationResponse>>> Update(int checkoutCustomizationId, [FromBody] UpdateCheckoutCustomizationRequest request)
        {
            if (checkoutCustomizationId != request.CheckoutCustomizationId)
                return BadRequest(new ApiResponse { Success = false, Message = "ID mismatch" });

            var result = await _service.UpdateAsync(checkoutCustomizationId, request);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Checkout customization not found" });
            return Ok(new ApiResponse<CheckoutCustomizationResponse> { Success = true, Message = "Checkout customization updated", Data = result });
        }

        [HttpGet("{checkoutCustomizationId:int}")]
        public async Task<ActionResult<ApiResponse<CheckoutCustomizationResponse>>> Get(int checkoutCustomizationId)
        {
            var result = await _service.GetAsync(checkoutCustomizationId);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Checkout customization not found" });
            return Ok(new ApiResponse<CheckoutCustomizationResponse> { Success = true, Message = "Checkout customization retrieved", Data = result });
        }

        [HttpGet("by-mid/{mid:int}")]
        public async Task<ActionResult<ApiResponse<CheckoutCustomizationResponse>>> GetByMid(int mid)
        {
            var result = await _service.GetByMidAsync(mid);
            if (result == null) return NotFound(new ApiResponse { Success = false, Message = "Checkout customization not found" });
            return Ok(new ApiResponse<CheckoutCustomizationResponse> { Success = true, Message = "Checkout customization retrieved", Data = result });
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<CheckoutCustomizationResponse>>>> List(int pageNumber = 1, int pageSize = 20)
        {
            var result = await _service.ListAsync(pageNumber, pageSize);
            return Ok(new ApiResponse<PagedResponse<CheckoutCustomizationResponse>> { Success = true, Message = "Checkout customizations retrieved", Data = result });
        }

        [HttpDelete("{checkoutCustomizationId:int}")]
        public async Task<ActionResult<ApiResponse>> Delete(int checkoutCustomizationId)
        {
            var success = await _service.DeleteAsync(checkoutCustomizationId);
            if (!success) return NotFound(new ApiResponse { Success = false, Message = "Checkout customization not found" });
            return Ok(new ApiResponse { Success = true, Message = "Checkout customization deleted" });
        }
    }
}
