using System.Collections.Concurrent;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.API.Middleware
{
    /// <summary>
    /// Production-ready rate limiting middleware with token bucket algorithm
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly ConcurrentDictionary<string, RateLimitBucket> _buckets = new();
        private readonly Timer _cleanupTimer;

        // Rate limiting configuration
        private const int PermitsPerSecond = 10;
        private const int BurstLimit = 20;
        private const int WindowSeconds = 60;
        private static readonly TimeSpan BucketExpiry = TimeSpan.FromMinutes(5);

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            
            // Cleanup expired buckets every minute
            _cleanupTimer = new Timer(CleanupExpiredBuckets, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientIdentifier(context);
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Skip rate limiting for health checks and Swagger
            if (path.Contains("/health") || path.Contains("/swagger") || path.Contains("/api-docs"))
            {
                await _next(context);
                return;
            }

            // Special limits for sensitive endpoints
            var (permitsPerSecond, burstLimit, windowSeconds) = GetRateLimitsForPath(path);

            var bucket = _buckets.GetOrAdd(clientId, _ => new RateLimitBucket
            {
                Tokens = burstLimit,
                LastRefill = DateTime.UtcNow,
                MaxTokens = burstLimit,
                RefillRate = permitsPerSecond
            });

            lock (bucket)
            {
                RefillTokens(bucket, burstLimit, permitsPerSecond);

                if (bucket.Tokens < 1)
                {
                    _logger.LogWarning("Rate limit exceeded for client: {ClientId}, path: {Path}", clientId, path);
                    
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers.Append("Retry-After", GetRetryAfterSeconds(bucket).ToString());
                    context.Response.Headers.Append("X-RateLimit-Limit", burstLimit.ToString());
                    context.Response.Headers.Append("X-RateLimit-Remaining", "0");
                    context.Response.Headers.Append("X-RateLimit-Reset", 
                        DateTimeOffset.UtcNow.AddSeconds(GetRetryAfterSeconds(bucket)).ToUnixTimeSeconds().ToString());

                    var response = new ApiResponse
                    {
                        Success = false,
                        Message = $"Rate limit exceeded. Please try again in {GetRetryAfterSeconds(bucket)} seconds."
                    };

                    context.Response.WriteAsJsonAsync(response).Wait();
                    return;
                }

                bucket.Tokens--;
                bucket.LastRequest = DateTime.UtcNow;

                // Add rate limit headers
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers.Append("X-RateLimit-Limit", burstLimit.ToString());
                    context.Response.Headers.Append("X-RateLimit-Remaining", ((int)bucket.Tokens).ToString());
                    return Task.CompletedTask;
                });
            }

            await _next(context);
        }

        private static (int permitsPerSecond, int burstLimit, int windowSeconds) GetRateLimitsForPath(string path)
        {
            // Stricter limits for authentication endpoints
            if (path.Contains("/login") || path.Contains("/register") || path.Contains("/otp") || path.Contains("/initiate"))
            {
                return (2, 5, 300); // 2 per second, burst of 5, 5-minute window
            }

            if (path.Contains("/api/auth") || path.Contains("/api/registration"))
            {
                return (5, 10, 120); // 5 per second, burst of 10, 2-minute window
            }

            return (PermitsPerSecond, BurstLimit, WindowSeconds);
        }

        private static void RefillTokens(RateLimitBucket bucket, int maxTokens, double refillRate)
        {
            var now = DateTime.UtcNow;
            var timePassed = (now - bucket.LastRefill).TotalSeconds;
            var tokensToAdd = timePassed * refillRate;

            bucket.Tokens = Math.Min(bucket.Tokens + tokensToAdd, maxTokens);
            bucket.LastRefill = now;
        }

        private static int GetRetryAfterSeconds(RateLimitBucket bucket)
        {
            var tokensNeeded = 1 - bucket.Tokens;
            var secondsNeeded = (int)Math.Ceiling(tokensNeeded / bucket.RefillRate);
            return Math.Max(secondsNeeded, 1);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Priority: User ID > API Key > IP Address
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                    return $"user:{userId}";
            }

            // Check for API key
            if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
            {
                return $"api:{apiKey}";
            }

            // Fall back to IP address
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                var forwardedIp = forwardedFor.ToString().Split(',')[0].Trim();
                if (!string.IsNullOrEmpty(forwardedIp))
                    ip = forwardedIp;
            }

            return $"ip:{ip}";
        }

        private void CleanupExpiredBuckets(object? state)
        {
            var cutoff = DateTime.UtcNow - BucketExpiry;
            var expired = _buckets.Where(b => b.Value.LastRequest < cutoff).Select(b => b.Key).ToList();
            
            foreach (var key in expired)
            {
                _buckets.TryRemove(key, out _);
            }

            if (expired.Count > 0)
            {
                _logger.LogDebug("Cleaned up {Count} expired rate limit buckets", expired.Count);
            }
        }
    }

    public class RateLimitBucket
    {
        public double Tokens { get; set; }
        public DateTime LastRefill { get; set; }
        public DateTime LastRequest { get; set; }
        public double MaxTokens { get; set; }
        public double RefillRate { get; set; }
    }

    /// <summary>
    /// Extension method to add rate limiting middleware
    /// </summary>
    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
