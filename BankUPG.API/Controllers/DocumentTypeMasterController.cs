using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BankUPG.Application.Interfaces.DocumentTypeMaster;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class DocumentTypeMasterController : ControllerBase
    {
        private readonly IDocumentTypeMasterService _documentTypeMasterService;
        private readonly ILogger<DocumentTypeMasterController> _logger;

        public DocumentTypeMasterController(
            IDocumentTypeMasterService documentTypeMasterService,
            ILogger<DocumentTypeMasterController> logger)
        {
            _documentTypeMasterService = documentTypeMasterService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DocumentTypeMasterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<DocumentTypeMasterResponse>>> CreateDocumentType(
            [FromBody] CreateDocumentTypeMasterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<DocumentTypeMasterResponse>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                            .ToList()
                    });
                }

                var result = await _documentTypeMasterService.CreateDocumentTypeAsync(request);

                return Ok(new ApiResponse<DocumentTypeMasterResponse>
                {
                    Success = true,
                    Message = "Document Type created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Document Type");

                return StatusCode(500, new ApiResponse<DocumentTypeMasterResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{documentTypeId}")]
        [ProducesResponseType(typeof(ApiResponse<DocumentTypeMasterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DocumentTypeMasterResponse>>> GetDocumentTypeById(int documentTypeId)
        {
            try
            {
                var result = await _documentTypeMasterService.GetDocumentTypeByIdAsync(documentTypeId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<DocumentTypeMasterResponse>
                    {
                        Success = false,
                        Message = "Document Type not found"
                    });
                }

                return Ok(new ApiResponse<DocumentTypeMasterResponse>
                {
                    Success = true,
                    Message = "Document Type retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Document Type");

                return StatusCode(500, new ApiResponse<DocumentTypeMasterResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<DocumentTypeMasterResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<DocumentTypeMasterResponse>>>> GetAllDocumentTypes()
        {
            try
            {
                var result = await _documentTypeMasterService.GetAllDocumentTypesAsync();

                return Ok(new ApiResponse<List<DocumentTypeMasterResponse>>
                {
                    Success = true,
                    Message = "Document Types retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Document Types");

                return StatusCode(500, new ApiResponse<List<DocumentTypeMasterResponse>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<DocumentTypeMasterResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<DocumentTypeMasterResponse>>>> GetDocumentTypeList(
            [FromQuery] GetDocumentTypeListRequest request)
        {
            try
            {
                var result = await _documentTypeMasterService.GetDocumentTypeListAsync(request);

                return Ok(new ApiResponse<PagedResponse<DocumentTypeMasterResponse>>
                {
                    Success = true,
                    Message = "Document Types retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Document Type list");

                return StatusCode(500, new ApiResponse<PagedResponse<DocumentTypeMasterResponse>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<DocumentTypeMasterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<DocumentTypeMasterResponse>>> UpdateDocumentType(
            [FromBody] UpdateDocumentTypeMasterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<DocumentTypeMasterResponse>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                            .ToList()
                    });
                }

                var result = await _documentTypeMasterService.UpdateDocumentTypeAsync(request);

                return Ok(new ApiResponse<DocumentTypeMasterResponse>
                {
                    Success = true,
                    Message = "Document Type updated successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Document Type");

                return StatusCode(500, new ApiResponse<DocumentTypeMasterResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}