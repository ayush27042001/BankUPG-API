-- =========================================================================
-- Migration: Payment Gateway filter/column enhancements + bulk upload support
-- Date: 2026-07-23
-- =========================================================================

-- PaymentLink new columns
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaymentLinks') AND name = 'Purpose')
    ALTER TABLE PaymentLinks ADD Purpose NVARCHAR(200) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaymentLinks') AND name = 'DueDate')
    ALTER TABLE PaymentLinks ADD DueDate DATETIME NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaymentLinks') AND name = 'PaymentType')
    ALTER TABLE PaymentLinks ADD PaymentType NVARCHAR(20) NULL DEFAULT 'Standard';
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaymentLinks') AND name = 'IsPartialPayment')
    ALTER TABLE PaymentLinks ADD IsPartialPayment BIT NULL DEFAULT 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaymentLinks') AND name = 'MaxPaymentsAllowed')
    ALTER TABLE PaymentLinks ADD MaxPaymentsAllowed INT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaymentLinks') AND name = 'ValidationPeriod')
    ALTER TABLE PaymentLinks ADD ValidationPeriod INT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaymentLinks') AND name = 'TimeUnit')
    ALTER TABLE PaymentLinks ADD TimeUnit NVARCHAR(5) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaymentLinks') AND name = 'SendSms')
    ALTER TABLE PaymentLinks ADD SendSms BIT NULL DEFAULT 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaymentLinks') AND name = 'InvoiceId')
    ALTER TABLE PaymentLinks ADD InvoiceId NVARCHAR(100) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaymentLinks') AND name = 'TotalViews')
    ALTER TABLE PaymentLinks ADD TotalViews INT NOT NULL DEFAULT 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaymentLinks') AND name = 'TotalAmountPaid')
    ALTER TABLE PaymentLinks ADD TotalAmountPaid DECIMAL(18,2) NOT NULL DEFAULT 0;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('PaymentLinks') AND name = 'CustomerDataCapture')
    ALTER TABLE PaymentLinks ADD CustomerDataCapture NVARCHAR(MAX) NULL;
GO

-- Bulk upload tables
IF OBJECT_ID('PaymentLinkBulkUploads', 'U') IS NULL
BEGIN
    CREATE TABLE PaymentLinkBulkUploads (
        BulkUploadID BIGINT IDENTITY(1,1) NOT NULL,
        MID INT NOT NULL,
        BatchReferenceId NVARCHAR(100) NULL,
        CreatorEmail NVARCHAR(255) NULL,
        BatchDescription NVARCHAR(500) NULL,
        FileName NVARCHAR(500) NULL,
        LinkCreated INT NOT NULL DEFAULT 0,
        ActiveCount INT NOT NULL DEFAULT 0,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        SendEmail BIT NOT NULL DEFAULT 0,
        SendSms BIT NOT NULL DEFAULT 0,
        CustomerDataCapture NVARCHAR(MAX) NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_PaymentLinkBulkUploads PRIMARY KEY (BulkUploadID),
        CONSTRAINT FK_PaymentLinkBulkUploads_Merchants FOREIGN KEY (MID) REFERENCES Merchants(MID)
    );
    CREATE NONCLUSTERED INDEX IX_PaymentLinkBulkUploads_MID ON PaymentLinkBulkUploads(MID);
    CREATE NONCLUSTERED INDEX IX_PaymentLinkBulkUploads_BatchRef ON PaymentLinkBulkUploads(BatchReferenceId);
END
GO

IF OBJECT_ID('PaymentLinkBulkUploadFiles', 'U') IS NULL
BEGIN
    CREATE TABLE PaymentLinkBulkUploadFiles (
        FileID BIGINT IDENTITY(1,1) NOT NULL,
        MID INT NOT NULL,
        FileName NVARCHAR(500) NOT NULL,
        FilePath NVARCHAR(1000) NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active',
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_PaymentLinkBulkUploadFiles PRIMARY KEY (FileID),
        CONSTRAINT FK_PaymentLinkBulkUploadFiles_Merchants FOREIGN KEY (MID) REFERENCES Merchants(MID)
    );
    CREATE NONCLUSTERED INDEX IX_PaymentLinkBulkUploadFiles_MID ON PaymentLinkBulkUploadFiles(MID);
END
GO
