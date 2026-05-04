using System.Net;
using System.Text.Json;
using BankUPG.SharedKernal.Responses;

namespace BankUPG.API.Middleware
{
    /// <summary>
    /// Global exception handling middleware for production-ready error responses
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next, 
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ApiResponse();
            int statusCode;

            switch (exception)
            {
                case ArgumentException:
                    statusCode = StatusCodes.Status400BadRequest;
                    response.Message = exception.Message;
                    _logger.LogWarning("Bad Request: {Message}", exception.Message);
                    break;

                case InvalidOperationException:
                    statusCode = StatusCodes.Status409Conflict;
                    response.Message = exception.Message;
                    _logger.LogWarning("Conflict: {Message}", exception.Message);
                    break;

                case UnauthorizedAccessException:
                    statusCode = StatusCodes.Status401Unauthorized;
                    response.Message = "Unauthorized access";
                    _logger.LogWarning("Unauthorized access attempt");
                    break;

                case KeyNotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    response.Message = "Resource not found";
                    _logger.LogWarning("Resource not found: {Message}", exception.Message);
                    break;

                case TimeoutException:
                    statusCode = StatusCodes.Status408RequestTimeout;
                    response.Message = "Request timeout. Please try again.";
                    _logger.LogError("Request timeout: {Message}", exception.Message);
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    response.Message = _environment.IsDevelopment() 
                        ? exception.Message 
                        : "An unexpected error occurred. Please try again later.";
                    _logger.LogError(exception, "Unhandled exception occurred");
                    break;
            }

            // Include stack trace in development
            if (_environment.IsDevelopment() && statusCode == StatusCodes.Status500InternalServerError)
            {
                response.Errors = new List<string>
                {
                    exception.StackTrace ?? "No stack trace available"
                };
            }

            response.Success = false;
            context.Response.StatusCode = statusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }

    /// <summary>
    /// Extension method to add exception handling middleware
    /// </summary>
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
