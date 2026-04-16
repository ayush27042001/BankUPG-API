using BankUPG.SharedKernal.Models;
using System.Net;

namespace BankUPG.API.Middleware
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;
        private readonly ILogger<RateLimitMiddleware> _logger;
        private static readonly Dictionary<string, RateLimitCounter> _counters = new();
        private static readonly object _lock = new();

        public RateLimitMiddleware(
            RequestDelegate next,
            AppSettings appSettings,
            ILogger<RateLimitMiddleware> logger)
        {
            _next = next;
            _appSettings = appSettings;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientId(context);
            var path = context.Request.Path.Value ?? string.Empty;

            // Skip rate limiting for health checks and Swagger
            if (path.Contains("/health") || path.Contains("/swagger") || path.Contains("/api-docs"))
            {
                await _next(context);
                return;
            }

            var now = DateTime.UtcNow;
            var windowStart = now.AddSeconds(-_appSettings.RateLimit.WindowInSeconds);

            bool shouldBlock = false;
            lock (_lock)
            {
                if (!_counters.ContainsKey(clientId))
                {
                    _counters[clientId] = new RateLimitCounter();
                }

                var counter = _counters[clientId];

                // Clean up old entries
                counter.RequestTimes.RemoveAll(t => t < windowStart);

                if (counter.RequestTimes.Count >= _appSettings.RateLimit.BurstLimit)
                {
                    _logger.LogWarning($"Rate limit exceeded for client: {clientId}");
                    shouldBlock = true;
                }
                else
                {
                    counter.RequestTimes.Add(now);
                }
            }

            if (shouldBlock)
            {
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Rate limit exceeded. Please try again later."
                });
                return;
            }

            await _next(context);
        }

        private string GetClientId(HttpContext context)
        {
            // Try to get client IP from X-Forwarded-For header first (for load balancers)
            if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return context.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
            }

            // Fall back to remote IP address
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private class RateLimitCounter
        {
            public List<DateTime> RequestTimes { get; set; } = new();
        }
    }
}
