using System;
using System.Collections.Generic;
using BankUPG.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BankUPG.Infrastructure.Data;

public partial class AppDBContext : DbContext
{
    public AppDBContext()
    {
    }

    public AppDBContext(DbContextOptions<AppDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BankAccountDetail> BankAccountDetails { get; set; }

    public virtual DbSet<BusinessAddressDetail> BusinessAddressDetails { get; set; }

    public virtual DbSet<BusinessCategory> BusinessCategories { get; set; }

    public virtual DbSet<BusinessEntityType> BusinessEntityTypes { get; set; }

    public virtual DbSet<BusinessPandetail> BusinessPandetails { get; set; }

    public virtual DbSet<BusinessProofType> BusinessProofTypes { get; set; }

    public virtual DbSet<BusinessSubCategory> BusinessSubCategories { get; set; }

    public virtual DbSet<DocumentType> DocumentTypes { get; set; }

    public virtual DbSet<DocumentUpload> DocumentUploads { get; set; }

    public virtual DbSet<LoginAuditLog> LoginAuditLogs { get; set; }

    public virtual DbSet<Merchant> Merchants { get; set; }

    public virtual DbSet<OnboardingStatus> OnboardingStatuses { get; set; }

    public virtual DbSet<OnboardingStepTracking> OnboardingStepTrackings { get; set; }

    public virtual DbSet<Otpverification> Otpverifications { get; set; }

    public virtual DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }

    public virtual DbSet<Pepstatus> Pepstatuses { get; set; }

    public virtual DbSet<SigningAuthorityDetail> SigningAuthorityDetails { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VideoKycdetail> VideoKycdetails { get; set; }

    public virtual DbSet<VwMerchantCompleteProfile> VwMerchantCompleteProfiles { get; set; }

    public virtual DbSet<WebsiteAppDetail> WebsiteAppDetails { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<ServiceAgreement> ServiceAgreements { get; set; }

    public virtual DbSet<SuperAdmin> SuperAdmins { get; set; }

    public virtual DbSet<SuperAdminRefreshToken> SuperAdminRefreshTokens { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<Refund> Refunds { get; set; }

    public virtual DbSet<Settlement> Settlements { get; set; }

    public virtual DbSet<Chargeback> Chargebacks { get; set; }

    public virtual DbSet<MerchantApiKey> MerchantApiKeys { get; set; }

    public virtual DbSet<Webhook> Webhooks { get; set; }

    public virtual DbSet<WebhookLog> WebhookLogs { get; set; }

    public virtual DbSet<CheckoutCustomization> CheckoutCustomizations { get; set; }

    public virtual DbSet<MerchantPaymentMethod> MerchantPaymentMethods { get; set; }

    public virtual DbSet<MerchantColumnPreference> MerchantColumnPreferences { get; set; }

    public virtual DbSet<MerchantWallet> MerchantWallets { get; set; }

    public virtual DbSet<WalletLedger> WalletLedgers { get; set; }

    public virtual DbSet<MerchantSettlementConfig> MerchantSettlementConfigs { get; set; }

    public virtual DbSet<PaymentMethodCharge> PaymentMethodCharges { get; set; }

    public virtual DbSet<TransactionCharge> TransactionCharges { get; set; }

    public virtual DbSet<MerchantDailySummary> MerchantDailySummaries { get; set; }

    public virtual DbSet<PaymentOrder> PaymentOrders { get; set; }

    public virtual DbSet<PaymentAttempt> PaymentAttempts { get; set; }

    public virtual DbSet<PaymentLink> PaymentLinks { get; set; }

    public virtual DbSet<Payout> Payouts { get; set; }

    public virtual DbSet<PayoutBeneficiary> PayoutBeneficiaries { get; set; }

    public virtual DbSet<BatchRefund> BatchRefunds { get; set; }

    public virtual DbSet<BatchRefundItem> BatchRefundItems { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceItem> InvoiceItems { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<EmiPlan> EmiPlans { get; set; }

    public virtual DbSet<PaymentLinkBulkUpload> PaymentLinkBulkUploads { get; set; }

    public virtual DbSet<PaymentLinkBulkUploadFile> PaymentLinkBulkUploadFiles { get; set; }

    public virtual DbSet<MerchantIpWhitelist> MerchantIpWhitelists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=97.74.80.235,1232;Initial Catalog=BankuPG;Persist Security Info=True;User ID=sa;Password=BankU@2022*1std^ca;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure DateTime to use IST timezone
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => DateTimeService.IstToUtc(v),
            v => DateTimeService.UtcToIst(v));

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? DateTimeService.IstToUtc(v.Value) : null,
            v => v.HasValue ? DateTimeService.UtcToIst(v.Value) : null);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }

        modelBuilder.Entity<BankAccountDetail>(entity =>
        {
            entity.HasKey(e => e.BankAccountDetailId).HasName("PK__BankAcco__263E0BB007C364C2");

            entity.HasIndex(e => e.Mid, "UQ_BankAccountDetails_MID").IsUnique();

            entity.Property(e => e.BankAccountDetailId).HasColumnName("BankAccountDetailID");
            entity.Property(e => e.AccountType).HasMaxLength(50);
            entity.Property(e => e.BankAccountNumber).HasMaxLength(50);
            entity.Property(e => e.BankHolderName).HasMaxLength(200);
            entity.Property(e => e.BankName).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Ifsccode)
                .HasMaxLength(20)
                .HasColumnName("IFSCCode");
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.VerifiedDate).HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithOne(p => p.BankAccountDetail)
                .HasForeignKey<BankAccountDetail>(d => d.Mid)
                .HasConstraintName("FK_BankAccountDetails_Merchants");
        });

