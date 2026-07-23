using BankUPG.Application.Interfaces.PaymentLink;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.PaymentLink
{
    public class PaymentLinkService : IPaymentLinkService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<PaymentLinkService> _logger;

        public PaymentLinkService(AppDBContext context, ILogger<PaymentLinkService> logger)
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

        public async Task<PaymentLinkResponse> CreateLinkAsync(int userId, CreatePaymentLinkRequest request)
        {
            var mid = await GetMidAsync(userId);

            DateTime? expiry = request.ExpiryDate;
            if (expiry == null && request.ValidationPeriod.HasValue && !string.IsNullOrEmpty(request.TimeUnit))
            {
                expiry = request.TimeUnit?.ToUpper() switch
                {
                    "D" => DateTime.UtcNow.AddDays(request.ValidationPeriod.Value),
                    "H" => DateTime.UtcNow.AddHours(request.ValidationPeriod.Value),
                    "M" => DateTime.UtcNow.AddMinutes(request.ValidationPeriod.Value),
                    _ => expiry
                };
            }

            var shortCode = GenerateShortCode();
            var link = new Infrastructure.Entities.PaymentLink
            {
                Mid = mid,
                Amount = request.Amount,
                AmountType = request.AmountType ?? "fixed",
                Description = request.Description,
                Purpose = request.Description,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                CustomerName = request.CustomerName,
                ExpiryDate = expiry,
                DueDate = request.DueDate,
                Status = "active",
                PaymentType = request.IsPartialPayment == true ? "PartialPayment" : (request.PaymentType ?? "Standard"),
                IsPartialPayment = request.IsPartialPayment ?? false,
                MaxPaymentsAllowed = request.MaxPaymentsAllowed,
                SendSms = request.SendSms ?? false,
                ShortUrl = $"pay.banku.in/{shortCode}",
                ReferenceId = request.ReferenceId,
                InvoiceId = request.InvoiceId,
                MaxUses = request.MaxUses ?? (request.IsPartialPayment == true ? request.MaxPaymentsAllowed : 1),
                UsedCount = 0,
                TotalViews = 0,
                TotalAmountPaid = 0,
                CustomerDataCapture = SerializeDataCapture(request.CustomerDataCapture),
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.PaymentLinks.Add(link);
            await _context.SaveChangesAsync();

            _logger.LogInformation("PaymentLink {LinkId} created for MID {Mid}", link.PaymentLinkId, mid);
            return MapToResponse(link);
        }

        public async Task<PaymentLinkResponse?> GetLinkAsync(int userId, long linkId)
        {
            var mid = await GetMidAsync(userId);
            var link = await _context.PaymentLinks.AsNoTracking()
                .FirstOrDefaultAsync(l => l.PaymentLinkId == linkId && l.Mid == mid);
            return link == null ? null : MapToResponse(link);
        }

        public async Task<PagedResponse<PaymentLinkResponse>> ListLinksAsync(int userId, ListPaymentLinksRequest request)
        {
            var mid = await GetMidAsync(userId);
            var (from, to) = ResolveDateFilter(request.DateFilter, request.FromDate, request.ToDate);

            var query = _context.PaymentLinks.AsNoTracking().Where(l => l.Mid == mid);

            if (from.HasValue)
                query = query.Where(l => l.CreatedDate >= from);
            if (to.HasValue)
                query = query.Where(l => l.CreatedDate <= to);
            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(l => l.Status == request.Status);
            if (!string.IsNullOrEmpty(request.PaymentType))
                query = query.Where(l => l.PaymentType == request.PaymentType);
            if (!string.IsNullOrEmpty(request.Purpose))
                query = query.Where(l => (l.Purpose != null && l.Purpose.Contains(request.Purpose)) ||
                                          (l.Description != null && l.Description.Contains(request.Purpose)));
            if (!string.IsNullOrEmpty(request.InvoiceId))
                query = query.Where(l => l.InvoiceId != null && l.InvoiceId.Contains(request.InvoiceId));
            if (!string.IsNullOrEmpty(request.ReferenceId))
                query = query.Where(l => l.ReferenceId != null && l.ReferenceId.Contains(request.ReferenceId));
            if (!string.IsNullOrEmpty(request.CustomerName))
                query = query.Where(l => l.CustomerName != null && l.CustomerName.Contains(request.CustomerName));
            if (!string.IsNullOrEmpty(request.CustomerEmail))
                query = query.Where(l => l.CustomerEmail != null && l.CustomerEmail.Contains(request.CustomerEmail));
            if (!string.IsNullOrEmpty(request.CustomerPhone))
                query = query.Where(l => l.CustomerPhone != null && l.CustomerPhone.Contains(request.CustomerPhone));

            query = ApplySorting(query, request.SortBy, request.SortDirection);

            var total = await query.CountAsync();
            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(l => MapToResponse(l))
                .ToListAsync();

            return new PagedResponse<PaymentLinkResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<PaymentLinkSummaryResponse> GetSummaryAsync(int userId)
        {
            var mid = await GetMidAsync(userId);
            var links = await _context.PaymentLinks.AsNoTracking()
                .Where(l => l.Mid == mid)
                .ToListAsync();

            return new PaymentLinkSummaryResponse
            {
                ActivePaymentLinks = links.Count(l => l.Status == "active"),
                RevenueViaPaymentLinks = links.Sum(l => l.TotalAmountPaid),
                TotalViews = links.Sum(l => l.TotalViews),
                TotalLinks = links.Count,
                PaidLinks = links.Count(l => l.Status == "paid")
            };
        }

        public async Task<bool> CancelLinkAsync(int userId, long linkId)
        {
            var mid = await GetMidAsync(userId);
            var link = await _context.PaymentLinks
                .FirstOrDefaultAsync(l => l.PaymentLinkId == linkId && l.Mid == mid);

            if (link == null || link.Status == "paid") return false;

            link.Status = "cancelled";
            link.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private static string GenerateShortCode() =>
            Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("+", "").Replace("/", "").Replace("=", "")[..8];

        private static PaymentLinkResponse MapToResponse(Infrastructure.Entities.PaymentLink l) => new()
        {
            PaymentLinkId = l.PaymentLinkId,
            Mid = l.Mid,
            Amount = l.Amount,
            AmountType = l.AmountType,
            Description = l.Description,
            Purpose = l.Purpose,
            CustomerEmail = l.CustomerEmail,
            CustomerPhone = l.CustomerPhone,
            CustomerName = l.CustomerName,
            ExpiryDate = l.ExpiryDate,
            DueDate = l.DueDate,
            Status = l.Status,
            PaymentType = l.PaymentType,
            IsPartialPayment = l.IsPartialPayment,
            MaxPaymentsAllowed = l.MaxPaymentsAllowed,
            ShortUrl = l.ShortUrl,
            ReferenceId = l.ReferenceId,
            InvoiceId = l.InvoiceId,
            MaxUses = l.MaxUses,
            UsedCount = l.UsedCount,
            TotalViews = l.TotalViews,
            TotalAmountPaid = l.TotalAmountPaid,
            SendSms = l.SendSms,
            CustomerDataCapture = DeserializeDataCapture(l.CustomerDataCapture),
            CreatedDate = l.CreatedDate,
            UpdatedDate = l.UpdatedDate
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

        private static IQueryable<Infrastructure.Entities.PaymentLink> ApplySorting(
            IQueryable<Infrastructure.Entities.PaymentLink> query, string? sortBy, string? sortDirection)
        {
            var desc = string.IsNullOrEmpty(sortDirection) || sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
            return sortBy?.ToLower() switch
            {
                "amount" => desc ? query.OrderByDescending(l => l.Amount) : query.OrderBy(l => l.Amount),
                "status" => desc ? query.OrderByDescending(l => l.Status) : query.OrderBy(l => l.Status),
                "paymenttype" => desc ? query.OrderByDescending(l => l.PaymentType) : query.OrderBy(l => l.PaymentType),
                "purpose" => desc ? query.OrderByDescending(l => l.Purpose) : query.OrderBy(l => l.Purpose),
                "createddate" => desc ? query.OrderByDescending(l => l.CreatedDate) : query.OrderBy(l => l.CreatedDate),
                _ => desc ? query.OrderByDescending(l => l.CreatedDate) : query.OrderBy(l => l.CreatedDate)
            };
        }

        private static string? SerializeDataCapture(List<PaymentLinkDataCaptureFieldRequest>? fields)
        {
            if (fields == null || !fields.Any()) return null;
            return System.Text.Json.JsonSerializer.Serialize(fields.Select(f => new PaymentLinkDataCaptureFieldResponse
            {
                FieldType = f.FieldType,
                Name = f.Name,
                Options = f.Options
            }));
        }

        private static List<PaymentLinkDataCaptureFieldResponse>? DeserializeDataCapture(string? json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<PaymentLinkDataCaptureFieldResponse>>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
