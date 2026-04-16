using System;
using System.Collections.Generic;

namespace BankUPG.Infrastructure.Entities;

public partial class Merchant
{
    public int Mid { get; set; }

    public int UserId { get; set; }

    public string? BusinessName { get; set; }

    public int? BusinessEntityTypeId { get; set; }

    public int? OnboardingStatusId { get; set; }

    public string? Ckycidentifier { get; set; }

    public bool? CkycconsentGiven { get; set; }

    public DateTime? CkycconsentDate { get; set; }

    public decimal? ExpectedSalesPerMonth { get; set; }

    public bool? HasGstin { get; set; }

    public string? Gstin { get; set; }

    public int? BusinessCategoryId { get; set; }

    public int? BusinessSubCategoryId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual BankAccountDetail? BankAccountDetail { get; set; }

    public virtual BusinessAddressDetail? BusinessAddressDetail { get; set; }

    public virtual BusinessCategory? BusinessCategory { get; set; }

    public virtual BusinessEntityType? BusinessEntityType { get; set; }

    public virtual BusinessPandetail? BusinessPandetail { get; set; }

    public virtual BusinessSubCategory? BusinessSubCategory { get; set; }

    public virtual ICollection<DocumentUpload> DocumentUploads { get; set; } = new List<DocumentUpload>();

    public virtual ICollection<LoginAuditLog> LoginAuditLogs { get; set; } = new List<LoginAuditLog>();

    public virtual OnboardingStatus? OnboardingStatus { get; set; }

    public virtual ICollection<OnboardingStepTracking> OnboardingStepTrackings { get; set; } = new List<OnboardingStepTracking>();

    public virtual ICollection<Otpverification> Otpverifications { get; set; } = new List<Otpverification>();

    public virtual SigningAuthorityDetail? SigningAuthorityDetail { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual VideoKycdetail? VideoKycdetail { get; set; }

    public virtual WebsiteAppDetail? WebsiteAppDetail { get; set; }
}
