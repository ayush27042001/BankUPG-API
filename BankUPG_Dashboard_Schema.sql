-- MS SQL Database Schema for BankU User Dashboard operations
-- Compatible with existing BankUPG Merchant / User model

CREATE DATABASE BankUPG_Dashboard;
GO

USE BankUPG_Dashboard;
GO

-- ============================================================
-- 1. USERS & MERCHANTS (Login & Profile)
-- ============================================================

CREATE TABLE [dbo].[Users](
    [UserID] INT IDENTITY(1,1) NOT NULL,
    [Email] NVARCHAR(255) NOT NULL,
    [MobileNumber] NVARCHAR(20) NULL,
    [PasswordHash] NVARCHAR(512) NOT NULL,
    [Salt] NVARCHAR(256) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsEmailVerified] BIT NOT NULL DEFAULT 0,
    [IsMobileVerified] BIT NOT NULL DEFAULT 0,
    [FailedLoginAttempts] INT NOT NULL DEFAULT 0,
    [IsLocked] BIT NOT NULL DEFAULT 0,
    [LastLoginDate] DATETIME NULL,
    [PasswordLastChangedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([UserID]),
    CONSTRAINT [UQ_Users_Email] UNIQUE NONCLUSTERED ([Email])
);
GO

CREATE TABLE [dbo].[Merchants](
    [MID] INT IDENTITY(1,1) NOT NULL,
    [UserID] INT NOT NULL,
    [BusinessName] NVARCHAR(500) NULL,
    [BusinessEntityTypeID] INT NULL,
    [OnboardingStatusID] INT NOT NULL DEFAULT 1,
    [CKYCIdentifier] NVARCHAR(50) NULL,
    [CKYCConsentGiven] BIT NOT NULL DEFAULT 0,
    [CKYCConsentDate] DATETIME NULL,
    [ExpectedSalesPerMonth] DECIMAL(18,2) NULL,
    [HasGSTIN] BIT NOT NULL DEFAULT 0,
    [GSTIN] NVARCHAR(15) NULL,
    [BusinessCategoryID] INT NULL,
    [BusinessSubCategoryID] INT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsOnboardingCompleted] BIT NOT NULL DEFAULT 0,
    [IsOnboardingRejected] BIT NOT NULL DEFAULT 0,
    [LiveMode] BIT NOT NULL DEFAULT 1,
    [PaymentHandleCode] NVARCHAR(50) NULL,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Merchants] PRIMARY KEY CLUSTERED ([MID]),
    CONSTRAINT [FK_Merchants_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users]([UserID])
);
GO

CREATE UNIQUE INDEX [UQ_Merchants_PaymentHandleCode] ON [dbo].[Merchants]([PaymentHandleCode]) WHERE [PaymentHandleCode] IS NOT NULL;
GO

-- ============================================================
-- 2. DASHBOARD NAVIGATION / AUDIT
-- ============================================================

CREATE TABLE [dbo].[LoginAuditLog](
    [LoginAuditLogID] BIGINT IDENTITY(1,1) NOT NULL,
    [UserID] INT NOT NULL,
    [MID] INT NULL,
    [LoginStatus] NVARCHAR(50) NOT NULL,
    [LoginIPAddress] NVARCHAR(50) NULL,
    [UserAgent] NVARCHAR(500) NULL,
    [LoginAttemptDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_LoginAuditLog] PRIMARY KEY CLUSTERED ([LoginAuditLogID]),
    CONSTRAINT [FK_LoginAuditLog_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users]([UserID]),
    CONSTRAINT [FK_LoginAuditLog_Merchants] FOREIGN KEY ([MID]) REFERENCES [dbo].[Merchants]([MID])
);
GO

CREATE INDEX [IX_LoginAuditLog_UserID] ON [dbo].[LoginAuditLog]([UserID]);
CREATE INDEX [IX_LoginAuditLog_MID] ON [dbo].[LoginAuditLog]([MID]);
CREATE INDEX [IX_LoginAuditLog_LoginStatus] ON [dbo].[LoginAuditLog]([LoginStatus]);
GO