        modelBuilder.Entity<BusinessAddressDetail>(entity =>
        {
            entity.HasKey(e => e.BusinessAddressDetailId).HasName("PK__Business__9710E7A6D0E66AB3");

            entity.HasIndex(e => e.Mid, "UQ_BusinessAddressDetails_MID").IsUnique();

            entity.Property(e => e.BusinessAddressDetailId).HasColumnName("BusinessAddressDetailID");
            entity.Property(e => e.AddressLine1).HasMaxLength(500);
            entity.Property(e => e.AddressLine2).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasDefaultValue("India");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.HasDifferentOperatingAddress).HasDefaultValue(false);
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.OperatingAddressLine1).HasMaxLength(500);
            entity.Property(e => e.OperatingAddressLine2).HasMaxLength(500);
            entity.Property(e => e.OperatingCity).HasMaxLength(100);
            entity.Property(e => e.OperatingCountry).HasMaxLength(100);
            entity.Property(e => e.OperatingPostalCode).HasMaxLength(20);
            entity.Property(e => e.OperatingState).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithOne(p => p.BusinessAddressDetail)
                .HasForeignKey<BusinessAddressDetail>(d => d.Mid)
                .HasConstraintName("FK_BusinessAddressDetails_Merchants");
        });

        modelBuilder.Entity<BusinessCategory>(entity =>
        {
            entity.HasKey(e => e.BusinessCategoryId).HasName("PK__Business__4024A24DDF74F1C5");

            entity.HasIndex(e => e.CategoryCode, "UQ__Business__371BA9552D2E8C65").IsUnique();

            entity.HasIndex(e => e.CategoryName, "UQ__Business__8517B2E0D7750B71").IsUnique();

            entity.Property(e => e.BusinessCategoryId).HasColumnName("BusinessCategoryID");
            entity.Property(e => e.CategoryCode).HasMaxLength(50);
            entity.Property(e => e.CategoryName).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<BusinessEntityType>(entity =>
        {
            entity.HasKey(e => e.BusinessEntityTypeId).HasName("PK__Business__837393FA3186B1E3");

            entity.HasIndex(e => e.EntityName, "UQ__Business__853BB33BF1FAE8CF").IsUnique();

            entity.Property(e => e.BusinessEntityTypeId).HasColumnName("BusinessEntityTypeID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EntityName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<BusinessPandetail>(entity =>
        {
            entity.HasKey(e => e.BusinessPandetailId).HasName("PK__Business__7959E2B2E3E4900D");

            entity.ToTable("BusinessPANDetails");

            entity.HasIndex(e => e.Mid, "UQ_BusinessPANDetails_MID").IsUnique();

            entity.Property(e => e.BusinessPandetailId).HasColumnName("BusinessPANDetailID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.NameOnPancard)
                .HasMaxLength(200)
                .HasColumnName("NameOnPANCard");
            entity.Property(e => e.PancardNumber)
                .HasMaxLength(10)
                .HasColumnName("PANCardNumber");
            entity.Property(e => e.PanverificationStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .HasColumnName("PANVerificationStatus");
            entity.Property(e => e.PanverifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("PANVerifiedDate");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithOne(p => p.BusinessPandetail)
                .HasForeignKey<BusinessPandetail>(d => d.Mid)
                .HasConstraintName("FK_BusinessPANDetails_Merchants");
        });

        modelBuilder.Entity<BusinessProofType>(entity =>
        {
            entity.HasKey(e => e.BusinessProofTypeId).HasName("PK__Business__A66C7E8B9FBAB378");

            entity.HasIndex(e => e.ProofName, "UQ__Business__0B0FDAEBFED4EF73").IsUnique();

            entity.HasIndex(e => e.ProofCode, "UQ__Business__108D6FC24E47DA10").IsUnique();

            entity.Property(e => e.BusinessProofTypeId).HasColumnName("BusinessProofTypeID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ProofCode).HasMaxLength(50);
            entity.Property(e => e.ProofName).HasMaxLength(100);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<BusinessSubCategory>(entity =>
        {
            entity.HasKey(e => e.BusinessSubCategoryId).HasName("PK__Business__400AE501F08C0186");

            entity.HasIndex(e => new { e.BusinessCategoryId, e.SubCategoryCode }, "UQ_BusinessSubCategories_Code").IsUnique();

            entity.Property(e => e.BusinessSubCategoryId).HasColumnName("BusinessSubCategoryID");
            entity.Property(e => e.BusinessCategoryId).HasColumnName("BusinessCategoryID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SubCategoryCode).HasMaxLength(50);
            entity.Property(e => e.SubCategoryName).HasMaxLength(200);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.BusinessCategory).WithMany(p => p.BusinessSubCategories)
                .HasForeignKey(d => d.BusinessCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BusinessSubCategories_Categories");
        });

        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.HasKey(e => e.DocumentTypeId).HasName("PK__Document__DBA390C106315FAB");

            entity.HasIndex(e => e.DocumentCode, "UQ__Document__2282345BF7C34BBF").IsUnique();

            entity.HasIndex(e => e.DocumentName, "UQ__Document__7DEDE07E413D3A37").IsUnique();

            entity.Property(e => e.DocumentTypeId).HasColumnName("DocumentTypeID");
            entity.Property(e => e.AllowedExtensions)
                .HasMaxLength(200)
                .HasDefaultValue("JPG,PNG,PDF");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DocumentCode).HasMaxLength(50);
            entity.Property(e => e.DocumentName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsRequired).HasDefaultValue(false);
            entity.Property(e => e.MaxFileSizeMb)
                .HasDefaultValue(5)
                .HasColumnName("MaxFileSizeMB");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<DocumentUpload>(entity =>
        {
            entity.HasKey(e => e.DocumentUploadId).HasName("PK__Document__2E00133C859077E5");

            entity.HasIndex(e => e.DocumentTypeId, "IX_DocumentUploads_DocumentTypeID");

            entity.HasIndex(e => e.IsVerified, "IX_DocumentUploads_IsVerified");

            entity.HasIndex(e => e.Mid, "IX_DocumentUploads_MID");

            entity.Property(e => e.DocumentUploadId).HasColumnName("DocumentUploadID");
            entity.Property(e => e.BusinessProofTypeId).HasColumnName("BusinessProofTypeID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DocumentFileName).HasMaxLength(500);
            entity.Property(e => e.DocumentFilePath).HasMaxLength(1000);
            entity.Property(e => e.DocumentMimeType).HasMaxLength(100);
            entity.Property(e => e.DocumentTypeId).HasColumnName("DocumentTypeID");
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.VerifiedDate).HasColumnType("datetime");

            entity.HasOne(d => d.BusinessProofType).WithMany(p => p.DocumentUploads)
                .HasForeignKey(d => d.BusinessProofTypeId)
                .HasConstraintName("FK_DocumentUploads_BusinessProofType");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.DocumentUploads)
                .HasForeignKey(d => d.DocumentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentUploads_DocumentType");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.DocumentUploads)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_DocumentUploads_Merchants");
        });

        modelBuilder.Entity<LoginAuditLog>(entity =>
        {
            entity.HasKey(e => e.LoginAuditLogId).HasName("PK__LoginAud__A5D4A7999A01DDFC");

            entity.ToTable("LoginAuditLog");

            entity.HasIndex(e => e.LoginAttemptDate, "IX_LoginAuditLog_LoginAttemptDate");

            entity.HasIndex(e => e.LoginStatus, "IX_LoginAuditLog_LoginStatus");

            entity.HasIndex(e => e.UserId, "IX_LoginAuditLog_UserID");

            entity.Property(e => e.LoginAuditLogId).HasColumnName("LoginAuditLogID");
            entity.Property(e => e.LoginAttemptDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LoginIpaddress)
                .HasMaxLength(50)
                .HasColumnName("LoginIPAddress");
            entity.Property(e => e.LoginStatus).HasMaxLength(50);
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.LoginAuditLogs)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_LoginAuditLog_Merchants");

            entity.HasOne(d => d.User).WithMany(p => p.LoginAuditLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoginAuditLog_Users");
        });

        modelBuilder.Entity<Merchant>(entity =>
        {
            entity.HasKey(e => e.Mid).HasName("PK__Merchant__C797348AA85C9811");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("tr_Merchants_PreventDelete");
                    tb.HasTrigger("tr_Merchants_UpdateDate");
                });

            entity.HasIndex(e => e.Ckycidentifier, "IX_Merchants_CKYCIdentifier");

            entity.HasIndex(e => e.Gstin, "IX_Merchants_GSTIN");

            entity.HasIndex(e => e.OnboardingStatusId, "IX_Merchants_OnboardingStatusID");

            entity.HasIndex(e => e.UserId, "IX_Merchants_UserID");

            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.BusinessCategoryId).HasColumnName("BusinessCategoryID");
            entity.Property(e => e.BusinessEntityTypeId).HasColumnName("BusinessEntityTypeID");
            entity.Property(e => e.BusinessName).HasMaxLength(500);
            entity.Property(e => e.BusinessSubCategoryId).HasColumnName("BusinessSubCategoryID");
            entity.Property(e => e.CkycconsentDate)
                .HasColumnType("datetime")
                .HasColumnName("CKYCConsentDate");
            entity.Property(e => e.CkycconsentGiven)
                .HasDefaultValue(false)
                .HasColumnName("CKYCConsentGiven");
            entity.Property(e => e.Ckycidentifier)
                .HasMaxLength(50)
                .HasColumnName("CKYCIdentifier");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpectedSalesPerMonth).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Gstin)
                .HasMaxLength(15)
                .HasColumnName("GSTIN");
            entity.Property(e => e.HasGstin)
                .HasDefaultValue(false)
                .HasColumnName("HasGSTIN");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsOnboardingCompleted)
                .HasDefaultValue(false)
                .HasColumnName("IsOnboardingCompleted");
            entity.Property(e => e.IsOnboardingRejected)
                .HasDefaultValue(false)
                .HasColumnName("IsOnboardingRejected");
            entity.Property(e => e.OnboardingStatusId)
                .HasDefaultValue(1)
                .HasColumnName("OnboardingStatusID");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.BusinessCategory).WithMany(p => p.Merchants)
                .HasForeignKey(d => d.BusinessCategoryId)
                .HasConstraintName("FK_Merchants_BusinessCategory");

            entity.HasOne(d => d.BusinessEntityType).WithMany(p => p.Merchants)
                .HasForeignKey(d => d.BusinessEntityTypeId)
                .HasConstraintName("FK_Merchants_BusinessEntityType");

            entity.HasOne(d => d.BusinessSubCategory).WithMany(p => p.Merchants)
                .HasForeignKey(d => d.BusinessSubCategoryId)
                .HasConstraintName("FK_Merchants_BusinessSubCategory");

            entity.HasOne(d => d.OnboardingStatus).WithMany(p => p.Merchants)
                .HasForeignKey(d => d.OnboardingStatusId)
                .HasConstraintName("FK_Merchants_OnboardingStatus");

            entity.HasOne(d => d.User).WithMany(p => p.Merchants)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Merchants_Users");
        });

        modelBuilder.Entity<OnboardingStatus>(entity =>
        {
            entity.HasKey(e => e.OnboardingStatusId).HasName("PK__Onboardi__D3AA290CCC41111A");

            entity.ToTable("OnboardingStatus");

            entity.HasIndex(e => e.StatusName, "UQ__Onboardi__05E7698A4C24A3A6").IsUnique();

            entity.Property(e => e.OnboardingStatusId).HasColumnName("OnboardingStatusID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StatusDescription).HasMaxLength(200);
            entity.Property(e => e.StatusName).HasMaxLength(50);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<OnboardingStepTracking>(entity =>
        {
            entity.HasKey(e => e.OnboardingStepTrackingId).HasName("PK__Onboardi__9FEFFBA24FA27667");

            entity.ToTable("OnboardingStepTracking");

            entity.HasIndex(e => e.Mid, "IX_OnboardingStepTracking_MID");

            entity.HasIndex(e => e.StepStatus, "IX_OnboardingStepTracking_StepStatus");

            entity.Property(e => e.OnboardingStepTrackingId).HasColumnName("OnboardingStepTrackingID");
            entity.Property(e => e.CompletedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.StartedDate).HasColumnType("datetime");
            entity.Property(e => e.StepName).HasMaxLength(100);
            entity.Property(e => e.StepStatus).HasMaxLength(50);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.OnboardingStepTrackings)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_OnboardingStepTracking_Merchants");
        });

        modelBuilder.Entity<Otpverification>(entity =>
        {
            entity.HasKey(e => e.OtpverificationId).HasName("PK__OTPVerif__37E08BD120ECDA05");

            entity.ToTable("OTPVerification");

            entity.HasIndex(e => e.OtpexpiryTime, "IX_OTPVerification_ExpiryTime");

            entity.HasIndex(e => e.Mid, "IX_OTPVerification_MID");

            entity.HasIndex(e => e.MobileNumber, "IX_OTPVerification_MobileNumber");

            entity.HasIndex(e => e.Otpcode, "IX_OTPVerification_OTPCode");

            entity.HasIndex(e => e.UserId, "IX_OTPVerification_UserID");

            entity.Property(e => e.OtpverificationId).HasColumnName("OTPVerificationID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.IsUsed).HasDefaultValue(false);
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.MobileNumber).HasMaxLength(20);
            entity.Property(e => e.Otpcode)
                .HasMaxLength(10)
                .HasColumnName("OTPCode");
            entity.Property(e => e.OtpcreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("OTPCreatedTime");
            entity.Property(e => e.OtpexpiryTime)
                .HasColumnType("datetime")
                .HasColumnName("OTPExpiryTime");
            entity.Property(e => e.Otppurpose)
                .HasMaxLength(50)
                .HasColumnName("OTPPurpose");
            entity.Property(e => e.UsedTime).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.Otpverifications)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_OTPVerification_Merchants");

            entity.HasOne(d => d.User).WithMany(p => p.Otpverifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_OTPVerification_Users");
        });

        modelBuilder.Entity<PasswordResetRequest>(entity =>
        {
            entity.HasKey(e => e.PasswordResetRequestId).HasName("PK__Password__AEE1B2A8B96E6051");

            entity.HasIndex(e => e.ResetToken, "UQ__Password__0395685B465DA596").IsUnique();

            entity.Property(e => e.PasswordResetRequestId).HasColumnName("PasswordResetRequestID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.IsUsed).HasDefaultValue(false);
            entity.Property(e => e.ResetToken).HasMaxLength(512);
            entity.Property(e => e.TokenExpiryTime).HasColumnType("datetime");
            entity.Property(e => e.UsedTime).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.PasswordResetRequests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PasswordResetRequests_Users");
        });

        modelBuilder.Entity<Pepstatus>(entity =>
        {
            entity.HasKey(e => e.PepstatusId).HasName("PK__PEPStatu__D6C75228954BAE8C");

            entity.ToTable("PEPStatus");

            entity.HasIndex(e => e.StatusName, "UQ__PEPStatu__05E7698A61B8A42B").IsUnique();

            entity.Property(e => e.PepstatusId).HasColumnName("PEPStatusID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StatusName).HasMaxLength(100);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<SigningAuthorityDetail>(entity =>
        {
            entity.HasKey(e => e.SigningAuthorityDetailId).HasName("PK__SigningA__38B251C5618E99F4");

            entity.HasIndex(e => e.Mid, "UQ_SigningAuthorityDetails_MID").IsUnique();

            entity.Property(e => e.SigningAuthorityDetailId).HasColumnName("SigningAuthorityDetailID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.PepstatusId).HasColumnName("PEPStatusID");
            entity.Property(e => e.SigningAuthorityEmail).HasMaxLength(255);
            entity.Property(e => e.SigningAuthorityName).HasMaxLength(200);
            entity.Property(e => e.SigningAuthorityPan)
                .HasMaxLength(10)
                .HasColumnName("SigningAuthorityPAN");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithOne(p => p.SigningAuthorityDetail)
                .HasForeignKey<SigningAuthorityDetail>(d => d.Mid)
                .HasConstraintName("FK_SigningAuthorityDetails_Merchants");

            entity.HasOne(d => d.Pepstatus).WithMany(p => p.SigningAuthorityDetails)
                .HasForeignKey(d => d.PepstatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SigningAuthorityDetails_PEPStatus");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACE6EEC573");

            entity.HasIndex(e => e.Email, "IX_Users_Email");

            entity.HasIndex(e => e.IsActive, "IX_Users_IsActive");

            entity.HasIndex(e => e.MobileNumber, "IX_Users_MobileNumber");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053416F06352").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FailedLoginAttempts).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsEmailVerified).HasDefaultValue(false);
            entity.Property(e => e.IsLocked).HasDefaultValue(false);
            entity.Property(e => e.IsMobileVerified).HasDefaultValue(false);
            entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
            entity.Property(e => e.MobileNumber).HasMaxLength(20);
            entity.Property(e => e.PasswordHash).HasMaxLength(512);
            entity.Property(e => e.PasswordLastChangedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Salt).HasMaxLength(256);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<VideoKycdetail>(entity =>
        {
            entity.HasKey(e => e.VideoKycdetailId).HasName("PK__VideoKYC__DE7F2A16EDDC8C4B");

            entity.ToTable("VideoKYCDetails");

            entity.HasIndex(e => e.Mid, "UQ_VideoKYCDetails_MID").IsUnique();

            entity.Property(e => e.VideoKycdetailId).HasColumnName("VideoKYCDetailID");
            entity.Property(e => e.AadhaarVerified).HasDefaultValue(false);
            entity.Property(e => e.AgentId).HasColumnName("AgentID");
            entity.Property(e => e.AgentName).HasMaxLength(200);
            entity.Property(e => e.CompletedDateTime).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DigilockerReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.Remarks).HasMaxLength(1000);
            entity.Property(e => e.ScheduledDateTime).HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.VideoKycstatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .HasColumnName("VideoKYCStatus");
            entity.Property(e => e.VideoRecordingUrl)
                .HasMaxLength(1000)
                .HasColumnName("VideoRecordingURL");

            entity.HasOne(d => d.MidNavigation).WithOne(p => p.VideoKycdetail)
                .HasForeignKey<VideoKycdetail>(d => d.Mid)
                .HasConstraintName("FK_VideoKYCDetails_Merchants");
        });

        modelBuilder.Entity<VwMerchantCompleteProfile>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_MerchantCompleteProfile");

            entity.Property(e => e.BankHolderName).HasMaxLength(200);
            entity.Property(e => e.BankName).HasMaxLength(200);
            entity.Property(e => e.BusinessCategory).HasMaxLength(200);
            entity.Property(e => e.BusinessEntityType).HasMaxLength(100);
            entity.Property(e => e.BusinessName).HasMaxLength(500);
            entity.Property(e => e.BusinessSubCategory).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.ExpectedSalesPerMonth).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Gstin)
                .HasMaxLength(15)
                .HasColumnName("GSTIN");
            entity.Property(e => e.HasGstin).HasColumnName("HasGSTIN");
            entity.Property(e => e.LastUpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.MobileNumber).HasMaxLength(20);
            entity.Property(e => e.NameOnPancard)
                .HasMaxLength(200)
                .HasColumnName("NameOnPANCard");
            entity.Property(e => e.OnboardingStatus).HasMaxLength(50);
            entity.Property(e => e.PancardNumber)
                .HasMaxLength(10)
                .HasColumnName("PANCardNumber");
            entity.Property(e => e.PaymentCollectionPreference).HasMaxLength(50);
            entity.Property(e => e.Pepstatus)
                .HasMaxLength(100)
                .HasColumnName("PEPStatus");
            entity.Property(e => e.RegistrationDate).HasColumnType("datetime");
            entity.Property(e => e.SigningAuthorityEmail).HasMaxLength(255);
            entity.Property(e => e.SigningAuthorityName).HasMaxLength(200);
            entity.Property(e => e.VideoKycstatus)
                .HasMaxLength(50)
                .HasColumnName("VideoKYCStatus");
            entity.Property(e => e.WebsiteAppUrl)
                .HasMaxLength(500)
                .HasColumnName("WebsiteAppURL");
        });

        modelBuilder.Entity<WebsiteAppDetail>(entity =>
        {
            entity.HasKey(e => e.WebsiteAppDetailId).HasName("PK__WebsiteA__ED87645CB8FBDF91");

            entity.HasIndex(e => e.Mid, "UQ_WebsiteAppDetails_MID").IsUnique();

            entity.Property(e => e.WebsiteAppDetailId).HasColumnName("WebsiteAppDetailID");
            entity.Property(e => e.AndroidAppUrl)
                .HasMaxLength(500)
                .HasColumnName("AndroidAppURL");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IOsappUrl)
                .HasMaxLength(500)
                .HasColumnName("iOSAppURL");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.PaymentCollectionPreference).HasMaxLength(50);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.WebsiteAppUrl)
                .HasMaxLength(500)
                .HasColumnName("WebsiteAppURL");

            entity.HasOne(d => d.MidNavigation).WithOne(p => p.WebsiteAppDetail)
                .HasForeignKey<WebsiteAppDetail>(d => d.Mid)
                .HasConstraintName("FK_WebsiteAppDetails_Merchants");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("PK__RefreshT__F5835E3977E9E1C0");

            entity.ToTable("RefreshTokens");

            entity.HasIndex(e => e.Token, "UQ__RefreshT__1EB4F817BF4C21A2").IsUnique();

            entity.HasIndex(e => e.UserId, "IX_RefreshTokens_UserId");

            entity.Property(e => e.RefreshTokenId).HasColumnName("RefreshTokenID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.ReplacedByToken).HasMaxLength(500);
            entity.Property(e => e.RevokedAt).HasColumnType("datetime");
            entity.Property(e => e.Token).HasMaxLength(500);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RefreshTokens_Users");
        });

        modelBuilder.Entity<ServiceAgreement>(entity =>
        {
            entity.HasKey(e => e.ServiceAgreementId).HasName("PK__ServiceAgreements__ID");

            entity.HasIndex(e => e.Mid, "UQ_ServiceAgreements_MID").IsUnique();

            entity.Property(e => e.ServiceAgreementId).HasColumnName("ServiceAgreementID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.SignatureData).HasColumnType("nvarchar(max)");
            entity.Property(e => e.AgreementDate).HasColumnType("date");
            entity.Property(e => e.IsAccepted).HasDefaultValue(false);
            entity.Property(e => e.SubmittedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithOne(p => p.ServiceAgreement)
                .HasForeignKey<ServiceAgreement>(d => d.Mid)
                .HasConstraintName("FK_ServiceAgreements_Merchants");
        });

        modelBuilder.Entity<SuperAdmin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK__SuperAdm__719FE48802A36FBE");

            entity.ToTable("SuperAdmins");

            entity.HasIndex(e => e.Username, "UQ__SuperAdm__Username").IsUnique();
            entity.HasIndex(e => e.Email, "UQ__SuperAdm__Email").IsUnique();

            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.MobileNumber).HasMaxLength(20);
            entity.Property(e => e.PasswordHash).HasMaxLength(512);
            entity.Property(e => e.Salt).HasMaxLength(256);
            entity.Property(e => e.Role).HasMaxLength(50).HasDefaultValue("SuperAdmin");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsLocked).HasDefaultValue(false);
            entity.Property(e => e.FailedLoginAttempts).HasDefaultValue(0);
            entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<SuperAdminRefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("PK__SuperAdm__RefreshToken");

            entity.ToTable("SuperAdminRefreshTokens");

            entity.HasIndex(e => e.Token, "UQ__SuperAdmRefreshT__Token").IsUnique();
            entity.HasIndex(e => e.AdminId, "IX__SuperAdmRefreshT__AdminId");

            entity.Property(e => e.RefreshTokenId).HasColumnName("RefreshTokenID");
            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.Token).HasMaxLength(500);
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RevokedAt).HasColumnType("datetime");
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            entity.HasOne(d => d.Admin).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SuperAdminRefreshTokens_SuperAdmins");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK_Transactions");

            entity.HasIndex(e => e.Mid, "IX_Transactions_MID");
            entity.HasIndex(e => e.PayuId, "IX_Transactions_PayuId");
            entity.HasIndex(e => e.Status, "IX_Transactions_Status");
            entity.HasIndex(e => e.TransactionDate, "IX_Transactions_TransactionDate");
            entity.HasIndex(e => e.CustomerEmail, "IX_Transactions_CustomerEmail");

            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.PayuId).HasMaxLength(100);
            entity.Property(e => e.MerchantReferenceId).HasMaxLength(200);
            entity.Property(e => e.CustomerEmail).HasMaxLength(255);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.PaymentMode).HasMaxLength(100);
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.UpiReference).HasMaxLength(200);
            entity.Property(e => e.BankReference).HasMaxLength(200);
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.PaymentLinkId).HasColumnName("PaymentLinkID");
            entity.Property(e => e.SubscriptionId).HasColumnName("SubscriptionID");
            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_Transactions_Merchants");

            entity.HasOne(d => d.Order).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_Transactions_PaymentOrders");

            entity.HasOne(d => d.PaymentLink).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PaymentLinkId)
                .HasConstraintName("FK_Transactions_PaymentLinks");

            entity.HasOne(d => d.Subscription).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("FK_Transactions_Subscriptions");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("FK_Transactions_Invoices");
        });

        modelBuilder.Entity<Refund>(entity =>
        {
            entity.HasKey(e => e.RefundId).HasName("PK_Refunds");

            entity.HasIndex(e => e.Mid, "IX_Refunds_MID");
            entity.HasIndex(e => e.TransactionId, "IX_Refunds_TransactionID");
            entity.HasIndex(e => e.PayuId, "IX_Refunds_PayuId");
            entity.HasIndex(e => e.Status, "IX_Refunds_Status");

            entity.Property(e => e.RefundId).HasColumnName("RefundID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.PayuId).HasMaxLength(100);
            entity.Property(e => e.MerchantReferenceId).HasMaxLength(200);
            entity.Property(e => e.RefundType).HasMaxLength(100);
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.Property(e => e.BankArn).HasMaxLength(200);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.PaymentAggregator).HasMaxLength(100);
            entity.Property(e => e.RefundDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.Refunds)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_Refunds_Merchants");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Refunds)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("FK_Refunds_Transactions");
        });

        modelBuilder.Entity<Settlement>(entity =>
        {
            entity.HasKey(e => e.SettlementId).HasName("PK_Settlements");

            entity.HasIndex(e => e.Mid, "IX_Settlements_MID");
            entity.HasIndex(e => e.UtrNumber, "IX_Settlements_UtrNumber");
            entity.HasIndex(e => e.Status, "IX_Settlements_Status");
            entity.HasIndex(e => e.SettlementDate, "IX_Settlements_SettlementDate");

            entity.Property(e => e.SettlementId).HasColumnName("SettlementID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.UtrNumber).HasMaxLength(100);
            entity.Property(e => e.SalesAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Fees).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SettledAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.SettlementCycle).HasMaxLength(50);
            entity.Property(e => e.SettlementDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.Property(e => e.SettlementT);

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.Settlements)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_Settlements_Merchants");
        });

        modelBuilder.Entity<Chargeback>(entity =>
        {
            entity.HasKey(e => e.ChargebackId).HasName("PK_Chargebacks");

            entity.HasIndex(e => e.Mid, "IX_Chargebacks_MID");
            entity.HasIndex(e => e.TransactionId, "IX_Chargebacks_TransactionID");
            entity.HasIndex(e => e.PayuId, "IX_Chargebacks_PayuId");
            entity.HasIndex(e => e.Status, "IX_Chargebacks_Status");

            entity.Property(e => e.ChargebackId).HasColumnName("ChargebackID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.PayuId).HasMaxLength(100);
            entity.Property(e => e.BankCaseNumber).HasMaxLength(100);
            entity.Property(e => e.CaseNumber).HasMaxLength(100);
            entity.Property(e => e.ChargebackDate).HasColumnType("datetime");
            entity.Property(e => e.ReplyBefore).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.ChargebackReason).HasMaxLength(500);
            entity.Property(e => e.ChargebackType).HasMaxLength(50);
            entity.Property(e => e.CloseReason).HasMaxLength(50);
            entity.Property(e => e.DocumentPath).HasMaxLength(1000);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.Chargebacks)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_Chargebacks_Merchants");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Chargebacks)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("FK_Chargebacks_Transactions");
        });

        modelBuilder.Entity<MerchantApiKey>(entity =>
        {
            entity.HasKey(e => e.MerchantApiKeyId).HasName("PK_MerchantApiKeys");

            entity.HasIndex(e => e.Mid, "UQ_MerchantApiKeys_MID").IsUnique();

            entity.Property(e => e.MerchantApiKeyId).HasColumnName("MerchantApiKeyID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.ApiKey).HasMaxLength(500);
            entity.Property(e => e.ApiSalt).HasMaxLength(500);
            entity.Property(e => e.ClientId).HasMaxLength(200);
            entity.Property(e => e.ClientSecretHash).HasMaxLength(512);
            entity.Property(e => e.LastUpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithOne(p => p.MerchantApiKey)
                .HasForeignKey<MerchantApiKey>(d => d.Mid)
                .HasConstraintName("FK_MerchantApiKeys_Merchants");
        });

        modelBuilder.Entity<Webhook>(entity =>
        {
            entity.HasKey(e => e.WebhookId).HasName("PK_Webhooks");

            entity.HasIndex(e => e.Mid, "IX_Webhooks_MID");
            entity.HasIndex(e => e.Status, "IX_Webhooks_Status");

            entity.Property(e => e.WebhookId).HasColumnName("WebhookID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.Event).HasMaxLength(50);
            entity.Property(e => e.WebhookUrl).HasMaxLength(1000);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.Webhooks)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_Webhooks_Merchants");
        });

        modelBuilder.Entity<WebhookLog>(entity =>
        {
            entity.HasKey(e => e.WebhookLogId).HasName("PK_WebhookLogs");

            entity.HasIndex(e => e.WebhookId, "IX_WebhookLogs_WebhookID");
            entity.HasIndex(e => e.Mid, "IX_WebhookLogs_MID");
            entity.HasIndex(e => e.LogDate, "IX_WebhookLogs_LogDate");

            entity.Property(e => e.WebhookLogId).HasColumnName("WebhookLogID");
            entity.Property(e => e.WebhookId).HasColumnName("WebhookID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.TransactionReference).HasMaxLength(200);
            entity.Property(e => e.EventType).HasMaxLength(100);
            entity.Property(e => e.ErrorMessage).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.LogDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Webhook).WithMany(p => p.WebhookLogs)
                .HasForeignKey(d => d.WebhookId)
                .HasConstraintName("FK_WebhookLogs_Webhooks");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.WebhookLogs)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_WebhookLogs_Merchants");
        });

        modelBuilder.Entity<CheckoutCustomization>(entity =>
        {
            entity.HasKey(e => e.CheckoutCustomizationId).HasName("PK_CheckoutCustomizations");

            entity.HasIndex(e => e.Mid, "UQ_CheckoutCustomizations_MID").IsUnique();

            entity.Property(e => e.CheckoutCustomizationId).HasColumnName("CheckoutCustomizationID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.BrandLogoUrl).HasMaxLength(1000);
            entity.Property(e => e.PrimaryColor).HasMaxLength(20);
            entity.Property(e => e.SecondaryColor).HasMaxLength(20);
            entity.Property(e => e.Language).HasMaxLength(50).HasDefaultValue("English");
            entity.Property(e => e.OwnerSignatureUrl).HasMaxLength(1000);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithOne(p => p.CheckoutCustomization)
                .HasForeignKey<CheckoutCustomization>(d => d.Mid)
                .HasConstraintName("FK_CheckoutCustomizations_Merchants");
        });

        modelBuilder.Entity<MerchantPaymentMethod>(entity =>
        {
            entity.HasKey(e => e.MerchantPaymentMethodId).HasName("PK_MerchantPaymentMethods");

            entity.HasIndex(e => e.Mid, "IX_MerchantPaymentMethods_MID");
            entity.HasIndex(e => e.PaymentMethodType, "IX_MerchantPaymentMethods_PaymentMethodType");

            entity.Property(e => e.MerchantPaymentMethodId).HasColumnName("MerchantPaymentMethodID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.PaymentMethodType).HasMaxLength(100);
            entity.Property(e => e.SubMethodName).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.MerchantPaymentMethods)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_MerchantPaymentMethods_Merchants");
        });

        modelBuilder.Entity<PaymentOrder>(entity =>
        {
            entity.HasKey(e => e.PaymentOrderId).HasName("PK_PaymentOrders");

            entity.HasIndex(e => e.Mid, "IX_PaymentOrders_MID");
            entity.HasIndex(e => e.Status, "IX_PaymentOrders_Status");
            entity.HasIndex(e => e.OrderRef, "IX_PaymentOrders_OrderRef");

            entity.Property(e => e.PaymentOrderId).HasColumnName("PaymentOrderID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.OrderRef).HasMaxLength(200);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Currency).HasMaxLength(10).HasDefaultValue("INR");
            entity.Property(e => e.CustomerEmail).HasMaxLength(255);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("created");
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.PaidDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.PaymentOrders)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_PaymentOrders_Merchants");
        });

        modelBuilder.Entity<PaymentAttempt>(entity =>
        {
            entity.HasKey(e => e.PaymentAttemptId).HasName("PK_PaymentAttempts");

            entity.HasIndex(e => e.OrderId, "IX_PaymentAttempts_OrderID");
            entity.HasIndex(e => e.Mid, "IX_PaymentAttempts_MID");
            entity.HasIndex(e => e.TransactionId, "IX_PaymentAttempts_TransactionID");
            entity.HasIndex(e => e.Status, "IX_PaymentAttempts_Status");

            entity.Property(e => e.PaymentAttemptId).HasColumnName("PaymentAttemptID");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.PaymentMode).HasMaxLength(100);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.AttemptDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Order).WithMany(p => p.PaymentAttempts)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_PaymentAttempts_PaymentOrders");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.PaymentAttempts)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_PaymentAttempts_Merchants");

            entity.HasOne(d => d.Transaction).WithMany(p => p.PaymentAttempts)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("FK_PaymentAttempts_Transactions");
        });

        modelBuilder.Entity<PaymentLink>(entity =>
        {
            entity.HasKey(e => e.PaymentLinkId).HasName("PK_PaymentLinks");

            entity.HasIndex(e => e.Mid, "IX_PaymentLinks_MID");
            entity.HasIndex(e => e.Status, "IX_PaymentLinks_Status");
            entity.HasIndex(e => e.ShortUrl, "UQ_PaymentLinks_ShortUrl").IsUnique();

            entity.Property(e => e.PaymentLinkId).HasColumnName("PaymentLinkID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.AmountType).HasMaxLength(20).HasDefaultValue("fixed");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Purpose).HasMaxLength(200);
            entity.Property(e => e.CustomerEmail).HasMaxLength(255);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("created");
            entity.Property(e => e.PaymentType).HasMaxLength(20).HasDefaultValue("Standard");
            entity.Property(e => e.IsPartialPayment).HasDefaultValue(false);
            entity.Property(e => e.MaxPaymentsAllowed);
            entity.Property(e => e.ValidationPeriod);
            entity.Property(e => e.TimeUnit).HasMaxLength(5);
            entity.Property(e => e.SendSms).HasDefaultValue(false);
            entity.Property(e => e.ShortUrl).HasMaxLength(300);
            entity.Property(e => e.ReferenceId).HasMaxLength(200);
            entity.Property(e => e.InvoiceId).HasMaxLength(100);
            entity.Property(e => e.MaxUses);
            entity.Property(e => e.UsedCount).HasDefaultValue(0);
            entity.Property(e => e.TotalViews).HasDefaultValue(0);
            entity.Property(e => e.TotalAmountPaid).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.CustomerDataCapture);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.PaymentLinks)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_PaymentLinks_Merchants");
        });

        modelBuilder.Entity<PayoutBeneficiary>(entity =>
        {
            entity.HasKey(e => e.PayoutBeneficiaryId).HasName("PK_PayoutBeneficiaries");

            entity.HasIndex(e => e.Mid, "IX_PayoutBeneficiaries_MID");
            entity.HasIndex(e => e.IsActive, "IX_PayoutBeneficiaries_IsActive");

            entity.Property(e => e.PayoutBeneficiaryId).HasColumnName("PayoutBeneficiaryID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.BeneficiaryName).HasMaxLength(200);
            entity.Property(e => e.AccountNumber).HasMaxLength(50);
            entity.Property(e => e.Ifsccode).HasMaxLength(20).HasColumnName("IFSCCode");
            entity.Property(e => e.BankName).HasMaxLength(200);
            entity.Property(e => e.AccountType).HasMaxLength(20);
            entity.Property(e => e.UpiId).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.PayoutBeneficiaries)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_PayoutBeneficiaries_Merchants");
        });

        modelBuilder.Entity<Payout>(entity =>
        {
            entity.HasKey(e => e.PayoutId).HasName("PK_Payouts");

            entity.HasIndex(e => e.Mid, "IX_Payouts_MID");
            entity.HasIndex(e => e.Status, "IX_Payouts_Status");
            entity.HasIndex(e => e.UtrNumber, "IX_Payouts_UtrNumber");

            entity.Property(e => e.PayoutId).HasColumnName("PayoutID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.BeneficiaryId).HasColumnName("BeneficiaryID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Currency).HasMaxLength(10).HasDefaultValue("INR");
            entity.Property(e => e.Mode).HasMaxLength(10);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("queued");
            entity.Property(e => e.ReferenceId).HasMaxLength(200);
            entity.Property(e => e.UtrNumber).HasMaxLength(100);
            entity.Property(e => e.Narration).HasMaxLength(500);
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.ScheduledDate).HasColumnType("datetime");
            entity.Property(e => e.ProcessedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.Payouts)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_Payouts_Merchants");

            entity.HasOne(d => d.Beneficiary).WithMany(p => p.Payouts)
                .HasForeignKey(d => d.BeneficiaryId)
                .HasConstraintName("FK_Payouts_PayoutBeneficiaries");
        });

        modelBuilder.Entity<BatchRefund>(entity =>
        {
            entity.HasKey(e => e.BatchRefundId).HasName("PK_BatchRefunds");

            entity.HasIndex(e => e.Mid, "IX_BatchRefunds_MID");
            entity.HasIndex(e => e.Status, "IX_BatchRefunds_Status");

            entity.Property(e => e.BatchRefundId).HasColumnName("BatchRefundID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.BatchReferenceId).HasMaxLength(200);
            entity.Property(e => e.TotalItems).HasDefaultValue(0);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.ProcessedItems).HasDefaultValue(0);
            entity.Property(e => e.SuccessCount).HasDefaultValue(0);
            entity.Property(e => e.FailedCount).HasDefaultValue(0);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("pending");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.BatchRefunds)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_BatchRefunds_Merchants");
        });

        modelBuilder.Entity<BatchRefundItem>(entity =>
        {
            entity.HasKey(e => e.BatchRefundItemId).HasName("PK_BatchRefundItems");

            entity.HasIndex(e => e.BatchRefundId, "IX_BatchRefundItems_BatchRefundID");
            entity.HasIndex(e => e.TransactionId, "IX_BatchRefundItems_TransactionID");
            entity.HasIndex(e => e.RefundId, "IX_BatchRefundItems_RefundID");

            entity.Property(e => e.BatchRefundItemId).HasColumnName("BatchRefundItemID");
            entity.Property(e => e.BatchRefundId).HasColumnName("BatchRefundID");
            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.RefundId).HasColumnName("RefundID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("pending");
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.BatchRefund).WithMany(p => p.BatchRefundItems)
                .HasForeignKey(d => d.BatchRefundId)
                .HasConstraintName("FK_BatchRefundItems_BatchRefunds");

            entity.HasOne(d => d.Transaction).WithMany(p => p.BatchRefundItems)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("FK_BatchRefundItems_Transactions");

            entity.HasOne(d => d.Refund).WithMany()
                .HasForeignKey(d => d.RefundId)
                .HasConstraintName("FK_BatchRefundItems_Refunds");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK_Invoices");

            entity.HasIndex(e => e.Mid, "IX_Invoices_MID");
            entity.HasIndex(e => e.Status, "IX_Invoices_Status");
            entity.HasIndex(e => new { e.Mid, e.InvoiceNumber }, "UQ_Invoices_MID_InvoiceNumber").IsUnique();

            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50);
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.CustomerEmail).HasMaxLength(255);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("draft");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.PaidDate).HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_Invoices_Merchants");
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(e => e.InvoiceItemId).HasName("PK_InvoiceItems");

            entity.HasIndex(e => e.InvoiceId, "IX_InvoiceItems_InvoiceID");

            entity.Property(e => e.InvoiceItemId).HasColumnName("InvoiceItemID");
            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceItems)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("FK_InvoiceItems_Invoices");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.SubscriptionPlanId).HasName("PK_SubscriptionPlans");

            entity.HasIndex(e => e.Mid, "IX_SubscriptionPlans_MID");
            entity.HasIndex(e => e.IsActive, "IX_SubscriptionPlans_IsActive");

            entity.Property(e => e.SubscriptionPlanId).HasColumnName("SubscriptionPlanID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.PlanName).HasMaxLength(200);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Currency).HasMaxLength(10).HasDefaultValue("INR");
            entity.Property(e => e.Interval).HasMaxLength(20);
            entity.Property(e => e.IntervalCount).HasDefaultValue(1);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.SubscriptionPlans)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_SubscriptionPlans_Merchants");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId).HasName("PK_Subscriptions");

            entity.HasIndex(e => e.Mid, "IX_Subscriptions_MID");
            entity.HasIndex(e => e.PlanId, "IX_Subscriptions_PlanID");
            entity.HasIndex(e => e.Status, "IX_Subscriptions_Status");
            entity.HasIndex(e => e.NextBillingDate, "IX_Subscriptions_NextBillingDate");

            entity.Property(e => e.SubscriptionId).HasColumnName("SubscriptionID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.PlanId).HasColumnName("PlanID");
            entity.Property(e => e.CustomerEmail).HasMaxLength(255);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            entity.Property(e => e.CustomerName).HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("created");
            entity.Property(e => e.CurrentCycle).HasDefaultValue(0);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.NextBillingDate).HasColumnType("datetime");
            entity.Property(e => e.UpiMandateRef).HasMaxLength(200);
            entity.Property(e => e.NachMandateRef).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_Subscriptions_Merchants");

            entity.HasOne(d => d.Plan).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("FK_Subscriptions_SubscriptionPlans");
        });

        modelBuilder.Entity<EmiPlan>(entity =>
        {
            entity.HasKey(e => e.EmiPlanId).HasName("PK_EmiPlans");

            entity.HasIndex(e => e.Mid, "IX_EmiPlans_MID");
            entity.HasIndex(e => e.IsActive, "IX_EmiPlans_IsActive");

            entity.Property(e => e.EmiPlanId).HasColumnName("EmiPlanID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.BankName).HasMaxLength(200);
            entity.Property(e => e.CardType).HasMaxLength(20);
            entity.Property(e => e.Tenure);
            entity.Property(e => e.InterestRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.MinAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.EmiPlans)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_EmiPlans_Merchants");
        });

        modelBuilder.Entity<MerchantWallet>(entity =>
        {
            entity.HasKey(e => e.MerchantWalletId).HasName("PK_MerchantWallets");

            entity.HasIndex(e => e.Mid, "UQ_MerchantWallets_MID").IsUnique();

            entity.Property(e => e.MerchantWalletId).HasColumnName("MerchantWalletID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.TotalBalance).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.OnHoldBalance).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.RefundWalletBalance).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.TotalCredited).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.TotalDebited).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithOne(p => p.MerchantWallet)
                .HasForeignKey<MerchantWallet>(d => d.Mid)
                .HasConstraintName("FK_MerchantWallets_Merchants");
        });

        modelBuilder.Entity<WalletLedger>(entity =>
        {
            entity.HasKey(e => e.WalletLedgerId).HasName("PK_WalletLedger");

            entity.HasIndex(e => e.MerchantWalletId, "IX_WalletLedger_WalletID");
            entity.HasIndex(e => e.Mid, "IX_WalletLedger_MID");
            entity.HasIndex(e => e.CreatedDate, "IX_WalletLedger_CreatedDate");

            entity.Property(e => e.WalletLedgerId).HasColumnName("WalletLedgerID");
            entity.Property(e => e.MerchantWalletId).HasColumnName("MerchantWalletID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.ReferenceType).HasMaxLength(50);
            entity.Property(e => e.EntryType).HasMaxLength(10);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BalanceAfter).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MerchantWallet).WithMany(p => p.WalletLedgers)
                .HasForeignKey(d => d.MerchantWalletId)
                .HasConstraintName("FK_WalletLedger_MerchantWallets");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.WalletLedgers)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_WalletLedger_Merchants");
        });

        modelBuilder.Entity<MerchantSettlementConfig>(entity =>
        {
            entity.HasKey(e => e.MerchantSettlementConfigId).HasName("PK_MerchantSettlementConfigs");

            entity.HasIndex(e => e.Mid, "UQ_MerchantSettlementConfigs_MID").IsUnique();

            entity.Property(e => e.MerchantSettlementConfigId).HasColumnName("MerchantSettlementConfigID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.SettlementT).HasDefaultValue(2);
            entity.Property(e => e.SettlementCycleType).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.EffectiveFrom).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithOne(p => p.MerchantSettlementConfig)
                .HasForeignKey<MerchantSettlementConfig>(d => d.Mid)
                .HasConstraintName("FK_MerchantSettlementConfigs_Merchants");
        });

        modelBuilder.Entity<PaymentMethodCharge>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodChargeId).HasName("PK_PaymentMethodCharges");

            entity.HasIndex(e => new { e.PaymentMethodType, e.NetworkName }, "IX_PaymentMethodCharges_Type_Network");

            entity.Property(e => e.PaymentMethodChargeId).HasColumnName("PaymentMethodChargeID");
            entity.Property(e => e.PaymentMethodType).HasMaxLength(100);
            entity.Property(e => e.NetworkName).HasMaxLength(100);
            entity.Property(e => e.ChargeType).HasMaxLength(20);
            entity.Property(e => e.ChargeValue).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.MinCharge).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaxCharge).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GstPercentage).HasColumnType("decimal(5, 2)").HasDefaultValue(18m);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<TransactionCharge>(entity =>
        {
            entity.HasKey(e => e.TransactionChargeId).HasName("PK_TransactionCharges");

            entity.HasIndex(e => e.TransactionId, "IX_TransactionCharges_TransactionID");
            entity.HasIndex(e => e.Mid, "IX_TransactionCharges_MID");

            entity.Property(e => e.TransactionChargeId).HasColumnName("TransactionChargeID");
            entity.Property(e => e.TransactionId).HasColumnName("TransactionID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.PaymentMethodChargeId).HasColumnName("PaymentMethodChargeID");
            entity.Property(e => e.PaymentMethodType).HasMaxLength(100);
            entity.Property(e => e.NetworkName).HasMaxLength(100);
            entity.Property(e => e.ChargeType).HasMaxLength(20);
            entity.Property(e => e.ChargeValue).HasColumnType("decimal(10, 4)");
            entity.Property(e => e.TransactionAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ChargeAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GstAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalDeduction).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NetAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TransactionCharges)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("FK_TransactionCharges_Transactions");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.TransactionCharges)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_TransactionCharges_Merchants");

            entity.HasOne(d => d.PaymentMethodCharge).WithMany(p => p.TransactionCharges)
                .HasForeignKey(d => d.PaymentMethodChargeId)
                .HasConstraintName("FK_TransactionCharges_PaymentMethodCharges");
        });

        modelBuilder.Entity<MerchantDailySummary>(entity =>
        {
            entity.HasKey(e => e.MerchantDailySummaryId).HasName("PK_MerchantDailySummaries");

            entity.HasIndex(e => new { e.Mid, e.SummaryDate }, "UQ_MerchantDailySummaries_MID_Date").IsUnique();

            entity.Property(e => e.MerchantDailySummaryId).HasColumnName("MerchantDailySummaryID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.SummaryDate).HasColumnType("date");
            entity.Property(e => e.TotalTransactions).HasDefaultValue(0);
            entity.Property(e => e.TotalTransactionAmount).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.SuccessfulTransactions).HasDefaultValue(0);
            entity.Property(e => e.TotalMdrCharges).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.TotalGst).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.TotalDeductions).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.TotalSettledAmount).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.PendingSettlementAmount).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.TotalRefunds).HasDefaultValue(0);
            entity.Property(e => e.TotalRefundAmount).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.TotalChargebacks).HasDefaultValue(0);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.MerchantDailySummaries)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_MerchantDailySummaries_Merchants");
        });

        modelBuilder.Entity<MerchantColumnPreference>(entity =>
        {
            entity.HasKey(e => e.MerchantColumnPreferenceId).HasName("PK_MerchantColumnPreferences");

            entity.HasIndex(e => new { e.Mid, e.GridName }, "UQ_MerchantColumnPreferences_MID_Grid").IsUnique();

            entity.Property(e => e.MerchantColumnPreferenceId).HasColumnName("MerchantColumnPreferenceID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.GridName).HasMaxLength(100);
            entity.Property(e => e.SelectedColumns).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.MerchantColumnPreferences)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_MerchantColumnPreferences_Merchants");
        });

        modelBuilder.Entity<MerchantIpWhitelist>(entity =>
        {
            entity.HasKey(e => e.IpWhitelistId).HasName("PK_MerchantIpWhitelists");

            entity.HasIndex(e => new { e.Mid, e.IpAddress }, "UQ_MerchantIpWhitelists_MID_IP").IsUnique();
            entity.HasIndex(e => e.Mid, "IX_MerchantIpWhitelists_MID");

            entity.Property(e => e.IpWhitelistId).HasColumnName("IpWhitelistID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.IpAddress).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.IpWhitelists)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_MerchantIpWhitelists_Merchants");
        });

        modelBuilder.Entity<PaymentLinkBulkUpload>(entity =>
        {
            entity.HasKey(e => e.BulkUploadId).HasName("PK_PaymentLinkBulkUploads");
            entity.HasIndex(e => e.Mid, "IX_PaymentLinkBulkUploads_MID");
            entity.HasIndex(e => e.BatchReferenceId, "IX_PaymentLinkBulkUploads_BatchRef");

            entity.Property(e => e.BulkUploadId).HasColumnName("BulkUploadID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.BatchReferenceId).HasMaxLength(100);
            entity.Property(e => e.CreatorEmail).HasMaxLength(255);
            entity.Property(e => e.BatchDescription).HasMaxLength(500);
            entity.Property(e => e.FileName).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
            entity.Property(e => e.LinkCreated).HasDefaultValue(0);
            entity.Property(e => e.ActiveCount).HasDefaultValue(0);
            entity.Property(e => e.CustomerDataCapture);
            entity.Property(e => e.SendEmail).HasDefaultValue(false);
            entity.Property(e => e.SendSms).HasDefaultValue(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.PaymentLinkBulkUploads)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_PaymentLinkBulkUploads_Merchants");
        });

        modelBuilder.Entity<PaymentLinkBulkUploadFile>(entity =>
        {
            entity.HasKey(e => e.FileId).HasName("PK_PaymentLinkBulkUploadFiles");
            entity.HasIndex(e => e.Mid, "IX_PaymentLinkBulkUploadFiles_MID");

            entity.Property(e => e.FileId).HasColumnName("FileID");
            entity.Property(e => e.Mid).HasColumnName("MID");
            entity.Property(e => e.FileName).HasMaxLength(500);
            entity.Property(e => e.FilePath).HasMaxLength(1000);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MidNavigation).WithMany(p => p.PaymentLinkBulkUploadFiles)
                .HasForeignKey(d => d.Mid)
                .HasConstraintName("FK_PaymentLinkBulkUploadFiles_Merchants");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
