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

    public bool? IsOnboardingCompleted { get; set; }

    public bool? IsOnboardingRejected { get; set; }

    public bool IpWhitelistEnabled { get; set; }

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

    public virtual ServiceAgreement? ServiceAgreement { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual ICollection<Chargeback> Chargebacks { get; set; } = new List<Chargeback>();

    public virtual MerchantApiKey? MerchantApiKey { get; set; }

    public virtual ICollection<Webhook> Webhooks { get; set; } = new List<Webhook>();

    public virtual ICollection<WebhookLog> WebhookLogs { get; set; } = new List<WebhookLog>();

    public virtual CheckoutCustomization? CheckoutCustomization { get; set; }

    public virtual ICollection<MerchantPaymentMethod> MerchantPaymentMethods { get; set; } = new List<MerchantPaymentMethod>();

    public virtual ICollection<MerchantColumnPreference> MerchantColumnPreferences { get; set; } = new List<MerchantColumnPreference>();

    public virtual MerchantWallet? MerchantWallet { get; set; }

    public virtual ICollection<WalletLedger> WalletLedgers { get; set; } = new List<WalletLedger>();

    public virtual MerchantSettlementConfig? MerchantSettlementConfig { get; set; }

    public virtual ICollection<TransactionCharge> TransactionCharges { get; set; } = new List<TransactionCharge>();

    public virtual ICollection<MerchantDailySummary> MerchantDailySummaries { get; set; } = new List<MerchantDailySummary>();

    public virtual ICollection<PaymentOrder> PaymentOrders { get; set; } = new List<PaymentOrder>();

    public virtual ICollection<PaymentAttempt> PaymentAttempts { get; set; } = new List<PaymentAttempt>();

    public virtual ICollection<PaymentLink> PaymentLinks { get; set; } = new List<PaymentLink>();

    public virtual ICollection<Payout> Payouts { get; set; } = new List<Payout>();

    public virtual ICollection<PayoutBeneficiary> PayoutBeneficiaries { get; set; } = new List<PayoutBeneficiary>();

    public virtual ICollection<BatchRefund> BatchRefunds { get; set; } = new List<BatchRefund>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<SubscriptionPlan> SubscriptionPlans { get; set; } = new List<SubscriptionPlan>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual ICollection<EmiPlan> EmiPlans { get; set; } = new List<EmiPlan>();

    public virtual ICollection<PaymentLinkBulkUpload> PaymentLinkBulkUploads { get; set; } = new List<PaymentLinkBulkUpload>();

    public virtual ICollection<PaymentLinkBulkUploadFile> PaymentLinkBulkUploadFiles { get; set; } = new List<PaymentLinkBulkUploadFile>();

    public virtual ICollection<MerchantIpWhitelist> IpWhitelists { get; set; } = new List<MerchantIpWhitelist>();
}