-- ============================================================
-- 3. INTEGRATION OPTIONS
-- ============================================================

CREATE TABLE [dbo].[IntegrationOptions](
    [IntegrationOptionID] INT IDENTITY(1,1) NOT NULL,
    [OptionCode] NVARCHAR(50) NOT NULL,
    [OptionName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [EstimatedDuration] NVARCHAR(100) NULL,
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_IntegrationOptions] PRIMARY KEY CLUSTERED ([IntegrationOptionID]),
    CONSTRAINT [UQ_IntegrationOptions_OptionCode] UNIQUE NONCLUSTERED ([OptionCode])
);
GO

CREATE TABLE [dbo].[MerchantIntegration](
    [MerchantIntegrationID] INT IDENTITY(1,1) NOT NULL,
    [MID] INT NOT NULL,
    [IntegrationOptionID] INT NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    [StartedDate] DATETIME NULL,
    [CompletedDate] DATETIME NULL,
    [ConfigurationDetails] NVARCHAR(MAX) NULL,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_MerchantIntegration] PRIMARY KEY CLUSTERED ([MerchantIntegrationID]),
    CONSTRAINT [FK_MerchantIntegration_Merchants] FOREIGN KEY ([MID]) REFERENCES [dbo].[Merchants]([MID]),
    CONSTRAINT [FK_MerchantIntegration_IntegrationOptions] FOREIGN KEY ([IntegrationOptionID]) REFERENCES [dbo].[IntegrationOptions]([IntegrationOptionID])
);
GO

CREATE UNIQUE INDEX [UQ_MerchantIntegration_MID_OptionID] ON [dbo].[MerchantIntegration]([MID], [IntegrationOptionID]);
GO

-- ============================================================
-- 4. PAYMENT MODES
-- ============================================================

CREATE TABLE [dbo].[PaymentModes](
    [PaymentModeID] INT IDENTITY(1,1) NOT NULL,
    [ModeCode] NVARCHAR(50) NOT NULL,
    [ModeName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [IconUrl] NVARCHAR(500) NULL,
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_PaymentModes] PRIMARY KEY CLUSTERED ([PaymentModeID]),
    CONSTRAINT [UQ_PaymentModes_ModeCode] UNIQUE NONCLUSTERED ([ModeCode])
);
GO

CREATE TABLE [dbo].[MerchantPaymentModes](
    [MerchantPaymentModeID] INT IDENTITY(1,1) NOT NULL,
    [MID] INT NOT NULL,
    [PaymentModeID] INT NOT NULL,
    [IsActivated] BIT NOT NULL DEFAULT 0,
    [ActivatedDate] DATETIME NULL,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_MerchantPaymentModes] PRIMARY KEY CLUSTERED ([MerchantPaymentModeID]),
    CONSTRAINT [FK_MerchantPaymentModes_Merchants] FOREIGN KEY ([MID]) REFERENCES [dbo].[Merchants]([MID]),
    CONSTRAINT [FK_MerchantPaymentModes_PaymentModes] FOREIGN KEY ([PaymentModeID]) REFERENCES [dbo].[PaymentModes]([PaymentModeID])
);
GO

CREATE UNIQUE INDEX [UQ_MerchantPaymentModes_MID_ModeID] ON [dbo].[MerchantPaymentModes]([MID], [PaymentModeID]);
CREATE INDEX [IX_MerchantPaymentModes_IsActivated] ON [dbo].[MerchantPaymentModes]([IsActivated]);
GO

-- ============================================================
-- 5. PAYMENT HANDLE / PAYMENT LINKS
-- ============================================================

