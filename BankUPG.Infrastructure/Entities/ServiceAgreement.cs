namespace BankUPG.Infrastructure.Entities;

public partial class ServiceAgreement
{
    public int ServiceAgreementId { get; set; }

    public int Mid { get; set; }

    public string? SignatureData { get; set; }

    public DateTime? AgreementDate { get; set; }

    public bool IsAccepted { get; set; }

    public DateTime? SubmittedDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Merchant MidNavigation { get; set; } = null!;
}
