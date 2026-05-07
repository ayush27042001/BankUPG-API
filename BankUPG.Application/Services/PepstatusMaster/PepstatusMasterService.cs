using BankUPG.Application.Interfaces.PepstatusMaster;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.PepstatusMaster
{
    public class PepstatusMasterService : IPepstatusMasterService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<PepstatusMasterService> _logger;

        public PepstatusMasterService(
            AppDBContext context,
            ILogger<PepstatusMasterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PepstatusDto>> GetAllPepstatusesAsync()
        {
            var pepstatuses = await _context.Pepstatuses
                .AsNoTracking()
                .Where(p => p.IsActive == true)
                .OrderBy(p => p.PepstatusId)
                .Select(p => new PepstatusDto
                {
                    PepstatusId = p.PepstatusId,
                    StatusName = p.StatusName,
                    Description = p.Description
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} active PEP statuses", pepstatuses.Count);

            return pepstatuses;
        }
    }
}
