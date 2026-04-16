using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class VwMerchantCompleteProfile
{
    public int Mid { get; set; }

    public string? Email { get; set; }

    public string? MobileNumber { get; set; }

    public string? BusinessName { get; set; }

    public string? BusinessEntityType { get; set; }

    public string? OnboardingStatus { get; set; }

    public string? Gstin { get; set; }

    public bool? HasGstin { get; set; }

    public string? BusinessCategory { get; set; }

    public string? BusinessSubCategory { get; set; }

    public decimal? ExpectedSalesPerMonth { get; set; }

    public string? PancardNumber { get; set; }

    public string? NameOnPancard { get; set; }

    public DateOnly? DateOfBirthOrIncorporation { get; set; }

    public string? PaymentCollectionPreference { get; set; }

    public string? WebsiteAppUrl { get; set; }

    public string? BankHolderName { get; set; }

    public string? BankName { get; set; }

    public string? SigningAuthorityName { get; set; }

    public string? SigningAuthorityEmail { get; set; }

    public string? Pepstatus { get; set; }

    public string? VideoKycstatus { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public DateTime? LastUpdatedDate { get; set; }
}
