using BankUPG.API.Middleware;
using BankUPG.Application.Interfaces.Admin;
using BankUPG.Application.Interfaces.Auth;
using BankUPG.Application.Interfaces.BankAccountDetail;
using BankUPG.Application.Interfaces.BusinessAddress;
using BankUPG.Application.Interfaces.BusinessCategory;
using BankUPG.Application.Interfaces.BusinessDetails;
using BankUPG.Application.Interfaces.BusinessEntity;
using BankUPG.Application.Interfaces.BusinessEntityTypeMaster;
using BankUPG.Application.Interfaces.BusinessEntityTypeMaster;
using BankUPG.Application.Interfaces.BusinessProofTypeMaster;
using BankUPG.Application.Interfaces.Cache;
using BankUPG.Application.Interfaces.ConnectPlatform;
using BankUPG.Application.Interfaces.Document;
using BankUPG.Application.Interfaces.DocumentMaster;
using BankUPG.Application.Interfaces.DocumentTypeMaster;
using BankUPG.Application.Interfaces.MerchantMaster;
using BankUPG.Application.Interfaces.PEPStatusMaster;
using BankUPG.Application.Interfaces.PEPStatusMaster;
using BankUPG.Application.Interfaces.PhoneCkyc;
using BankUPG.Application.Interfaces.Registration;
using BankUPG.Application.Interfaces.ServiceAgreement;
using BankUPG.Application.Interfaces.SigningAuthorityDetail;
using BankUPG.Application.Interfaces.StatusTracker;
using BankUPG.Application.Interfaces.Verification;
using BankUPG.Application.Services;
using BankUPG.Application.Services.Admin;
using BankUPG.Application.Services.Auth;
using BankUPG.Application.Services.Auth;
using BankUPG.Application.Services.BankAccountDetail;
using BankUPG.Application.Services.BusinessAddress;
using BankUPG.Application.Services.BusinessCategory;
using BankUPG.Application.Services.BusinessDetails;
using BankUPG.Application.Services.BusinessEntity;
using BankUPG.Application.Services.BusinessEntityTypeMaster;
using BankUPG.Application.Services.BusinessEntityTypeMaster;
using BankUPG.Application.Services.BusinessProofTypeMaster;
using BankUPG.Application.Services.Cache;
using BankUPG.Application.Services.ConnectPlatform;
using BankUPG.Application.Services.Document;
using BankUPG.Application.Services.DocumentMaster;
using BankUPG.Application.Services.DocumentTypeMaster;
using BankUPG.Application.Services.MerchantMaster;
using BankUPG.Application.Services.PEPStatusMaster;
using BankUPG.Application.Services.PEPStatusMaster;
using BankUPG.Application.Services.PhoneCkyc;
using BankUPG.Application.Services.Registration;
using BankUPG.Application.Services.ServiceAgreement;
using BankUPG.Application.Services.SigningAuthorityDetail;
using BankUPG.Application.Services.StatusTracker;
using BankUPG.Application.Services.Verification;
using BankUPG.Infrastructure.Data;
using BankUPG.SharedKernal.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using System.Text;
using BankUPG.Application.Interfaces.MerchantMaster;
using BankUPG.Application.Services.MerchantMaster;
using BankUPG.Application.Interfaces.UserMaster;
using BankUPG.Application.Services.UserMaster;
using BankUPG.Application.Interfaces.BusinessSubCategoryMaster;
using BankUPG.Application.Services.BusinessSubCategoryMaster;
using BankUPG.Application.Interfaces.BusinessCategoryMaster;
using BankUPG.Application.Services.BusinessCategoryMaster;
using BankUPG.Application.Interfaces.PaymentOrder;
using BankUPG.Application.Services.PaymentOrder;
using BankUPG.Application.Interfaces.PaymentLink;
using BankUPG.Application.Services.PaymentLink;
using BankUPG.Application.Interfaces.PaymentLinkBulkUpload;
using BankUPG.Application.Services.PaymentLinkBulkUpload;
using BankUPG.Application.Interfaces.PayoutBeneficiary;
using BankUPG.Application.Services.PayoutBeneficiary;
using BankUPG.Application.Interfaces.Payout;
using BankUPG.Application.Services.Payout;
using BankUPG.Application.Interfaces.BatchRefund;
using BankUPG.Application.Services.BatchRefund;
using BankUPG.Application.Interfaces.Invoice;
using BankUPG.Application.Services.Invoice;
using BankUPG.Application.Interfaces.SubscriptionPlan;
using BankUPG.Application.Services.SubscriptionPlan;
using BankUPG.Application.Interfaces.Subscription;
using BankUPG.Application.Services.Subscription;
using BankUPG.Application.Interfaces.EmiPlan;
using BankUPG.Application.Services.EmiPlan;
using BankUPG.Application.Interfaces.Wallet;
using BankUPG.Application.Services.Wallet;
using BankUPG.Application.Interfaces.Dashboard;
using BankUPG.Application.Services.Dashboard;
using BankUPG.Application.Interfaces.Transaction;
using BankUPG.Application.Services.Transaction;
using BankUPG.Application.Interfaces.Settlement;
using BankUPG.Application.Services.Settlement;
using BankUPG.Application.Interfaces.Chargeback;
using BankUPG.Application.Services.Chargeback;
using BankUPG.Application.Interfaces.Refund;
using BankUPG.Application.Services.Refund;
using BankUPG.Application.Interfaces.TransactionCharge;
using BankUPG.Application.Services.TransactionCharge;
using BankUPG.Application.Interfaces.PaymentMethodCharge;
using BankUPG.Application.Services.PaymentMethodCharge;
using BankUPG.Application.Interfaces.MerchantApiKey;
using BankUPG.Application.Services.MerchantApiKey;
using BankUPG.Application.Interfaces.CheckoutCustomization;
using BankUPG.Application.Services.CheckoutCustomization;
using BankUPG.Application.Interfaces.Webhook;
using BankUPG.Application.Services.Webhook;
using BankUPG.Application.Interfaces.MerchantPaymentMethod;
using BankUPG.Application.Services.MerchantPaymentMethod;
using BankUPG.Application.Interfaces.MerchantColumnPreference;
using BankUPG.Application.Services.MerchantColumnPreference;
using BankUPG.Application.Interfaces.IpWhitelist;
using BankUPG.Application.Services.IpWhitelist;

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
builder.Services.AddScoped<IBusinessEntityService, BusinessEntityService>();
builder.Services.AddScoped<IPhoneCkycService, PhoneCkycService>();
builder.Services.AddScoped<IBusinessCategoryService, BusinessCategoryService>();
builder.Services.AddScoped<IBusinessDetailsService, BusinessDetailsService>();
builder.Services.AddScoped<IConnectPlatformService, ConnectPlatformService>();
builder.Services.AddScoped<ISigningAuthorityDetailService, SigningAuthorityDetailService>();
builder.Services.AddScoped<IDocumentMasterService, DocumentMasterService>();
builder.Services.AddScoped<IBusinessAddressService, BusinessAddressService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