CREATE TABLE [dbo].[PaymentHandles](
    [PaymentHandleID] INT IDENTITY(1,1) NOT NULL,
    [MID] INT NOT NULL,
    [HandleCode] NVARCHAR(50) NOT NULL,
    [HandleUrl] NVARCHAR(500) NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [UsageCount] INT NOT NULL DEFAULT 0,
    [LastUsedDate] DATETIME NULL,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_PaymentHandles] PRIMARY KEY CLUSTERED ([PaymentHandleID]),
    CONSTRAINT [UQ_PaymentHandles_HandleCode] UNIQUE NONCLUSTERED ([HandleCode]),
    CONSTRAINT [FK_PaymentHandles_Merchants] FOREIGN KEY ([MID]) REFERENCES [dbo].[Merchants]([MID])
);
GO

CREATE INDEX [IX_PaymentHandles_MID] ON [dbo].[PaymentHandles]([MID]);
GO

-- ============================================================
-- 6. PAYMENTS / TRANSACTIONS
-- ============================================================

CREATE TABLE [dbo].[Transactions](
    [TransactionID] BIGINT IDENTITY(1,1) NOT NULL,
    [MID] INT NOT NULL,
    [OrderID] NVARCHAR(100) NOT NULL,
    [TransactionReference] NVARCHAR(100) NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [Currency] NVARCHAR(3) NOT NULL DEFAULT 'INR',
    [PaymentModeID] INT NULL,
    [Status] NVARCHAR(50) NOT NULL,
    [CustomerName] NVARCHAR(200) NULL,
    [CustomerEmail] NVARCHAR(255) NULL,
    [CustomerMobile] NVARCHAR(20) NULL,
    [PaymentHandleID] INT NULL,
    [TransactionDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [SettlementDate] DATETIME NULL,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Transactions] PRIMARY KEY CLUSTERED ([TransactionID]),
    CONSTRAINT [FK_Transactions_Merchants] FOREIGN KEY ([MID]) REFERENCES [dbo].[Merchants]([MID]),
    CONSTRAINT [FK_Transactions_PaymentModes] FOREIGN KEY ([PaymentModeID]) REFERENCES [dbo].[PaymentModes]([PaymentModeID])
);
GO

CREATE INDEX [IX_Transactions_MID] ON [dbo].[Transactions]([MID]);
CREATE INDEX [IX_Transactions_OrderID] ON [dbo].[Transactions]([OrderID]);
CREATE INDEX [IX_Transactions_TransactionDate] ON [dbo].[Transactions]([TransactionDate]);
CREATE INDEX [IX_Transactions_Status] ON [dbo].[Transactions]([Status]);
GO

-- ============================================================
-- 7. SETTLEMENTS
-- ============================================================

CREATE TABLE [dbo].[Settlements](
    [SettlementID] BIGINT IDENTITY(1,1) NOT NULL,
    [MID] INT NOT NULL,
    [SettlementReference] NVARCHAR(100) NOT NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL,
    [SettlementDate] DATETIME NULL,
    [UTRNumber] NVARCHAR(100) NULL,
    [BankAccountNumber] NVARCHAR(50) NULL,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Settlements] PRIMARY KEY CLUSTERED ([SettlementID]),
    CONSTRAINT [FK_Settlements_Merchants] FOREIGN KEY ([MID]) REFERENCES [dbo].[Merchants]([MID])
);
GO

CREATE INDEX [IX_Settlements_MID] ON [dbo].[Settlements]([MID]);
CREATE INDEX [IX_Settlements_SettlementDate] ON [dbo].[Settlements]([SettlementDate]);
CREATE INDEX [IX_Settlements_Status] ON [dbo].[Settlements]([Status]);
GO

-- ============================================================
-- 8. CHARGEBACKS
-- ============================================================

