using BankUPG.Infrastructure.Data;
using BankUPG.SharedKernal.Models;
using BankUPG.Application.Services.Auth;
using BankUPG.Application.Services.Cache;
using BankUPG.Application.Services.Registration;
using BankUPG.Application.Services.Verification;
using BankUPG.API.Middleware;
using BankUPG.Application.Interfaces.Cache;
using BankUPG.Application.Interfaces.Registration;
using BankUPG.Application.Interfaces.Verification;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using BankUPG.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure AppSettings
var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
if (appSettings == null)
{
    throw new InvalidOperationException("AppSettings configuration section is missing or invalid.");
}
if (string.IsNullOrEmpty(appSettings.ConnectionString))
{
    throw new InvalidOperationException("AppSettings.ConnectionString is not configured.");
}
if (appSettings.Jwt == null || string.IsNullOrEmpty(appSettings.Jwt.Secret))
{
    throw new InvalidOperationException("AppSettings.Jwt configuration is missing or invalid.");
}
builder.Services.AddSingleton(appSettings);

// Add DbContext
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(appSettings.ConnectionString));

// Register Services
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<OtpService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IPanVerificationService, PanVerificationService>();

// Add HttpClientFactory for external API calls
builder.Services.AddHttpClient();

// Add Memory Cache for high-performance caching
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 100 * 1024 * 1024; // 100 MB cache size limit
    options.CompactionPercentage = 0.25;
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
});

// Add Response Compression for faster API responses
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "application/xml", "text/plain", "image/svg+xml" });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

// Add CORS for Angular and Mobile App integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("BankUPG_CorsPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",      // Angular development
                "https://localhost:4200",     // Angular development HTTPS
                "http://localhost:8100",      // Ionic/Cordova
                "capacitor://localhost",       // Capacitor apps
                "ionic://localhost",           // Ionic apps
                "https://banku.in",           // Production domain
                "https://www.banku.in")       // Production www
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset", "Retry-After");
    });
});

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = appSettings.Jwt.Issuer,
        ValidAudience = appSettings.Jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Jwt.Secret)),
        ClockSkew = TimeSpan.Zero
    };
});

// Add Authorization
builder.Services.AddAuthorization();

// Add Controllers with JSON options optimized for performance
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = false; // Production: minimize payload size
    });

builder.Services.AddEndpointsApiExplorer();

// Add Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BankUPG Merchant Onboarding API",
        Version = "v1",
        Description = "Production-ready API for BankU Merchant Onboarding with JWT Authentication, OTP Verification, and Rate Limiting",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "BankU Support",
            Email = "support@banku.in"
        }
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure HTTP request pipeline
// Enable Swagger in all environments (with authentication in production)
app.UseSwagger(c =>
{
    c.RouteTemplate = "swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BankUPG API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "BankU Merchant API Documentation";
});

// Add Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
    await next();
});

// Enable Response Compression
app.UseResponseCompression();

// Enable CORS
app.UseCors("BankUPG_CorsPolicy");

app.UseHttpsRedirection();

// Add Production-Ready Rate Limiting Middleware
app.UseRateLimiting();

// Add Global Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint
app.MapGet("/health", () => new { 
    Status = "Healthy", 
    Timestamp = DateTimeService.Now,
    Service = "BankUPG API",
    Version = "1.0.0"
});

// Root endpoint
app.MapGet("/", () => new { 
    Message = "BankUPG Merchant Onboarding API",
    Version = "1.0.0",
    Status = "Running",
    Documentation = "/swagger"
});

app.MapControllers();

app.Run();
