using System;
using System.Collections.Generic;
using BankUPG.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=103.205.142.34,1433;Initial Catalog=BankuPG;Persist Security Info=True;User ID=sa;Password=zUG93NOh6WE7BQIS;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
