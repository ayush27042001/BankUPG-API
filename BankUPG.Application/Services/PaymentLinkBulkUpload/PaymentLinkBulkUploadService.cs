using BankUPG.Application.Interfaces.PaymentLinkBulkUpload;
using BankUPG.Infrastructure.Data;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BankUPG.Application.Services.PaymentLinkBulkUpload
{
    public class PaymentLinkBulkUploadService : IPaymentLinkBulkUploadService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<PaymentLinkBulkUploadService> _logger;

        public PaymentLinkBulkUploadService(AppDBContext context, ILogger<PaymentLinkBulkUploadService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private async Task<int> GetMidAsync(int userId)
        {
            var merchant = await _context.Merchants.AsNoTracking()
                .FirstOrDefaultAsync(m => m.UserId == userId);
            if (merchant == null) throw new InvalidOperationException("Merchant not found.");
            return merchant.Mid;
        }

        public async Task<PaymentLinkBulkUploadResponse> CreateAsync(int userId, CreatePaymentLinkBulkUploadRequest request)
        {
            var mid = await GetMidAsync(userId);
            var merchant = await _context.Merchants.AsNoTracking().FirstAsync(m => m.Mid == mid);

            var upload = new Infrastructure.Entities.PaymentLinkBulkUpload
            {
                Mid = mid,
                BatchReferenceId = request.BatchReferenceId ?? $"BULK-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                CreatorEmail = merchant?.User?.Email ?? "unknown",
                BatchDescription = request.BatchDescription,
                FileName = request.FileName,
                LinkCreated = 0,
                ActiveCount = 0,
                Status = "Pending",
                SendEmail = request.SendEmail,
                SendSms = request.SendSms,
                CustomerDataCapture = request.CustomerDataCapture != null && request.CustomerDataCapture.Any()
                    ? JsonSerializer.Serialize(request.CustomerDataCapture)
                    : null,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.PaymentLinkBulkUploads.Add(upload);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Bulk upload {BatchReferenceId} created for MID {Mid}", upload.BatchReferenceId, mid);
            return MapToResponse(upload);
        }

        public async Task<PagedResponse<PaymentLinkBulkUploadResponse>> ListAsync(int userId, ListPaymentLinkBulkUploadsRequest request)
        {
            var mid = await GetMidAsync(userId);
            var (from, to) = ResolveDateFilter(request.DateFilter, request.FromDate, request.ToDate);

            var query = _context.PaymentLinkBulkUploads.AsNoTracking().Where(b => b.Mid == mid);

            if (from.HasValue)
                query = query.Where(b => b.CreatedDate >= from);
            if (to.HasValue)
                query = query.Where(b => b.CreatedDate <= to);
            if (!string.IsNullOrEmpty(request.Status) && request.Status.ToLower() != "all")
                query = query.Where(b => b.Status == request.Status);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(b => b.CreatedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(b => MapToResponse(b))
                .ToListAsync();

            return new PagedResponse<PaymentLinkBulkUploadResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PaymentLinkBulkUploadResponse?> GetAsync(int userId, long bulkUploadId)
        {
            var mid = await GetMidAsync(userId);
            var b = await _context.PaymentLinkBulkUploads.AsNoTracking()
                .FirstOrDefaultAsync(x => x.BulkUploadId == bulkUploadId && x.Mid == mid);
            return b == null ? null : MapToResponse(b);
        }

        public async Task<PaymentLinkBulkUploadFileResponse> AddFileAsync(int userId, CreatePaymentLinkBulkUploadFileRequest request)
        {
            var mid = await GetMidAsync(userId);

            var file = new Infrastructure.Entities.PaymentLinkBulkUploadFile
            {
                Mid = mid,
                FileName = request.FileName,
                FilePath = request.FilePath,
                Status = "Active",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.PaymentLinkBulkUploadFiles.Add(file);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Bulk upload file {FileName} added for MID {Mid}", request.FileName, mid);
            return MapToFileResponse(file);
        }

        public async Task<PagedResponse<PaymentLinkBulkUploadFileResponse>> ListFilesAsync(int userId, int pageNumber, int pageSize)
        {
            var mid = await GetMidAsync(userId);

            var query = _context.PaymentLinkBulkUploadFiles.AsNoTracking().Where(f => f.Mid == mid);
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(f => f.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => MapToFileResponse(f))
                .ToListAsync();

            return new PagedResponse<PaymentLinkBulkUploadFileResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        private static PaymentLinkBulkUploadResponse MapToResponse(Infrastructure.Entities.PaymentLinkBulkUpload b) => new()
        {
            BulkUploadId = b.BulkUploadId,
            BatchReferenceId = b.BatchReferenceId,
            CreatorEmail = b.CreatorEmail,
            BatchDescription = b.BatchDescription,
            LinkCreated = b.LinkCreated,
            ActiveCount = b.ActiveCount,
            Status = b.Status,
            CreatedDate = b.CreatedDate,
            UpdatedDate = b.UpdatedDate
        };

        private static PaymentLinkBulkUploadFileResponse MapToFileResponse(Infrastructure.Entities.PaymentLinkBulkUploadFile f) => new()
        {
            FileId = f.FileId,
            FileName = f.FileName,
            Status = f.Status,
            CreatedDate = f.CreatedDate
        };

        private static (DateTime? From, DateTime? To) ResolveDateFilter(string? dateFilter, DateTime? fromDate, DateTime? toDate)
        {
            var now = DateTime.UtcNow;
            return dateFilter?.ToLower() switch
            {
                "today" => (now.Date, now.Date.AddDays(1).AddTicks(-1)),
                "yesterday" => (now.Date.AddDays(-1), now.Date.AddTicks(-1)),
                "last7days" => (now.Date.AddDays(-7), now),
                "last30days" => (now.Date.AddDays(-30), now),
                "custom" => (fromDate, toDate),
                _ => (fromDate, toDate)
            };
        }
    }
}