CREATE TABLE [dbo].[Chargebacks](
    [ChargebackID] BIGINT IDENTITY(1,1) NOT NULL,
    [TransactionID] BIGINT NOT NULL,
    [MID] INT NOT NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [Reason] NVARCHAR(500) NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Open',
    [RaisedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [ResolvedDate] DATETIME NULL,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Chargebacks] PRIMARY KEY CLUSTERED ([ChargebackID]),
    CONSTRAINT [FK_Chargebacks_Transactions] FOREIGN KEY ([TransactionID]) REFERENCES [dbo].[Transactions]([TransactionID]),
    CONSTRAINT [FK_Chargebacks_Merchants] FOREIGN KEY ([MID]) REFERENCES [dbo].[Merchants]([MID])
);
GO

CREATE INDEX [IX_Chargebacks_MID] ON [dbo].[Chargebacks]([MID]);
CREATE INDEX [IX_Chargebacks_TransactionID] ON [dbo].[Chargebacks]([TransactionID]);
CREATE INDEX [IX_Chargebacks_Status] ON [dbo].[Chargebacks]([Status]);
GO

-- ============================================================
-- 9. PAYOUTS
-- ============================================================

CREATE TABLE [dbo].[Payouts](
    [PayoutID] BIGINT IDENTITY(1,1) NOT NULL,
    [MID] INT NOT NULL,
    [BeneficiaryName] NVARCHAR(200) NOT NULL,
    [BeneficiaryAccountNumber] NVARCHAR(50) NOT NULL,
    [BeneficiaryIFSC] NVARCHAR(20) NOT NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL,
    [UTRNumber] NVARCHAR(100) NULL,
    [PayoutDate] DATETIME NULL,
    [Remarks] NVARCHAR(500) NULL,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Payouts] PRIMARY KEY CLUSTERED ([PayoutID]),
    CONSTRAINT [FK_Payouts_Merchants] FOREIGN KEY ([MID]) REFERENCES [dbo].[Merchants]([MID])
);
GO

CREATE INDEX [IX_Payouts_MID] ON [dbo].[Payouts]([MID]);
CREATE INDEX [IX_Payouts_PayoutDate] ON [dbo].[Payouts]([PayoutDate]);
CREATE INDEX [IX_Payouts_Status] ON [dbo].[Payouts]([Status]);
GO

-- ============================================================
-- 10. REPORTS
-- ============================================================

CREATE TABLE [dbo].[ReportTypes](
    [ReportTypeID] INT IDENTITY(1,1) NOT NULL,
    [TypeCode] NVARCHAR(50) NOT NULL,
    [TypeName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_ReportTypes] PRIMARY KEY CLUSTERED ([ReportTypeID]),
    CONSTRAINT [UQ_ReportTypes_TypeCode] UNIQUE NONCLUSTERED ([TypeCode])
);
GO

CREATE TABLE [dbo].[Reports](
    [ReportID] BIGINT IDENTITY(1,1) NOT NULL,
    [MID] INT NOT NULL,
    [ReportTypeID] INT NOT NULL,
    [FromDate] DATETIME NOT NULL,
    [ToDate] DATETIME NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Requested',
    [FilePath] NVARCHAR(1000) NULL,
    [GeneratedDate] DATETIME NULL,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Reports] PRIMARY KEY CLUSTERED ([ReportID]),
    CONSTRAINT [FK_Reports_Merchants] FOREIGN KEY ([MID]) REFERENCES [dbo].[Merchants]([MID]),
    CONSTRAINT [FK_Reports_ReportTypes] FOREIGN KEY ([ReportTypeID]) REFERENCES [dbo].[ReportTypes]([ReportTypeID])
);
GO

CREATE INDEX [IX_Reports_MID] ON [dbo].[Reports]([MID]);
CREATE INDEX [IX_Reports_ReportTypeID] ON [dbo].[Reports]([ReportTypeID]);
CREATE INDEX [IX_Reports_Status] ON [dbo].[Reports]([Status]);
GO

-- ============================================================
-- 11. PRODUCTS & CHAT SUPPORT
-- ============================================================

