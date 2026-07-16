
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BankUPG.Application.Interfaces.PEPStatusMaster;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace BankUPG.Application.Services.PEPStatusMaster


{
    public class PEPStatusMasterService : IPEPStatusMasterService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<PEPStatusMasterService> _logger;

        public PEPStatusMasterService(
            AppDBContext context,
            ILogger<PEPStatusMasterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PEPStatusMasterResponse> CreatePEPStatusAsync(CreatePEPStatusMasterRequest request)
        {
            // Check if Status Name already exists
            var existingStatus = await _context.Pepstatuses
                .AnyAsync(x => x.StatusName == request.StatusName);

            if (existingStatus)
                throw new InvalidOperationException(
                    $"PEP Status '{request.StatusName}' already exists.");

            var pepStatus = new Pepstatus
            {
                StatusName = request.StatusName,
                Description = request.Description,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Pepstatuses.Add(pepStatus);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "PEP Status created successfully: {PepstatusId} - {StatusName}",
                pepStatus.PepstatusId,
                pepStatus.StatusName);

            return new PEPStatusMasterResponse
            {
                PepstatusId = pepStatus.PepstatusId,
                StatusName = pepStatus.StatusName,
                Description = pepStatus.Description,
                IsActive = pepStatus.IsActive,
                CreatedDate = pepStatus.CreatedDate,
                UpdatedDate = pepStatus.UpdatedDate
            };
        }

        public async Task<PEPStatusMasterResponse?> GetPEPStatusByIdAsync(int pepStatusId)
        {
            var pepStatus = await _context.Pepstatuses
                .FirstOrDefaultAsync(x => x.PepstatusId == pepStatusId);

            if (pepStatus == null)
                return null;

            return new PEPStatusMasterResponse
            {
                PepstatusId = pepStatus.PepstatusId,
                StatusName = pepStatus.StatusName,
                Description = pepStatus.Description,
                IsActive = pepStatus.IsActive,
                CreatedDate = pepStatus.CreatedDate,
                UpdatedDate = pepStatus.UpdatedDate
            };
        }

        public async Task<List<PEPStatusMasterResponse>> GetAllPEPStatusAsync()
        {
            return await _context.Pepstatuses
                .OrderBy(x => x.StatusName)
                .Select(x => new PEPStatusMasterResponse
                {
                    PepstatusId = x.PepstatusId,
                    StatusName = x.StatusName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToListAsync();
        }

        public async Task<PagedResponse<PEPStatusMasterResponse>> GetPEPStatusListAsync(GetPEPStatusListRequest request)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 100 ? 100 : request.PageSize;

            var query = _context.Pepstatuses.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();

                query = query.Where(x =>
                    x.StatusName.ToLower().Contains(search) ||
                    (x.Description != null && x.Description.ToLower().Contains(search)));
            }

            // Active Filter
            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.StatusName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PEPStatusMasterResponse
                {
                    PepstatusId = x.PepstatusId,
                    StatusName = x.StatusName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToListAsync();

            return new PagedResponse<PEPStatusMasterResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PEPStatusMasterResponse> UpdatePEPStatusAsync(UpdatePEPStatusMasterRequest request)
        {
            var pepStatus = await _context.Pepstatuses
                .FirstOrDefaultAsync(x => x.PepstatusId == request.PepstatusId);

            if (pepStatus == null)
                throw new ArgumentException("PEP Status not found.");

            // Check duplicate Status Name (excluding current record)
            var existingStatus = await _context.Pepstatuses
                .AnyAsync(x => x.StatusName == request.StatusName &&
                               x.PepstatusId != request.PepstatusId);

            if (existingStatus)
                throw new InvalidOperationException(
                    $"PEP Status '{request.StatusName}' already exists.");

            // Update Values
            pepStatus.StatusName = request.StatusName;
            pepStatus.Description = request.Description;

            if (request.IsActive.HasValue)
                pepStatus.IsActive = request.IsActive.Value;

            pepStatus.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "PEP Status updated successfully: {PepstatusId} - {StatusName}",
                pepStatus.PepstatusId,
                pepStatus.StatusName);

            return new PEPStatusMasterResponse
            {
                PepstatusId = pepStatus.PepstatusId,
                StatusName = pepStatus.StatusName,
                Description = pepStatus.Description,
                IsActive = pepStatus.IsActive,
                CreatedDate = pepStatus.CreatedDate,
                UpdatedDate = pepStatus.UpdatedDate
            };
        }
    }
}