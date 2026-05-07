using BankUPG.Application.Interfaces.DocumentMaster;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Mvc;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentMasterController : ControllerBase
    {
        private readonly IDocumentMasterService _documentMasterService;
        private readonly ILogger<DocumentMasterController> _logger;

        public DocumentMasterController(
            IDocumentMasterService documentMasterService,
            ILogger<DocumentMasterController> logger)
        {
            _documentMasterService = documentMasterService;
            _logger = logger;
        }

        #region Document Type Endpoints

        [HttpPost("document-types")]
        [ProducesResponseType(typeof(ApiResponse<DocumentTypeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<DocumentTypeDetailResponse>>> CreateDocumentType([FromBody] CreateDocumentTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<DocumentTypeDetailResponse>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _documentMasterService.CreateDocumentTypeAsync(request);

                return Ok(new ApiResponse<DocumentTypeDetailResponse>
                {
                    Success = true,
                    Message = "Document type created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document type");
                return StatusCode(500, new ApiResponse<DocumentTypeDetailResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("document-types/{documentTypeId}")]
        [ProducesResponseType(typeof(ApiResponse<DocumentTypeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<DocumentTypeDetailResponse>>> GetDocumentTypeById(int documentTypeId)
        {
            try
            {
                var result = await _documentMasterService.GetDocumentTypeByIdAsync(documentTypeId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<DocumentTypeDetailResponse>
                    {
                        Success = false,
                        Message = "Document type not found"
                    });
                }

                return Ok(new ApiResponse<DocumentTypeDetailResponse>
                {
                    Success = true,
                    Message = "Document type retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document type");
                return StatusCode(500, new ApiResponse<DocumentTypeDetailResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("document-types")]
        [ProducesResponseType(typeof(ApiResponse<List<DocumentTypeDetailResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<DocumentTypeDetailResponse>>>> GetAllDocumentTypes()
        {
            try
            {
                var result = await _documentMasterService.GetAllDocumentTypesAsync();

                return Ok(new ApiResponse<List<DocumentTypeDetailResponse>>
                {
                    Success = true,
                    Message = "Document types retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document types");
                return StatusCode(500, new ApiResponse<List<DocumentTypeDetailResponse>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("document-types")]
        [ProducesResponseType(typeof(ApiResponse<DocumentTypeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<DocumentTypeDetailResponse>>> UpdateDocumentType([FromBody] UpdateDocumentTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<DocumentTypeDetailResponse>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _documentMasterService.UpdateDocumentTypeAsync(request);

                return Ok(new ApiResponse<DocumentTypeDetailResponse>
                {
                    Success = true,
                    Message = "Document type updated successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document type");
                return StatusCode(500, new ApiResponse<DocumentTypeDetailResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("document-types/{documentTypeId}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DeleteDocumentType(int documentTypeId)
        {
            try
            {
                var result = await _documentMasterService.DeleteDocumentTypeAsync(documentTypeId);

                if (!result)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Document type not found"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Document type deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document type");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPatch("document-types/{documentTypeId}/toggle-status")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> ToggleDocumentTypeStatus(int documentTypeId)
        {
            try
            {
                var result = await _documentMasterService.ToggleDocumentTypeStatusAsync(documentTypeId);

                if (!result)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Document type not found"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Document type status toggled successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling document type status");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        #endregion

        #region Business Proof Type Endpoints

        [HttpPost("business-proof-types")]
        [ProducesResponseType(typeof(ApiResponse<BusinessProofTypeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<BusinessProofTypeDetailResponse>>> CreateBusinessProofType([FromBody] CreateBusinessProofTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<BusinessProofTypeDetailResponse>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _documentMasterService.CreateBusinessProofTypeAsync(request);

                return Ok(new ApiResponse<BusinessProofTypeDetailResponse>
                {
                    Success = true,
                    Message = "Business proof type created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating business proof type");
                return StatusCode(500, new ApiResponse<BusinessProofTypeDetailResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("business-proof-types/{businessProofTypeId}")]
        [ProducesResponseType(typeof(ApiResponse<BusinessProofTypeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BusinessProofTypeDetailResponse>>> GetBusinessProofTypeById(int businessProofTypeId)
        {
            try
            {
                var result = await _documentMasterService.GetBusinessProofTypeByIdAsync(businessProofTypeId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<BusinessProofTypeDetailResponse>
                    {
                        Success = false,
                        Message = "Business proof type not found"
                    });
                }

                return Ok(new ApiResponse<BusinessProofTypeDetailResponse>
                {
                    Success = true,
                    Message = "Business proof type retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving business proof type");
                return StatusCode(500, new ApiResponse<BusinessProofTypeDetailResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("business-proof-types")]
        [ProducesResponseType(typeof(ApiResponse<List<BusinessProofTypeDetailResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<BusinessProofTypeDetailResponse>>>> GetAllBusinessProofTypes()
        {
            try
            {
                var result = await _documentMasterService.GetAllBusinessProofTypesAsync();

                return Ok(new ApiResponse<List<BusinessProofTypeDetailResponse>>
                {
                    Success = true,
                    Message = "Business proof types retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving business proof types");
                return StatusCode(500, new ApiResponse<List<BusinessProofTypeDetailResponse>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut("business-proof-types")]
        [ProducesResponseType(typeof(ApiResponse<BusinessProofTypeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<BusinessProofTypeDetailResponse>>> UpdateBusinessProofType([FromBody] UpdateBusinessProofTypeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<BusinessProofTypeDetailResponse>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _documentMasterService.UpdateBusinessProofTypeAsync(request);

                return Ok(new ApiResponse<BusinessProofTypeDetailResponse>
                {
                    Success = true,
                    Message = "Business proof type updated successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating business proof type");
                return StatusCode(500, new ApiResponse<BusinessProofTypeDetailResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("business-proof-types/{businessProofTypeId}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DeleteBusinessProofType(int businessProofTypeId)
        {
            try
            {
                var result = await _documentMasterService.DeleteBusinessProofTypeAsync(businessProofTypeId);

                if (!result)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Business proof type not found"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Business proof type deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting business proof type");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPatch("business-proof-types/{businessProofTypeId}/toggle-status")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> ToggleBusinessProofTypeStatus(int businessProofTypeId)
        {
            try
            {
                var result = await _documentMasterService.ToggleBusinessProofTypeStatusAsync(businessProofTypeId);

                if (!result)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Business proof type not found"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Business proof type status toggled successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling business proof type status");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        #endregion
    }
}