CREATE TABLE [dbo].[Products](
    [ProductID] INT IDENTITY(1,1) NOT NULL,
    [ProductCode] NVARCHAR(50) NOT NULL,
    [ProductName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500) NULL,
    [IconUrl] NVARCHAR(500) NULL,
    [RedirectUrl] NVARCHAR(500) NULL,
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [UpdatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([ProductID]),
    CONSTRAINT [UQ_Products_ProductCode] UNIQUE NONCLUSTERED ([ProductCode])
);
GO

CREATE TABLE [dbo].[ChatSupportTickets](
    [TicketID] BIGINT IDENTITY(1,1) NOT NULL,
    [UserID] INT NOT NULL,
    [MID] INT NULL,
    [Subject] NVARCHAR(200) NULL,
    [Message] NVARCHAR(MAX) NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Open',
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [ResolvedDate] DATETIME NULL,
    [AssignedTo] NVARCHAR(100) NULL,
    CONSTRAINT [PK_ChatSupportTickets] PRIMARY KEY CLUSTERED ([TicketID]),
    CONSTRAINT [FK_ChatSupportTickets_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users]([UserID]),
    CONSTRAINT [FK_ChatSupportTickets_Merchants] FOREIGN KEY ([MID]) REFERENCES [dbo].[Merchants]([MID])
);
GO

CREATE INDEX [IX_ChatSupportTickets_UserID] ON [dbo].[ChatSupportTickets]([UserID]);
CREATE INDEX [IX_ChatSupportTickets_MID] ON [dbo].[ChatSupportTickets]([MID]);
CREATE INDEX [IX_ChatSupportTickets_Status] ON [dbo].[ChatSupportTickets]([Status]);
GO

-- ============================================================
-- 12. NOTIFICATIONS
-- ============================================================

CREATE TABLE [dbo].[Notifications](
    [NotificationID] BIGINT IDENTITY(1,1) NOT NULL,
    [UserID] INT NOT NULL,
    [Title] NVARCHAR(200) NOT NULL,
    [Message] NVARCHAR(MAX) NULL,
    [IsRead] BIT NOT NULL DEFAULT 0,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED ([NotificationID]),
    CONSTRAINT [FK_Notifications_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users]([UserID])
);
GO

CREATE INDEX [IX_Notifications_UserID] ON [dbo].[Notifications]([UserID]);
CREATE INDEX [IX_Notifications_IsRead] ON [dbo].[Notifications]([IsRead]);
GO

-- ============================================================
-- SEED DATA
-- ============================================================

-- Payment Modes shown on dashboard
INSERT INTO [dbo].[PaymentModes] ([ModeCode], [ModeName], [Description], [DisplayOrder])
VALUES
('EMI', 'EMI', 'Equated Monthly Installment payments', 1),
('NET_BANKING', 'Net Banking', 'Internet banking payments', 2),
('CREDIT_CARD', 'Credit Card', 'Credit card payments', 3),
('DEBIT_CARD', 'Debit Card', 'Debit card payments', 4),
('CASH_CARD', 'Cash Card', 'Cash card / wallet payments', 5);
GO

-- Integration options shown on dashboard
INSERT INTO [dbo].[IntegrationOptions] ([OptionCode], [OptionName], [Description], [EstimatedDuration], [DisplayOrder])
VALUES
('BANKU_HOSTED', 'BankU Hosted', 'Use BankU hosted checkout page for quick integration.', '1-2 Days', 1),
('MERCHANT_HOSTED', 'Merchant Hosted', 'Integrate APIs on your own server.', '2-3 Days', 2),
('PLUGINS', 'Integrate with Plugins', 'Use plugins for supported platforms.', 'In Mins', 3);
GO

-- Report types for Reports section
INSERT INTO [dbo].[ReportTypes] ([TypeCode], [TypeName], [Description])
VALUES
('TRANSACTION', 'Transaction Report', 'Detailed transaction history'),
('SETTLEMENT', 'Settlement Report', 'Settlement summary and details'),
('CHARGEBACK', 'Chargeback Report', 'Chargeback and dispute report'),
('PAYOUT', 'Payout Report', 'Payout transaction report');
GO
