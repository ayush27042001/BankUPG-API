namespace BankUPG.SharedKernal.Models
{
    public class AppSettings
    {
        public string ConnectionString { get; set; }
        public JwtSettings Jwt { get; set; }
        public RateLimitSettings RateLimit { get; set; }
        public SwaggerSettings Swagger { get; set; }
        public SmsSettings Sms { get; set; }
    }

    public class JwtSettings
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpirationMinutes { get; set; }
    }

    public class RateLimitSettings
    {
        public int PermitsPerSecond { get; set; } = 10;
        public int BurstLimit { get; set; } = 20;
        public int WindowInSeconds { get; set; } = 60;
    }

    public class SwaggerSettings
    {
        public string Title { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
    }

    public class SmsSettings
    {
        public string ApiUrl { get; set; }
        public string Username { get; set; }
        public string ApiKey { get; set; }
        public string Sender { get; set; }
        public string Route { get; set; }
        public string TemplateId { get; set; }
        public string Format { get; set; }
    }
}
