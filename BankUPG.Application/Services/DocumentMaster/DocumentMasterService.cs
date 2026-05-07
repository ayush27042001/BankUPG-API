using BankUPG.Application.Interfaces.DocumentMaster;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.DocumentMaster
{
    public class DocumentMasterService : IDocumentMasterService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<DocumentMasterService> _logger;

        public DocumentMasterService(
            AppDBContext context,
            ILogger<DocumentMasterService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Document Type Operations

        public async Task<DocumentTypeDetailResponse> CreateDocumentTypeAsync(CreateDocumentTypeRequest request)
        {
            // Check if document code already exists
            var existingCode = await _context.DocumentTypes
                .AnyAsync(dt => dt.DocumentCode == request.DocumentCode);

            if (existingCode)
                throw new InvalidOperationException($"Document type with code '{request.DocumentCode}' already exists.");

            // Check if document name already exists
            var existingName = await _context.DocumentTypes
                .AnyAsync(dt => dt.DocumentName == request.DocumentName);

            if (existingName)
                throw new InvalidOperationException($"Document type with name '{request.DocumentName}' already exists.");

            var documentType = new DocumentType
            {
                DocumentName = request.DocumentName,
                DocumentCode = request.DocumentCode,
                AllowedExtensions = request.AllowedExtensions ?? "JPG,PNG,PDF",
                MaxFileSizeMb = request.MaxFileSizeMb ?? 5,
                IsRequired = request.IsRequired ?? false,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.DocumentTypes.Add(documentType);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Document type created successfully: {DocumentTypeId} - {DocumentName}", 
                documentType.DocumentTypeId, documentType.DocumentName);

            return new DocumentTypeDetailResponse
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

        public async Task<DocumentTypeDetailResponse?> GetDocumentTypeByIdAsync(int documentTypeId)
        {
            var documentType = await _context.DocumentTypes
                .FirstOrDefaultAsync(dt => dt.DocumentTypeId == documentTypeId);

            if (documentType == null)
                return null;

            return new DocumentTypeDetailResponse
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

        public async Task<List<DocumentTypeDetailResponse>> GetAllDocumentTypesAsync()
        {
            return await _context.DocumentTypes
                .OrderBy(dt => dt.DocumentName)
                .Select(dt => new DocumentTypeDetailResponse
                {
                    DocumentTypeId = dt.DocumentTypeId,
                    DocumentName = dt.DocumentName,
                    DocumentCode = dt.DocumentCode,
                    AllowedExtensions = dt.AllowedExtensions,
                    MaxFileSizeMb = dt.MaxFileSizeMb,
                    IsRequired = dt.IsRequired,
                    IsActive = dt.IsActive,
                    CreatedDate = dt.CreatedDate,
                    UpdatedDate = dt.UpdatedDate
                })
                .ToListAsync();
        }

        public async Task<DocumentTypeDetailResponse> UpdateDocumentTypeAsync(UpdateDocumentTypeRequest request)
        {
            var documentType = await _context.DocumentTypes
                .FirstOrDefaultAsync(dt => dt.DocumentTypeId == request.DocumentTypeId);

            if (documentType == null)
                throw new ArgumentException("Document type not found.");

            // Check if document code already exists (excluding current record)
            var existingCode = await _context.DocumentTypes
                .AnyAsync(dt => dt.DocumentCode == request.DocumentCode && dt.DocumentTypeId != request.DocumentTypeId);

            if (existingCode)
                throw new InvalidOperationException($"Document type with code '{request.DocumentCode}' already exists.");

            // Check if document name already exists (excluding current record)
            var existingName = await _context.DocumentTypes
                .AnyAsync(dt => dt.DocumentName == request.DocumentName && dt.DocumentTypeId != request.DocumentTypeId);

            if (existingName)
                throw new InvalidOperationException($"Document type with name '{request.DocumentName}' already exists.");

            documentType.DocumentName = request.DocumentName;
            documentType.DocumentCode = request.DocumentCode;
            documentType.AllowedExtensions = request.AllowedExtensions;
            documentType.MaxFileSizeMb = request.MaxFileSizeMb;
            documentType.IsRequired = request.IsRequired;
            if (request.IsActive.HasValue)
                documentType.IsActive = request.IsActive;
            documentType.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Document type updated successfully: {DocumentTypeId} - {DocumentName}", 
                documentType.DocumentTypeId, documentType.DocumentName);

            return new DocumentTypeDetailResponse
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

        public async Task<bool> DeleteDocumentTypeAsync(int documentTypeId)
        {
            var documentType = await _context.DocumentTypes
                .FirstOrDefaultAsync(dt => dt.DocumentTypeId == documentTypeId);

            if (documentType == null)
                return false;

            // Check if any documents are uploaded with this type
            var hasUploads = await _context.DocumentUploads
                .AnyAsync(du => du.DocumentTypeId == documentTypeId);

            if (hasUploads)
                throw new InvalidOperationException("Cannot delete document type. Documents are already uploaded with this type.");

            _context.DocumentTypes.Remove(documentType);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Document type deleted successfully: {DocumentTypeId}", documentTypeId);

            return true;
        }

        public async Task<bool> ToggleDocumentTypeStatusAsync(int documentTypeId)
        {
            var documentType = await _context.DocumentTypes
                .FirstOrDefaultAsync(dt => dt.DocumentTypeId == documentTypeId);

            if (documentType == null)
                return false;

            documentType.IsActive = !documentType.IsActive;
            documentType.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Document type status toggled: {DocumentTypeId} - IsActive: {IsActive}", 
                documentTypeId, documentType.IsActive);

            return true;
        }

        #endregion

        #region Business Proof Type Operations

        public async Task<BusinessProofTypeDetailResponse> CreateBusinessProofTypeAsync(CreateBusinessProofTypeRequest request)
        {
            // Check if proof code already exists
            var existingCode = await _context.BusinessProofTypes
                .AnyAsync(bpt => bpt.ProofCode == request.ProofCode);

            if (existingCode)
                throw new InvalidOperationException($"Business proof type with code '{request.ProofCode}' already exists.");

            // Check if proof name already exists
            var existingName = await _context.BusinessProofTypes
                .AnyAsync(bpt => bpt.ProofName == request.ProofName);

            if (existingName)
                throw new InvalidOperationException($"Business proof type with name '{request.ProofName}' already exists.");

            var businessProofType = new BusinessProofType
            {
                ProofName = request.ProofName,
                ProofCode = request.ProofCode,
                Description = request.Description,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.BusinessProofTypes.Add(businessProofType);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Business proof type created successfully: {BusinessProofTypeId} - {ProofName}", 
                businessProofType.BusinessProofTypeId, businessProofType.ProofName);

            return new BusinessProofTypeDetailResponse
            {
                BusinessProofTypeId = businessProofType.BusinessProofTypeId,
                ProofName = businessProofType.ProofName,
                ProofCode = businessProofType.ProofCode,
                Description = businessProofType.Description,
                IsActive = businessProofType.IsActive,
                CreatedDate = businessProofType.CreatedDate,
                UpdatedDate = businessProofType.UpdatedDate
            };
        }

        public async Task<BusinessProofTypeDetailResponse?> GetBusinessProofTypeByIdAsync(int businessProofTypeId)
        {
            var businessProofType = await _context.BusinessProofTypes
                .FirstOrDefaultAsync(bpt => bpt.BusinessProofTypeId == businessProofTypeId);

            if (businessProofType == null)
                return null;

            return new BusinessProofTypeDetailResponse
            {
                BusinessProofTypeId = businessProofType.BusinessProofTypeId,
                ProofName = businessProofType.ProofName,
                ProofCode = businessProofType.ProofCode,
                Description = businessProofType.Description,
                IsActive = businessProofType.IsActive,
                CreatedDate = businessProofType.CreatedDate,
                UpdatedDate = businessProofType.UpdatedDate
            };
        }

        public async Task<List<BusinessProofTypeDetailResponse>> GetAllBusinessProofTypesAsync()
        {
            return await _context.BusinessProofTypes
                .OrderBy(bpt => bpt.ProofName)
                .Select(bpt => new BusinessProofTypeDetailResponse
                {
                    BusinessProofTypeId = bpt.BusinessProofTypeId,
                    ProofName = bpt.ProofName,
                    ProofCode = bpt.ProofCode,
                    Description = bpt.Description,
                    IsActive = bpt.IsActive,
                    CreatedDate = bpt.CreatedDate,
                    UpdatedDate = bpt.UpdatedDate
                })
                .ToListAsync();
        }

        public async Task<BusinessProofTypeDetailResponse> UpdateBusinessProofTypeAsync(UpdateBusinessProofTypeRequest request)
        {
            var businessProofType = await _context.BusinessProofTypes
                .FirstOrDefaultAsync(bpt => bpt.BusinessProofTypeId == request.BusinessProofTypeId);

            if (businessProofType == null)
                throw new ArgumentException("Business proof type not found.");

            // Check if proof code already exists (excluding current record)
            var existingCode = await _context.BusinessProofTypes
                .AnyAsync(bpt => bpt.ProofCode == request.ProofCode && bpt.BusinessProofTypeId != request.BusinessProofTypeId);

            if (existingCode)
                throw new InvalidOperationException($"Business proof type with code '{request.ProofCode}' already exists.");

            // Check if proof name already exists (excluding current record)
            var existingName = await _context.BusinessProofTypes
                .AnyAsync(bpt => bpt.ProofName == request.ProofName && bpt.BusinessProofTypeId != request.BusinessProofTypeId);

            if (existingName)
                throw new InvalidOperationException($"Business proof type with name '{request.ProofName}' already exists.");

            businessProofType.ProofName = request.ProofName;
            businessProofType.ProofCode = request.ProofCode;
            businessProofType.Description = request.Description;
            if (request.IsActive.HasValue)
                businessProofType.IsActive = request.IsActive;
            businessProofType.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Business proof type updated successfully: {BusinessProofTypeId} - {ProofName}", 
                businessProofType.BusinessProofTypeId, businessProofType.ProofName);

            return new BusinessProofTypeDetailResponse
            {
                BusinessProofTypeId = businessProofType.BusinessProofTypeId,
                ProofName = businessProofType.ProofName,
                ProofCode = businessProofType.ProofCode,
                Description = businessProofType.Description,
                IsActive = businessProofType.IsActive,
                CreatedDate = businessProofType.CreatedDate,
                UpdatedDate = businessProofType.UpdatedDate
            };
        }

        public async Task<bool> DeleteBusinessProofTypeAsync(int businessProofTypeId)
        {
            var businessProofType = await _context.BusinessProofTypes
                .FirstOrDefaultAsync(bpt => bpt.BusinessProofTypeId == businessProofTypeId);

            if (businessProofType == null)
                return false;

            // Check if any documents are uploaded with this proof type
            var hasUploads = await _context.DocumentUploads
                .AnyAsync(du => du.BusinessProofTypeId == businessProofTypeId);

            if (hasUploads)
                throw new InvalidOperationException("Cannot delete business proof type. Documents are already uploaded with this type.");

            _context.BusinessProofTypes.Remove(businessProofType);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Business proof type deleted successfully: {BusinessProofTypeId}", businessProofTypeId);

            return true;
        }

        public async Task<bool> ToggleBusinessProofTypeStatusAsync(int businessProofTypeId)
        {
            var businessProofType = await _context.BusinessProofTypes
                .FirstOrDefaultAsync(bpt => bpt.BusinessProofTypeId == businessProofTypeId);

            if (businessProofType == null)
                return false;

            businessProofType.IsActive = !businessProofType.IsActive;
            businessProofType.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Business proof type status toggled: {BusinessProofTypeId} - IsActive: {IsActive}", 
                businessProofTypeId, businessProofType.IsActive);

            return true;
        }

        #endregion
    }
}