builder.Services.AddScoped<IBusinessProofTypeMasterService, BusinessProofTypeMasterService>();
builder.Services.AddScoped<IServiceAgreementService, ServiceAgreementService>();
builder.Services.AddScoped<IBankAccountDetailService, BankAccountDetailService>();
builder.Services.AddScoped<IStatusTrackerService, StatusTrackerService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddSingleton<ITokenBlocklistService, TokenBlocklistService>();
builder.Services.AddScoped<IBusinessEntityTypeMasterService, BusinessEntityTypeMasterService>();
builder.Services.AddScoped<IDocumentTypeMasterService, DocumentTypeMasterService>();

builder.Services.AddScoped<IPEPStatusMasterService, PEPStatusMasterService>();
builder.Services.AddScoped<IMerchantMasterService, MerchantMasterService>();
builder.Services.AddScoped<IUserMasterService, UserMasterService>();
builder.Services.AddScoped<IBusinessSubCategoryMasterService, BusinessSubCategoryMasterService>();
builder.Services.AddScoped<IBusinessCategoryMasterService, BusinessCategoryMasterService>();

// Payment Gateway Services
builder.Services.AddScoped<IPaymentOrderService, PaymentOrderService>();
builder.Services.AddScoped<IPaymentLinkService, PaymentLinkService>();
builder.Services.AddScoped<IPaymentLinkBulkUploadService, PaymentLinkBulkUploadService>();
builder.Services.AddScoped<IPayoutBeneficiaryService, PayoutBeneficiaryService>();
builder.Services.AddScoped<IPayoutService, PayoutService>();
builder.Services.AddScoped<IBatchRefundService, BatchRefundService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IEmiPlanService, EmiPlanService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ISettlementService, SettlementService>();
builder.Services.AddScoped<IChargebackService, ChargebackService>();
builder.Services.AddScoped<IRefundService, RefundService>();
builder.Services.AddScoped<ITransactionChargeService, TransactionChargeService>();
builder.Services.AddScoped<IPaymentMethodChargeService, PaymentMethodChargeService>();
builder.Services.AddScoped<IMerchantApiKeyService, MerchantApiKeyService>();
builder.Services.AddScoped<ICheckoutCustomizationService, CheckoutCustomizationService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddScoped<IMerchantPaymentMethodService, MerchantPaymentMethodService>();
builder.Services.AddScoped<IMerchantColumnPreferenceService, MerchantColumnPreferenceService>();
builder.Services.AddScoped<IIpWhitelistService, IpWhitelistService>();

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
                "https://paymentgateway.banku.co.in",            // Production domain
                "https://www.paymentgateway.banku.co.in",            // Production domain
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
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var blocklist = context.HttpContext.RequestServices
                .GetRequiredService<ITokenBlocklistService>();
            var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (!string.IsNullOrEmpty(jti) && blocklist.IsBlocklisted(jti))
            {
                context.Fail("Token has been revoked");
            }
            return Task.CompletedTask;
        }
    };
});

// Add Authorization
builder.Services.AddAuthorization();

// SuperAdmin bypass: any authenticated user with the SuperAdmin role satisfies every requirement
builder.Services.AddSingleton<IAuthorizationHandler, BankUPG.API.Authorization.SuperAdminAuthorizationHandler>();

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

// IP Whitelist enforcement — runs after auth so merchant MID is available
app.UseIpWhitelist();

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
