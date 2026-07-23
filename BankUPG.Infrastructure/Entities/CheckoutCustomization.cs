using System;

namespace BankUPG.Infrastructure.Entities;

public partial class CheckoutCustomization
{
    public int CheckoutCustomizationId { get; set; }

    public int Mid { get; set; }

    public string? BrandLogoUrl { get; set; }

    public string? PrimaryColor { get; set; }

    public string? SecondaryColor { get; set; }

    public string? Language { get; set; }

    public string? OwnerSignatureUrl { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;
}
