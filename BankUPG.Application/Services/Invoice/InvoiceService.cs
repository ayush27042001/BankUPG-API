using BankUPG.Application.Interfaces.Invoice;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankUPG.Application.Services.Invoice
{
    public class InvoiceService : IInvoiceService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(AppDBContext context, ILogger<InvoiceService> logger)
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

        public async Task<InvoiceResponse> CreateInvoiceAsync(int userId, CreateInvoiceRequest request)
        {
            var mid = await GetMidAsync(userId);

            var subTotal = request.Items.Sum(i => i.Quantity * i.UnitPrice);
            var totalAmount = subTotal + (request.TaxAmount ?? 0);

            var count = await _context.Invoices.CountAsync(i => i.Mid == mid);
            var invoiceNumber = $"INV-{mid}-{(count + 1):D6}";

            var invoice = new Infrastructure.Entities.Invoice
            {
                Mid = mid,
                InvoiceNumber = invoiceNumber,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                SubTotal = subTotal,
                TaxAmount = request.TaxAmount,
                TotalAmount = totalAmount,
                Status = "draft",
                DueDate = request.DueDate,
                Notes = request.Notes,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            var items = request.Items.Select(i => new Infrastructure.Entities.InvoiceItem
            {
                InvoiceId = invoice.InvoiceId,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Amount = i.Quantity * i.UnitPrice
            }).ToList();

            _context.InvoiceItems.AddRange(items);
            await _context.SaveChangesAsync();

            invoice.InvoiceItems = items;
            _logger.LogInformation("Invoice {InvoiceNumber} created for MID {Mid}", invoiceNumber, mid);
            return MapToResponse(invoice, includeItems: true);
        }

        public async Task<InvoiceResponse?> GetInvoiceAsync(int userId, long invoiceId)
        {
            var mid = await GetMidAsync(userId);
            var invoice = await _context.Invoices.AsNoTracking()
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && i.Mid == mid);
            return invoice == null ? null : MapToResponse(invoice, includeItems: true);
        }

        public async Task<PagedResponse<InvoiceResponse>> ListInvoicesAsync(int userId, ListInvoicesRequest request)
        {
            var mid = await GetMidAsync(userId);

            var query = _context.Invoices.AsNoTracking().Where(i => i.Mid == mid);

            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(i => i.Status == request.Status);
            if (request.FromDate.HasValue)
                query = query.Where(i => i.CreatedDate >= request.FromDate);
            if (request.ToDate.HasValue)
                query = query.Where(i => i.CreatedDate <= request.ToDate);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(i => i.CreatedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(i => MapToResponse(i, false))
                .ToListAsync();

            return new PagedResponse<InvoiceResponse>
            {
                Items = items,
                TotalCount = total,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<bool> SendInvoiceAsync(int userId, long invoiceId)
        {
            var mid = await GetMidAsync(userId);
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && i.Mid == mid);

            if (invoice == null || invoice.Status == "cancelled" || invoice.Status == "paid") return false;

            invoice.Status = "sent";
            invoice.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelInvoiceAsync(int userId, long invoiceId)
        {
            var mid = await GetMidAsync(userId);
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && i.Mid == mid);

            if (invoice == null || invoice.Status == "paid") return false;

            invoice.Status = "cancelled";
            invoice.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private static InvoiceResponse MapToResponse(Infrastructure.Entities.Invoice i, bool includeItems = false) => new()
        {
            InvoiceId = i.InvoiceId,
            Mid = i.Mid,
            InvoiceNumber = i.InvoiceNumber,
            CustomerName = i.CustomerName,
            CustomerEmail = i.CustomerEmail,
            CustomerPhone = i.CustomerPhone,
            SubTotal = i.SubTotal,
            TaxAmount = i.TaxAmount,
            TotalAmount = i.TotalAmount,
            Status = i.Status,
            DueDate = i.DueDate,
            PaidDate = i.PaidDate,
            Notes = i.Notes,
            CreatedDate = i.CreatedDate,
            UpdatedDate = i.UpdatedDate,
            Items = includeItems ? i.InvoiceItems?.Select(item => new InvoiceItemResponse
            {
                InvoiceItemId = item.InvoiceItemId,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Amount = item.Amount
            }).ToList() : null
        };
    }
}
