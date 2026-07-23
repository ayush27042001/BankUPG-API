using System;

namespace BankUPG.Infrastructure.Entities;

public partial class MerchantApiKey
{
    public int MerchantApiKeyId { get; set; }

    public int Mid { get; set; }

    public string? ApiKey { get; set; }

    public string? ApiSalt { get; set; }

    public string? ClientId { get; set; }

    public string? ClientSecretHash { get; set; }

    public DateTime? LastUpdatedDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;
}
