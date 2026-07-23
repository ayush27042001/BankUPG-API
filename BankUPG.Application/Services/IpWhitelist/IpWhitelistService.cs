using BankUPG.Application.Interfaces.IpWhitelist;
using BankUPG.Infrastructure.Data;
using BankUPG.Infrastructure.Entities;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace BankUPG.Application.Services.IpWhitelist
{
    public class IpWhitelistService : IIpWhitelistService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<IpWhitelistService> _logger;

        public IpWhitelistService(AppDBContext context, ILogger<IpWhitelistService> logger)
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

        public async Task<IpWhitelistStatusResponse> GetStatusAsync(int userId)
        {
            var mid = await GetMidAsync(userId);
            var merchant = await _context.Merchants.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Mid == mid);

            var ips = await _context.MerchantIpWhitelists.AsNoTracking()
                .Where(ip => ip.Mid == mid && ip.IsActive)
                .OrderBy(ip => ip.CreatedDate)
                .Select(ip => MapToResponse(ip))
                .ToListAsync();

            return new IpWhitelistStatusResponse
            {
                IpWhitelistEnabled = merchant?.IpWhitelistEnabled ?? false,
                TotalIps = ips.Count,
                IpAddresses = ips
            };
        }

        public async Task<IpWhitelistResponse> AddIpAsync(int userId, AddIpWhitelistRequest request)
        {
            var mid = await GetMidAsync(userId);

            var exists = await _context.MerchantIpWhitelists
                .AnyAsync(ip => ip.Mid == mid && ip.IpAddress == request.IpAddress && ip.IsActive);
            if (exists)
                throw new ArgumentException($"IP address {request.IpAddress} is already whitelisted.");

            var ip = new MerchantIpWhitelist
            {
                Mid = mid,
                IpAddress = request.IpAddress,
                Description = request.Description,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.MerchantIpWhitelists.Add(ip);
            await _context.SaveChangesAsync();

            _logger.LogInformation("IP {IP} whitelisted for MID {Mid}", request.IpAddress, mid);
            return MapToResponse(ip);
        }

        public async Task<bool> RemoveIpAsync(int userId, int ipWhitelistId)
        {
            var mid = await GetMidAsync(userId);
            var ip = await _context.MerchantIpWhitelists
                .FirstOrDefaultAsync(ip => ip.IpWhitelistId == ipWhitelistId && ip.Mid == mid);

            if (ip == null) return false;

            ip.IsActive = false;
            ip.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("IP {IP} removed from whitelist for MID {Mid}", ip.IpAddress, mid);
            return true;
        }

        public async Task<bool> ToggleWhitelistAsync(int userId, bool enabled)
        {
            var mid = await GetMidAsync(userId);
            var merchant = await _context.Merchants.FirstOrDefaultAsync(m => m.Mid == mid);

            if (merchant == null) return false;

            if (enabled)
            {
                var hasIps = await _context.MerchantIpWhitelists
                    .AnyAsync(ip => ip.Mid == mid && ip.IsActive);
                if (!hasIps)
                    throw new InvalidOperationException("Add at least one IP address before enabling IP whitelist.");
            }

            merchant.IpWhitelistEnabled = enabled;
            merchant.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("IP whitelist {Status} for MID {Mid}", enabled ? "ENABLED" : "DISABLED", mid);
            return true;
        }

        public async Task<bool> IsIpAllowedAsync(int mid, string ipAddress)
        {
            var merchant = await _context.Merchants.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Mid == mid);

            if (merchant == null || !merchant.IpWhitelistEnabled)
                return true;

            var allowedIps = await _context.MerchantIpWhitelists.AsNoTracking()
                .Where(ip => ip.Mid == mid && ip.IsActive)
                .Select(ip => ip.IpAddress)
                .ToListAsync();

            return IsIpInList(ipAddress, allowedIps);
        }

        private static bool IsIpInList(string requestIp, List<string> allowedIps)
        {
            if (!IPAddress.TryParse(requestIp, out var clientIp)) return false;

            foreach (var entry in allowedIps)
            {
                if (entry.Contains('/'))
                {
                    var parts = entry.Split('/');
                    if (IPAddress.TryParse(parts[0], out var networkIp) && int.TryParse(parts[1], out var prefixLen))
                    {
                        if (IsInCidrRange(clientIp, networkIp, prefixLen)) return true;
                    }
                }
                else if (IPAddress.TryParse(entry, out var whitelistedIp) && clientIp.Equals(whitelistedIp))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsInCidrRange(IPAddress ip, IPAddress networkIp, int prefixLen)
        {
            var ipBytes = ip.GetAddressBytes();
            var networkBytes = networkIp.GetAddressBytes();
            if (ipBytes.Length != networkBytes.Length) return false;

            int fullBytes = prefixLen / 8;
            int remainder = prefixLen % 8;

            for (int i = 0; i < fullBytes; i++)
                if (ipBytes[i] != networkBytes[i]) return false;

            if (remainder > 0)
            {
                int mask = 0xFF << (8 - remainder) & 0xFF;
                if ((ipBytes[fullBytes] & mask) != (networkBytes[fullBytes] & mask)) return false;
            }
            return true;
        }

        private static IpWhitelistResponse MapToResponse(MerchantIpWhitelist ip) => new()
        {
            IpWhitelistId = ip.IpWhitelistId,
            Mid = ip.Mid,
            IpAddress = ip.IpAddress,
            Description = ip.Description,
            IsActive = ip.IsActive,
            CreatedDate = ip.CreatedDate,
            UpdatedDate = ip.UpdatedDate
        };
    }
}
