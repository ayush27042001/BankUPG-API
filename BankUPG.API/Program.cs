using BankUPG.Infrastructure.Data;
using BankUPG.SharedKernal.Models;
using BankUPG.SharedKernal.Requests;
using BankUPG.SharedKernal.Responses;
using BankUPG.API.Services;
using BankUPG.API.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BankUPG API",
        Version = "v1",
        Description = "BankUPG Microservice API with JWT Authentication, OTP Verification, and Rate Limiting"
    });
});

var app = builder.Build();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BankUPG API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Add Rate Limiting Middleware
app.UseMiddleware<RateLimitMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Minimal API endpoints
app.MapGet("/", () => "BankUPG API is running");
app.MapControllers();
app.Run();
