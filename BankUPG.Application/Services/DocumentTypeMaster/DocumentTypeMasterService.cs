using BankUPG.Application.Interfaces.DocumentTypeMaster;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace BankUPG.Application.Services.DocumentTypeMaster
{
    public class DocumentTypeMasterService : IDocumentTypeMasterService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<DocumentTypeMasterService> _logger;

        public DocumentTypeMasterService(
            AppDBContext context,
            ILogger<DocumentTypeMasterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DocumentTypeMasterResponse> CreateDocumentTypeAsync(CreateDocumentTypeMasterRequest request)
        {
            // Check if Document Code already exists
            var existingCode = await _context.DocumentTypes
                .AnyAsync(x => x.DocumentCode == request.DocumentCode);

            if (existingCode)
                throw new InvalidOperationException($"Document Type with code '{request.DocumentCode}' already exists.");

            // Check if Document Name already exists
            var existingName = await _context.DocumentTypes
                .AnyAsync(x => x.DocumentName == request.DocumentName);

            if (existingName)
                throw new InvalidOperationException($"Document Type with name '{request.DocumentName}' already exists.");

            var documentType = new DocumentType
            {
                DocumentName = request.DocumentName,
                DocumentCode = request.DocumentCode,
                AllowedExtensions = request.AllowedExtensions,
                MaxFileSizeMb = request.MaxFileSizeMb,
                IsRequired = request.IsRequired,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.DocumentTypes.Add(documentType);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Document Type created successfully: {DocumentTypeId} - {DocumentName}",
                documentType.DocumentTypeId,
                documentType.DocumentName);

            return new DocumentTypeMasterResponse
            {
                DocumentTypeId = documentType.DocumentTypeId,
                DocumentName = documentType.DocumentName,
                DocumentCode = documentType.DocumentCode,
                AllowedExtensions = documentType.AllowedExtensions,
                MaxFileSizeMb = documentType.MaxFileSizeMb,
                IsRequired = documentType.IsRequired,
                IsActive = documentType.IsActive,
                CreatedDate = documentType.CreatedDate,
                UpdatedDate = documentType.UpdatedDate
            };
        }

        public async Task<DocumentTypeMasterResponse?> GetDocumentTypeByIdAsync(int documentTypeId)
        {
            var documentType = await _context.DocumentTypes
                .FirstOrDefaultAsync(x => x.DocumentTypeId == documentTypeId);

            if (documentType == null)
                return null;

            return new DocumentTypeMasterResponse
            {
                DocumentTypeId = documentType.DocumentTypeId,
                DocumentName = documentType.DocumentName,
                DocumentCode = documentType.DocumentCode,
                AllowedExtensions = documentType.AllowedExtensions,
                MaxFileSizeMb = documentType.MaxFileSizeMb,
                IsRequired = documentType.IsRequired,
                IsActive = documentType.IsActive,
                CreatedDate = documentType.CreatedDate,
                UpdatedDate = documentType.UpdatedDate
            };
        }

        public async Task<List<DocumentTypeMasterResponse>> GetAllDocumentTypesAsync()
        {
            return await _context.DocumentTypes
                .OrderBy(x => x.DocumentName)
                .Select(x => new DocumentTypeMasterResponse
                {
                    DocumentTypeId = x.DocumentTypeId,
                    DocumentName = x.DocumentName,
                    DocumentCode = x.DocumentCode,
                    AllowedExtensions = x.AllowedExtensions,
                    MaxFileSizeMb = x.MaxFileSizeMb,
                    IsRequired = x.IsRequired,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToListAsync();
        }

        public async Task<PagedResponse<DocumentTypeMasterResponse>> GetDocumentTypeListAsync(GetDocumentTypeListRequest request)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize > 100 ? 100 : request.PageSize;

            var query = _context.DocumentTypes.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();

                query = query.Where(x =>
                    x.DocumentName.ToLower().Contains(search) ||
                    x.DocumentCode.ToLower().Contains(search) ||
                    (x.AllowedExtensions != null && x.AllowedExtensions.ToLower().Contains(search)));
            }

            // Active Filter
            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.DocumentName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DocumentTypeMasterResponse
                {
                    DocumentTypeId = x.DocumentTypeId,
                    DocumentName = x.DocumentName,
                    DocumentCode = x.DocumentCode,
                    AllowedExtensions = x.AllowedExtensions,
                    MaxFileSizeMb = x.MaxFileSizeMb,
                    IsRequired = x.IsRequired,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate
                })
                .ToListAsync();

            return new PagedResponse<DocumentTypeMasterResponse>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<DocumentTypeMasterResponse> UpdateDocumentTypeAsync(UpdateDocumentTypeMasterRequest request)
        {
            var documentType = await _context.DocumentTypes
                .FirstOrDefaultAsync(x => x.DocumentTypeId == request.DocumentTypeId);

            if (documentType == null)
                throw new ArgumentException("Document Type not found.");

            // Check duplicate Document Code (excluding current record)
            var existingCode = await _context.DocumentTypes
                .AnyAsync(x => x.DocumentCode == request.DocumentCode &&
                               x.DocumentTypeId != request.DocumentTypeId);

            if (existingCode)
                throw new InvalidOperationException(
                    $"Document Type with code '{request.DocumentCode}' already exists.");

            // Check duplicate Document Name (excluding current record)
            var existingName = await _context.DocumentTypes
                .AnyAsync(x => x.DocumentName == request.DocumentName &&
                               x.DocumentTypeId != request.DocumentTypeId);

            if (existingName)
                throw new InvalidOperationException(
                    $"Document Type with name '{request.DocumentName}' already exists.");

            // Update Values
            documentType.DocumentName = request.DocumentName;
            documentType.DocumentCode = request.DocumentCode;
            documentType.AllowedExtensions = request.AllowedExtensions;
            documentType.MaxFileSizeMb = request.MaxFileSizeMb;
            documentType.IsRequired = request.IsRequired;

            if (request.IsActive.HasValue)
                documentType.IsActive = request.IsActive.Value;

            documentType.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Document Type updated successfully: {DocumentTypeId} - {DocumentName}",
                documentType.DocumentTypeId,
                documentType.DocumentName);

            return new DocumentTypeMasterResponse
            {
                DocumentTypeId = documentType.DocumentTypeId,
                DocumentName = documentType.DocumentName,
                DocumentCode = documentType.DocumentCode,
                AllowedExtensions = documentType.AllowedExtensions,
                MaxFileSizeMb = documentType.MaxFileSizeMb,
                IsRequired = documentType.IsRequired,
                IsActive = documentType.IsActive,
                CreatedDate = documentType.CreatedDate,
                UpdatedDate = documentType.UpdatedDate
            };
        }
    }
}