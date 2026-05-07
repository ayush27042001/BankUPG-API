using BankUPG.Application.Interfaces.Document;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<DocumentController> _logger;

        public DocumentController(
            IDocumentService documentService,
            ILogger<DocumentController> logger)
        {
            _documentService = documentService;
            _logger = logger;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }
            return userId;
        }

        [HttpPost("upload")]
        [ProducesResponseType(typeof(ApiResponse<DocumentUploadResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<DocumentUploadResponse>>> UploadDocument([FromForm] UploadDocumentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<DocumentUploadResponse>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var userId = GetUserId();
                var result = await _documentService.UploadDocumentAsync(userId, request);

                return Ok(new ApiResponse<DocumentUploadResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return StatusCode(500, new ApiResponse<DocumentUploadResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("update")]
        [ProducesResponseType(typeof(ApiResponse<DocumentUploadResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<DocumentUploadResponse>>> UpdateDocument([FromForm] UpdateDocumentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<DocumentUploadResponse>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var userId = GetUserId();
                var result = await _documentService.UpdateDocumentAsync(userId, request);

                return Ok(new ApiResponse<DocumentUploadResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document");
                return StatusCode(500, new ApiResponse<DocumentUploadResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{documentUploadId}")]
        [ProducesResponseType(typeof(ApiResponse<DocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<DocumentResponse>>> GetDocument(int documentUploadId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _documentService.GetDocumentAsync(userId, documentUploadId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<DocumentResponse>
                    {
                        Success = false,
                        Message = "Document not found"
                    });
                }

                return Ok(new ApiResponse<DocumentResponse>
                {
                    Success = true,
                    Message = "Document retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document");
                return StatusCode(500, new ApiResponse<DocumentResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("merchant")]
        [ProducesResponseType(typeof(ApiResponse<DocumentListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<DocumentListResponse>>> GetDocumentsByMerchant()
        {
            try
            {
                var userId = GetUserId();
                var result = await _documentService.GetDocumentsByMerchantAsync(userId);

                return Ok(new ApiResponse<DocumentListResponse>
                {
                    Success = true,
                    Message = "Documents retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents for merchant");
                return StatusCode(500, new ApiResponse<DocumentListResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("type/{documentTypeId}")]
        [ProducesResponseType(typeof(ApiResponse<DocumentListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<DocumentListResponse>>> GetDocumentsByType(int documentTypeId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _documentService.GetDocumentsByTypeAsync(userId, documentTypeId);

                return Ok(new ApiResponse<DocumentListResponse>
                {
                    Success = true,
                    Message = "Documents retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents by type");
                return StatusCode(500, new ApiResponse<DocumentListResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("download/{documentUploadId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DownloadDocument(int documentUploadId)
        {
            try
            {
                var userId = GetUserId();
                var (fileData, fileName, mimeType) = await _documentService.DownloadDocumentAsync(userId, documentUploadId);

                return File(fileData, mimeType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading document");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("{documentUploadId}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse>> DeleteDocument(int documentUploadId)
        {
            try
            {
                var userId = GetUserId();
                var result = await _documentService.DeleteDocumentAsync(userId, documentUploadId);

                if (!result)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Document not found"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Document deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("types")]
        [ProducesResponseType(typeof(ApiResponse<List<DocumentTypeResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<DocumentTypeResponse>>>> GetDocumentTypes()
        {
            try
            {
                var result = await _documentService.GetDocumentTypesAsync();

                return Ok(new ApiResponse<List<DocumentTypeResponse>>
                {
                    Success = true,
                    Message = "Document types retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document types");
                return StatusCode(500, new ApiResponse<List<DocumentTypeResponse>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("business-proof-types")]
        [ProducesResponseType(typeof(ApiResponse<List<BusinessProofTypeResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<BusinessProofTypeResponse>>>> GetBusinessProofTypes()
        {
            try
            {
                var result = await _documentService.GetBusinessProofTypesAsync();

                return Ok(new ApiResponse<List<BusinessProofTypeResponse>>
                {
                    Success = true,
                    Message = "Business proof types retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving business proof types");
                return StatusCode(500, new ApiResponse<List<BusinessProofTypeResponse>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
