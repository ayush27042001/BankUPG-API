using BankUPG.Application.Interfaces.PepstatusMaster;
using BankUPG.SharedKernal.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BankUPG.API.Controllers
{
    /// <summary>
    /// PEP Status Master Controller - Handles PEP status master data
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PepstatusMasterController : ControllerBase
    {
        private readonly IPepstatusMasterService _pepstatusMasterService;
        private readonly ILogger<PepstatusMasterController> _logger;

        public PepstatusMasterController(
            IPepstatusMasterService pepstatusMasterService,
            ILogger<PepstatusMasterController> logger)
        {
            _pepstatusMasterService = pepstatusMasterService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active PEP statuses (master data)
        /// </summary>
        /// <returns>List of PEP statuses</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<PepstatusDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<PepstatusDto>>>> GetAllPepstatuses()
        {
            try
            {
                var pepstatuses = await _pepstatusMasterService.GetAllPepstatusesAsync();

                return Ok(new ApiResponse<List<PepstatusDto>>
                {
                    Success = true,
                    Message = "PEP statuses retrieved successfully",
                    Data = pepstatuses
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving PEP statuses");
                return StatusCode(500, new ApiResponse<List<PepstatusDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving PEP statuses"
                });
            }
        }
    }
}
