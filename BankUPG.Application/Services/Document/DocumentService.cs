using BankUPG.Application.Interfaces.Document;
using BankUPG.Application.Services.Auth;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Models;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.Document
{
    public class DocumentService : IDocumentService
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly AppSettings _appSettings;
        private readonly ILogger<DocumentService> _logger;
        private readonly string _uploadPath;

        public DocumentService(
            AppDBContext context,
            JwtService jwtService,
            AppSettings appSettings,
            ILogger<DocumentService> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _appSettings = appSettings;
            _logger = logger;
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Documents");
            
            // Ensure upload directory exists
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<DocumentUploadResponse> UploadDocumentAsync(int userId, UploadDocumentRequest request)
        {
            var merchant = await _context.Merchants
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null || merchant.User == null)
                throw new InvalidOperationException("User or merchant not found. Please ensure you are logged in.");

            // Validate document type
            var documentType = await _context.DocumentTypes
                .FirstOrDefaultAsync(dt => dt.DocumentTypeId == request.DocumentTypeId && dt.IsActive == true);

            if (documentType == null)
                throw new ArgumentException("Invalid or inactive document type.");

            // Validate business proof type if provided
            if (request.BusinessProofTypeId.HasValue)
            {
                var businessProofType = await _context.BusinessProofTypes
                    .FirstOrDefaultAsync(bpt => bpt.BusinessProofTypeId == request.BusinessProofTypeId.Value && bpt.IsActive == true);

                if (businessProofType == null)
                    throw new ArgumentException("Invalid or inactive business proof type.");
            }

            // Validate file
            var fileExtension = Path.GetExtension(request.File.FileName).ToLower().TrimStart('.');
            var allowedExtensions = documentType.AllowedExtensions?.Split(',') ?? new[] { "jpg", "png", "pdf" };

            if (!allowedExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Invalid file type. Allowed types: {documentType.AllowedExtensions}");

            var maxFileSize = documentType.MaxFileSizeMb ?? 5;
            if (request.File.Length > maxFileSize * 1024 * 1024)
                throw new ArgumentException($"File size exceeds maximum allowed size of {maxFileSize}MB.");

            // Generate unique file name
            var uniqueFileName = $"{Guid.NewGuid()}_{request.File.FileName}";
            var filePath = Path.Combine(_uploadPath, uniqueFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            // Create document upload record
            var documentUpload = new DocumentUpload
            {
                Mid = merchant.Mid,
                DocumentTypeId = request.DocumentTypeId,
                BusinessProofTypeId = request.BusinessProofTypeId,
                DocumentFileName = request.File.FileName,
                DocumentFilePath = filePath,
                DocumentSizeBytes = request.File.Length,
                DocumentMimeType = request.File.ContentType,
                IsVerified = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.DocumentUploads.Add(documentUpload);
            await _context.SaveChangesAsync();

            // Generate new token
            var token = _jwtService.GenerateToken(
                merchant.User.Email,
                merchant.User.Email,
                string.Empty,
                merchant.User.UserId
            );

            _logger.LogInformation("Document uploaded successfully for userId: {UserId}, mid: {Mid}, documentUploadId: {DocumentUploadId}",
                userId, merchant.Mid, documentUpload.DocumentUploadId);

            return new DocumentUploadResponse
            {
                DocumentUploadId = documentUpload.DocumentUploadId,
                DocumentFileName = documentUpload.DocumentFileName,
                DocumentFilePath = documentUpload.DocumentFilePath,
                Message = "Document uploaded successfully",
                Token = token,
                TokenExpiration = DateTime.UtcNow.AddMinutes(_appSettings.Jwt.ExpirationMinutes),
                OnboardingStatus = await BuildOnboardingStatusAsync(merchant.Mid)
            };
        }

        public async Task<DocumentUploadResponse> UpdateDocumentAsync(int userId, UpdateDocumentRequest request)
        {
            var merchant = await _context.Merchants
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null || merchant.User == null)
                throw new InvalidOperationException("User or merchant not found. Please ensure you are logged in.");

            var documentUpload = await _context.DocumentUploads
                .FirstOrDefaultAsync(d => d.DocumentUploadId == request.DocumentUploadId && d.Mid == merchant.Mid);

            if (documentUpload == null)
                throw new ArgumentException("Document not found or you don't have permission to update it.");

            // Validate document type
            var documentType = await _context.DocumentTypes
                .FirstOrDefaultAsync(dt => dt.DocumentTypeId == request.DocumentTypeId && dt.IsActive == true);

            if (documentType == null)
                throw new ArgumentException("Invalid or inactive document type.");

            // Validate business proof type if provided
            if (request.BusinessProofTypeId.HasValue)
            {
                var businessProofType = await _context.BusinessProofTypes
                    .FirstOrDefaultAsync(bpt => bpt.BusinessProofTypeId == request.BusinessProofTypeId.Value && bpt.IsActive == true);

                if (businessProofType == null)
                    throw new ArgumentException("Invalid or inactive business proof type.");
            }

            // Update document type and business proof type
            documentUpload.DocumentTypeId = request.DocumentTypeId;
            documentUpload.BusinessProofTypeId = request.BusinessProofTypeId;

            // Update file if provided
            if (request.File != null)
            {
                // Delete old file
                if (File.Exists(documentUpload.DocumentFilePath))
                {
                    File.Delete(documentUpload.DocumentFilePath);
                }

                // Validate new file
                var fileExtension = Path.GetExtension(request.File.FileName).ToLower().TrimStart('.');
                var allowedExtensions = documentType.AllowedExtensions?.Split(',') ?? new[] { "jpg", "png", "pdf" };

                if (!allowedExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                    throw new ArgumentException($"Invalid file type. Allowed types: {documentType.AllowedExtensions}");

                var maxFileSize = documentType.MaxFileSizeMb ?? 5;
                if (request.File.Length > maxFileSize * 1024 * 1024)
                    throw new ArgumentException($"File size exceeds maximum allowed size of {maxFileSize}MB.");

                // Save new file
                var uniqueFileName = $"{Guid.NewGuid()}_{request.File.FileName}";
                var filePath = Path.Combine(_uploadPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                documentUpload.DocumentFileName = request.File.FileName;
                documentUpload.DocumentFilePath = filePath;
                documentUpload.DocumentSizeBytes = request.File.Length;
                documentUpload.DocumentMimeType = request.File.ContentType;
            }

            documentUpload.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate new token
            var token = _jwtService.GenerateToken(
                merchant.User.Email,
                merchant.User.Email,
                string.Empty,
                merchant.User.UserId
            );

            _logger.LogInformation("Document updated successfully for userId: {UserId}, mid: {Mid}, documentUploadId: {DocumentUploadId}",
                userId, merchant.Mid, documentUpload.DocumentUploadId);

            return new DocumentUploadResponse
            {
                DocumentUploadId = documentUpload.DocumentUploadId,
                DocumentFileName = documentUpload.DocumentFileName,
                DocumentFilePath = documentUpload.DocumentFilePath,
                Message = "Document updated successfully",
                Token = token,
                TokenExpiration = DateTime.UtcNow.AddMinutes(_appSettings.Jwt.ExpirationMinutes),
                OnboardingStatus = await BuildOnboardingStatusAsync(merchant.Mid)
            };
        }

        public async Task<DocumentResponse?> GetDocumentAsync(int userId, int documentUploadId)
        {
            var merchant = await _context.Merchants
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null)
                throw new InvalidOperationException("User or merchant not found. Please ensure you are logged in.");

            var documentUpload = await _context.DocumentUploads
                .Include(d => d.DocumentType)
                .Include(d => d.BusinessProofType)
                .FirstOrDefaultAsync(d => d.DocumentUploadId == documentUploadId && d.Mid == merchant.Mid);

            if (documentUpload == null)
                return null;

            return new DocumentResponse
            {
                DocumentUploadId = documentUpload.DocumentUploadId,
                Mid = documentUpload.Mid,
                DocumentTypeId = documentUpload.DocumentTypeId,
                DocumentTypeName = documentUpload.DocumentType.DocumentName,
                DocumentTypeCode = documentUpload.DocumentType.DocumentCode,
                BusinessProofTypeId = documentUpload.BusinessProofTypeId,
                BusinessProofTypeName = documentUpload.BusinessProofType?.ProofName,
                DocumentFileName = documentUpload.DocumentFileName,
                DocumentFilePath = documentUpload.DocumentFilePath,
                DocumentSizeBytes = documentUpload.DocumentSizeBytes,
                DocumentMimeType = documentUpload.DocumentMimeType,
                IsVerified = documentUpload.IsVerified,
                VerifiedBy = documentUpload.VerifiedBy,
                VerifiedDate = documentUpload.VerifiedDate,
                RejectionReason = documentUpload.RejectionReason,
                CreatedDate = documentUpload.CreatedDate,
                UpdatedDate = documentUpload.UpdatedDate
            };
        }

        public async Task<DocumentListResponse> GetDocumentsByMerchantAsync(int userId)
        {
            var merchant = await _context.Merchants
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null)
                throw new InvalidOperationException("User or merchant not found. Please ensure you are logged in.");

            var documents = await _context.DocumentUploads
                .Include(d => d.DocumentType)
                .Include(d => d.BusinessProofType)
                .Where(d => d.Mid == merchant.Mid)
                .OrderByDescending(d => d.CreatedDate)
                .Select(d => new DocumentResponse
                {
                    DocumentUploadId = d.DocumentUploadId,
                    Mid = d.Mid,
                    DocumentTypeId = d.DocumentTypeId,
                    DocumentTypeName = d.DocumentType.DocumentName,
                    DocumentTypeCode = d.DocumentType.DocumentCode,
                    BusinessProofTypeId = d.BusinessProofTypeId,
                    BusinessProofTypeName = d.BusinessProofType.ProofName,
                    DocumentFileName = d.DocumentFileName,
                    DocumentFilePath = d.DocumentFilePath,
                    DocumentSizeBytes = d.DocumentSizeBytes,
                    DocumentMimeType = d.DocumentMimeType,
                    IsVerified = d.IsVerified,
                    VerifiedBy = d.VerifiedBy,
                    VerifiedDate = d.VerifiedDate,
                    RejectionReason = d.RejectionReason,
                    CreatedDate = d.CreatedDate,
                    UpdatedDate = d.UpdatedDate
                })
                .ToListAsync();

            return new DocumentListResponse
            {
                Documents = documents,
                TotalCount = documents.Count
            };
        }

        public async Task<DocumentListResponse> GetDocumentsByTypeAsync(int userId, int documentTypeId)
        {
            var merchant = await _context.Merchants
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null)
                throw new InvalidOperationException("User or merchant not found. Please ensure you are logged in.");

            var documents = await _context.DocumentUploads
                .Include(d => d.DocumentType)
                .Include(d => d.BusinessProofType)
                .Where(d => d.Mid == merchant.Mid && d.DocumentTypeId == documentTypeId)
                .OrderByDescending(d => d.CreatedDate)
                .Select(d => new DocumentResponse
                {
                    DocumentUploadId = d.DocumentUploadId,
                    Mid = d.Mid,
                    DocumentTypeId = d.DocumentTypeId,
                    DocumentTypeName = d.DocumentType.DocumentName,
                    DocumentTypeCode = d.DocumentType.DocumentCode,
                    BusinessProofTypeId = d.BusinessProofTypeId,
                    BusinessProofTypeName = d.BusinessProofType.ProofName,
                    DocumentFileName = d.DocumentFileName,
                    DocumentFilePath = d.DocumentFilePath,
                    DocumentSizeBytes = d.DocumentSizeBytes,
                    DocumentMimeType = d.DocumentMimeType,
                    IsVerified = d.IsVerified,
                    VerifiedBy = d.VerifiedBy,
                    VerifiedDate = d.VerifiedDate,
                    RejectionReason = d.RejectionReason,
                    CreatedDate = d.CreatedDate,
                    UpdatedDate = d.UpdatedDate
                })
                .ToListAsync();

            return new DocumentListResponse
            {
                Documents = documents,
                TotalCount = documents.Count
            };
        }

        public async Task<(byte[] fileData, string fileName, string mimeType)> DownloadDocumentAsync(int userId, int documentUploadId)
        {
            var merchant = await _context.Merchants
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null)
                throw new InvalidOperationException("User or merchant not found. Please ensure you are logged in.");

            var documentUpload = await _context.DocumentUploads
                .FirstOrDefaultAsync(d => d.DocumentUploadId == documentUploadId && d.Mid == merchant.Mid);

            if (documentUpload == null)
                throw new ArgumentException("Document not found or you don't have permission to download it.");

            if (!File.Exists(documentUpload.DocumentFilePath))
                throw new FileNotFoundException("Document file not found on server.");

            var fileData = await File.ReadAllBytesAsync(documentUpload.DocumentFilePath);

            return (fileData, documentUpload.DocumentFileName, documentUpload.DocumentMimeType ?? "application/octet-stream");
        }

        public async Task<List<DocumentTypeResponse>> GetDocumentTypesAsync()
        {
            return await _context.DocumentTypes
                .Where(dt => dt.IsActive == true)
                .OrderBy(dt => dt.DocumentName)
                .Select(dt => new DocumentTypeResponse
                {
                    DocumentTypeId = dt.DocumentTypeId,
                    DocumentName = dt.DocumentName,
                    DocumentCode = dt.DocumentCode,
                    AllowedExtensions = dt.AllowedExtensions,
                    MaxFileSizeMb = dt.MaxFileSizeMb,
                    IsRequired = dt.IsRequired,
                    IsActive = dt.IsActive
                })
                .ToListAsync();
        }

        public async Task<List<BusinessProofTypeResponse>> GetBusinessProofTypesAsync()
        {
            return await _context.BusinessProofTypes
                .Where(bpt => bpt.IsActive == true)
                .OrderBy(bpt => bpt.ProofName)
                .Select(bpt => new BusinessProofTypeResponse
                {
                    BusinessProofTypeId = bpt.BusinessProofTypeId,
                    ProofName = bpt.ProofName,
                    ProofCode = bpt.ProofCode,
                    Description = bpt.Description,
                    IsActive = bpt.IsActive
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteDocumentAsync(int userId, int documentUploadId)
        {
            var merchant = await _context.Merchants
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (merchant == null)
                throw new InvalidOperationException("User or merchant not found. Please ensure you are logged in.");

            var documentUpload = await _context.DocumentUploads
                .FirstOrDefaultAsync(d => d.DocumentUploadId == documentUploadId && d.Mid == merchant.Mid);

            if (documentUpload == null)
                return false;

            // Delete file from disk
            if (File.Exists(documentUpload.DocumentFilePath))
            {
                File.Delete(documentUpload.DocumentFilePath);
            }

            _context.DocumentUploads.Remove(documentUpload);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Document deleted successfully for userId: {UserId}, mid: {Mid}, documentUploadId: {DocumentUploadId}",
                userId, merchant.Mid, documentUploadId);

            return true;
        }

        private async Task<OnboardingStatusDto> BuildOnboardingStatusAsync(int mid)
        {
            var stepOrder = new[]
            {
                new { StepNumber = 1, StepName = "PAN Verification", StepKey = "PAN_VERIFICATION" },
                new { StepNumber = 2, StepName = "Business Entity", StepKey = "BUSINESS_ENTITY" },
                new { StepNumber = 3, StepName = "Phone CKYC", StepKey = "PHONE_CKYC" },
                new { StepNumber = 4, StepName = "Business Category", StepKey = "BUSINESS_CATEGORY" },
                new { StepNumber = 5, StepName = "Share Business Details", StepKey = "SHARE_BUSINESS_DETAILS" },
                new { StepNumber = 6, StepName = "Connect Platform", StepKey = "CONNECT_PLATFORM" },
                new { StepNumber = 7, StepName = "Upload Documents", StepKey = "UPLOAD_DOCUMENTS" },
                new { StepNumber = 8, StepName = "Service Agreement", StepKey = "SERVICE_AGREEMENT" }
            };

            var completedSteps = await _context.OnboardingStepTrackings
                .Where(s => s.Mid == mid && s.StepStatus == "COMPLETED")
                .Select(s => s.StepName)
                .ToListAsync();

            int currentStepIndex = 1;
            string currentStepName = "PAN Verification";
            bool allCompleted = true;

            foreach (var step in stepOrder)
            {
                if (!completedSteps.Contains(step.StepName))
                {
                    currentStepIndex = step.StepNumber;
                    currentStepName = step.StepName;
                    allCompleted = false;
                    break;
                }
            }

            if (allCompleted)
            {
                currentStepIndex = 9;
                currentStepName = "Completed";
            }

            var steps = stepOrder.Select(step => new OnboardingStepDto
            {
                StepNumber = step.StepNumber,
                StepName = step.StepName,
                StepKey = step.StepKey,
                IsCompleted = completedSteps.Contains(step.StepName),
                IsActive = step.StepName == currentStepName
            }).ToList();

            return new OnboardingStatusDto
            {
                StepNumber = currentStepIndex,
                StepName = currentStepName,
                IsCompleted = allCompleted,
                Steps = steps
            };
        }
    }
}
