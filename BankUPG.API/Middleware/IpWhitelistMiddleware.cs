using BankUPG.Application.Interfaces.IpWhitelist;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace BankUPG.API.Middleware
{
    public class IpWhitelistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IpWhitelistMiddleware> _logger;

        private static readonly HashSet<string> _exemptPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/health", "/swagger", "/", "/api/auth/login", "/api/auth/register",
            "/api/auth/send-otp", "/api/auth/verify-otp"
        };

        public IpWhitelistMiddleware(RequestDelegate next, ILogger<IpWhitelistMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IIpWhitelistService ipWhitelistService)
        {
            var path = context.Request.Path.Value ?? "";

            if (IsExempt(path))
            {
                await _next(context);
                return;
            }

            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            var midClaim = context.User.FindFirst("MID") ??
                           context.User.FindFirst(ClaimTypes.NameIdentifier);

            if (midClaim == null || !int.TryParse(midClaim.Value, out int mid))
            {
                await _next(context);
                return;
            }

            var requestIp = GetClientIp(context);

            if (!string.IsNullOrEmpty(requestIp))
            {
                var allowed = await ipWhitelistService.IsIpAllowedAsync(mid, requestIp);
                if (!allowed)
                {
                    _logger.LogWarning("IP whitelist BLOCKED: MID={Mid}, IP={IP}, Path={Path}", mid, requestIp, path);

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    var response = JsonSerializer.Serialize(new
                    {
                        success = false,
                        message = $"Access denied: your IP address ({requestIp}) is not whitelisted.",
                        timestamp = DateTime.UtcNow
                    });
                    await context.Response.WriteAsync(response);
                    return;
                }
            }

            await _next(context);
        }

        private static bool IsExempt(string path)
        {
            if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)) return true;
            return _exemptPaths.Contains(path);
        }

        private static string? GetClientIp(HttpContext context)
        {
            var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
                return forwarded.Split(',')[0].Trim();

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp)) return realIp;

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }

    public static class IpWhitelistMiddlewareExtensions
    {
        public static IApplicationBuilder UseIpWhitelist(this IApplicationBuilder app) =>
            app.UseMiddleware<IpWhitelistMiddleware>();
    }
}
