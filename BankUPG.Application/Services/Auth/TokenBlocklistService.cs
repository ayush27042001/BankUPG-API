using BankUPG.Application.Interfaces.Auth;
using Microsoft.Extensions.Caching.Memory;

namespace BankUPG.Application.Services.Auth
{
    public class TokenBlocklistService : ITokenBlocklistService
    {
        private readonly IMemoryCache _cache;
        private const string Prefix = "jti_blocked:";

        public TokenBlocklistService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Blocklist(string jti, DateTime expiry)
        {
            var ttl = expiry - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero) return;

            _cache.Set(Prefix + jti, true, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = expiry,
                Size = 1
            });
        }

        public bool IsBlocklisted(string jti)
        {
            return _cache.TryGetValue(Prefix + jti, out _);
        }
    }
}
