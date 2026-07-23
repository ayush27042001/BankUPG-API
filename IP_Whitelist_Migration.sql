-- =========================================================================
-- Migration: IP Whitelist + Merchant Whitelist Flag
-- Date: 2026-07-23
-- Description: Adds IP whitelisting support for merchant transaction APIs
-- =========================================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Merchants') AND name = 'IpWhitelistEnabled')
BEGIN
    ALTER TABLE Merchants ADD IpWhitelistEnabled BIT NOT NULL DEFAULT 0;
END
GO

IF OBJECT_ID('MerchantIpWhitelists', 'U') IS NULL
BEGIN
    CREATE TABLE MerchantIpWhitelists (
        IpWhitelistID INT IDENTITY(1,1) NOT NULL,
        MID INT NOT NULL,
        IpAddress NVARCHAR(50) NOT NULL,
        Description NVARCHAR(300) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_MerchantIpWhitelists PRIMARY KEY (IpWhitelistID),
        CONSTRAINT FK_MerchantIpWhitelists_Merchants FOREIGN KEY (MID) REFERENCES Merchants(MID),
        CONSTRAINT UQ_MerchantIpWhitelists_MID_IP UNIQUE (MID, IpAddress)
    );

    CREATE NONCLUSTERED INDEX IX_MerchantIpWhitelists_MID ON MerchantIpWhitelists(MID);
END
GO
