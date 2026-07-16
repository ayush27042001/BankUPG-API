
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Mvc;
using BankUPG.Application.Interfaces.PEPStatusMaster;

namespace BankUPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PepstatusMasterController : ControllerBase
    {
        private readonly IPEPStatusMasterService _pepstatusMasterService;
        private readonly ILogger<PepstatusMasterController> _logger;

        public PepstatusMasterController(
            IPEPStatusMasterService pepstatusMasterService,
            ILogger<PepstatusMasterController> logger)
        {
            _pepstatusMasterService = pepstatusMasterService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PEPStatusMasterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PEPStatusMasterResponse>>> CreatePEPStatus(
            [FromBody] CreatePEPStatusMasterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<PEPStatusMasterResponse>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                            .ToList()
                    });
                }

                var result = await _pepstatusMasterService.CreatePEPStatusAsync(request);

                return Ok(new ApiResponse<PEPStatusMasterResponse>
                {
                    Success = true,
                    Message = "PEP Status created successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PEP Status");

                return StatusCode(500, new ApiResponse<PEPStatusMasterResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{pepstatusId}")]
        [ProducesResponseType(typeof(ApiResponse<PEPStatusMasterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<PEPStatusMasterResponse>>> GetPEPStatusById(int pepstatusId)
        {
            try
            {
                var result = await _pepstatusMasterService.GetPEPStatusByIdAsync(pepstatusId);

                if (result == null)
                {
                    return NotFound(new ApiResponse<PEPStatusMasterResponse>
                    {
                        Success = false,
                        Message = "PEP Status not found"
                    });
                }

                return Ok(new ApiResponse<PEPStatusMasterResponse>
                {
                    Success = true,
                    Message = "PEP Status retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PEP Status");

                return StatusCode(500, new ApiResponse<PEPStatusMasterResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<PEPStatusMasterResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<PEPStatusMasterResponse>>>> GetAllPEPStatus()
        {
            try
            {
                var result = await _pepstatusMasterService.GetAllPEPStatusAsync();

                return Ok(new ApiResponse<List<PEPStatusMasterResponse>>
                {
                    Success = true,
                    Message = "PEP Status retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PEP Status");

                return StatusCode(500, new ApiResponse<List<PEPStatusMasterResponse>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<PEPStatusMasterResponse>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<PagedResponse<PEPStatusMasterResponse>>>> GetPEPStatusList(
            [FromQuery] GetPEPStatusListRequest request)
        {
            try
            {
                var result = await _pepstatusMasterService.GetPEPStatusListAsync(request);

                return Ok(new ApiResponse<PagedResponse<PEPStatusMasterResponse>>
                {
                    Success = true,
                    Message = "PEP Status retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PEP Status list");

                return StatusCode(500, new ApiResponse<PagedResponse<PEPStatusMasterResponse>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<PEPStatusMasterResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<PEPStatusMasterResponse>>> UpdatePEPStatus(
            [FromBody] UpdatePEPStatusMasterRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<PEPStatusMasterResponse>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                            .ToList()
                    });
                }

                var result = await _pepstatusMasterService.UpdatePEPStatusAsync(request);

                return Ok(new ApiResponse<PEPStatusMasterResponse>
                {
                    Success = true,
                    Message = "PEP Status updated successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating PEP Status");

                return StatusCode(500, new ApiResponse<PEPStatusMasterResponse>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}