------------------------------------------------------------
-- 1. CREATE DATABASE (if not exists)
------------------------------------------------------------
IF DB_ID('DigitalPrizeDB') IS NULL
BEGIN
    CREATE DATABASE DigitalPrizeDB;
END
GO

USE DigitalPrizeDB;
GO

------------------------------------------------------------
-- 2. EXTERNAL USERS (PUBLIC USERS)
--    Identified primarily by CellNumber
------------------------------------------------------------
CREATE TABLE dbo.ExternalUsers
(
    ExternalUserId  BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CellNumber      NVARCHAR(20) NOT NULL,
    FirstName       NVARCHAR(100) NULL,
    LastName        NVARCHAR(100) NULL,
    Email           NVARCHAR(256) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_ExternalUsers_IsActive DEFAULT (1),
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_ExternalUsers_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt       DATETIME2(0) NULL,

    CONSTRAINT UQ_ExternalUsers_Cell UNIQUE (CellNumber)
);
GO

CREATE INDEX IX_ExternalUsers_CellNumber
    ON dbo.ExternalUsers (CellNumber);
GO

------------------------------------------------------------
-- 3. COMPETITIONS & REGISTRATION FORMS
------------------------------------------------------------
CREATE TABLE dbo.Competitions
(
    CompetitionId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name            NVARCHAR(200) NOT NULL,
    Description     NVARCHAR(MAX) NULL,
    TermsUrl        NVARCHAR(500) NULL,
    StartDate       DATETIME2(0) NOT NULL,
    EndDate         DATETIME2(0) NOT NULL,
    BannerImageUrl  NVARCHAR(500) NULL,
    IsActive        BIT NOT NULL CONSTRAINT DF_Competitions_IsActive DEFAULT (1),
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_Competitions_CreatedAt DEFAULT (SYSUTCDATETIME()),
    UpdatedAt       DATETIME2(0) NULL
);
GO

-- Registration field definitions (form builder)
CREATE TABLE dbo.RegistrationFields
(
    RegistrationFieldId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CompetitionId       INT NOT NULL,
    FieldName           NVARCHAR(100) NOT NULL,  -- internal name, e.g. "FirstName"
    Label               NVARCHAR(200) NOT NULL,  -- label shown to user
    FieldType           NVARCHAR(50) NOT NULL,   -- e.g. "Text", "Email", "Dropdown", "Checkbox"
    IsRequired          BIT NOT NULL CONSTRAINT DF_RegistrationFields_IsRequired DEFAULT (0),
    SortOrder           INT NOT NULL CONSTRAINT DF_RegistrationFields_SortOrder DEFAULT (0),
    OptionsJson         NVARCHAR(MAX) NULL,      -- JSON for dropdown options, etc.
    IsActive            BIT NOT NULL CONSTRAINT DF_RegistrationFields_IsActive DEFAULT (1),

    CONSTRAINT FK_RegistrationFields_Competitions FOREIGN KEY (CompetitionId)
        REFERENCES dbo.Competitions (CompetitionId)
        ON DELETE CASCADE
);
GO

-- Each registration submission
CREATE TABLE dbo.Registrations
(
    RegistrationId      BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CompetitionId       INT NOT NULL,
    ExternalUserId      BIGINT NULL,
    CellNumber          NVARCHAR(20) NOT NULL,
    RegistrationDate    DATETIME2(0) NOT NULL CONSTRAINT DF_Registrations_RegistrationDate DEFAULT (SYSUTCDATETIME()),
    Source              NVARCHAR(50) NULL,     -- "Web", "Import", etc.
    ConsentGiven        BIT NOT NULL CONSTRAINT DF_Registrations_ConsentGiven DEFAULT (0),

    CONSTRAINT FK_Registrations_Competitions FOREIGN KEY (CompetitionId)
        REFERENCES dbo.Competitions (CompetitionId)
        ON DELETE CASCADE,

    CONSTRAINT FK_Registrations_ExternalUsers FOREIGN KEY (ExternalUserId)
        REFERENCES dbo.ExternalUsers (ExternalUserId)
        ON DELETE SET NULL
);
GO

CREATE INDEX IX_Registrations_Competition
    ON dbo.Registrations (CompetitionId, RegistrationDate);
GO

CREATE INDEX IX_Registrations_CellNumber
    ON dbo.Registrations (CellNumber, CompetitionId);
GO

-- Answers per form field
CREATE TABLE dbo.RegistrationAnswers
(
    RegistrationAnswerId BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RegistrationId       BIGINT NOT NULL,
    RegistrationFieldId  INT NOT NULL,
    Value                NVARCHAR(MAX) NULL,

    CONSTRAINT FK_RegistrationAnswers_Registrations FOREIGN KEY (RegistrationId)
        REFERENCES dbo.Registrations (RegistrationId)
        ON DELETE CASCADE,

    CONSTRAINT FK_RegistrationAnswers_RegistrationFields FOREIGN KEY (RegistrationFieldId)
        REFERENCES dbo.RegistrationFields (RegistrationFieldId)
        ON DELETE CASCADE
);
GO

------------------------------------------------------------
-- 4. PRIZE TYPES, POOLS, AND PRIZES
------------------------------------------------------------
CREATE TABLE dbo.PrizeTypes
(
    PrizeTypeId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name          NVARCHAR(100) NOT NULL UNIQUE, -- "VoucherCode", "QRCode", "UrlLink", "DiscountCode", "WalletCredit"
    Description   NVARCHAR(500) NULL
);
GO

CREATE TABLE dbo.PrizePools
(
    PrizePoolId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name          NVARCHAR(200) NOT NULL,
    Description   NVARCHAR(MAX) NULL,
    IsActive      BIT NOT NULL CONSTRAINT DF_PrizePools_IsActive DEFAULT (1),
    CreatedAt     DATETIME2(0) NOT NULL CONSTRAINT DF_PrizePools_CreatedAt DEFAULT (SYSUTCDATETIME())
);
GO

-- Logical prize definition
CREATE TABLE dbo.Prizes
(
    PrizeId           BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PrizePoolId       INT NOT NULL,
    PrizeTypeId       INT NOT NULL,
    Name              NVARCHAR(200) NOT NULL,
    Description       NVARCHAR(MAX) NULL,
    MonetaryValue     DECIMAL(18,2) NULL,     -- if applicable
    TotalQuantity     INT NOT NULL,
    RemainingQuantity INT NOT NULL,
    ExpiryDate        DATETIME2(0) NULL,
    ImageUrl          NVARCHAR(500) NULL,
    MetadataJson      NVARCHAR(MAX) NULL,     -- provider codes, URL templates, etc.
    IsActive          BIT NOT NULL CONSTRAINT DF_Prizes_IsActive DEFAULT (1),
    CreatedAt         DATETIME2(0) NOT NULL CONSTRAINT DF_Prizes_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT FK_Prizes_PrizePools FOREIGN KEY (PrizePoolId)
        REFERENCES dbo.PrizePools (PrizePoolId)
        ON DELETE CASCADE,

    CONSTRAINT FK_Prizes_PrizeTypes FOREIGN KEY (PrizeTypeId)
        REFERENCES dbo.PrizeTypes (PrizeTypeId)
);
GO

CREATE INDEX IX_Prizes_PrizePool
    ON dbo.Prizes (PrizePoolId);
GO

------------------------------------------------------------
-- 5. PRIZE AWARDS & REDEMPTIONS
------------------------------------------------------------
-- One row per prize assigned to a cell number
CREATE TABLE dbo.PrizeAwards
(
    PrizeAwardId         BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PrizeId              BIGINT NOT NULL,
    CompetitionId        INT NULL,
    ExternalUserId       BIGINT NULL,
    CellNumber           NVARCHAR(20) NOT NULL,
    AwardedAt            DATETIME2(0) NOT NULL CONSTRAINT DF_PrizeAwards_AwardedAt DEFAULT (SYSUTCDATETIME()),
    AwardedBySubjectId   NVARCHAR(200) NULL, -- Admin subject/ID from OAuth2 server
    AwardMethod          NVARCHAR(50) NULL,  -- "Manual", "Bulk", "Auto"
    NotificationChannel  NVARCHAR(50) NULL,  -- "SMS", "WhatsApp", "Email"
    NotificationStatus   NVARCHAR(50) NULL,  -- "Pending", "Sent", "Failed"
    Status               NVARCHAR(50) NOT NULL CONSTRAINT DF_PrizeAwards_Status DEFAULT ('Awarded'), -- "Awarded", "Redeemed", "Expired", "Cancelled"
    ExpiryDate           DATETIME2(0) NULL,
    ExternalReference    NVARCHAR(200) NULL, -- wallet ref, 3rd party ref

    CONSTRAINT FK_PrizeAwards_Prizes FOREIGN KEY (PrizeId)
        REFERENCES dbo.Prizes (PrizeId)
        ON DELETE CASCADE,

    CONSTRAINT FK_PrizeAwards_Competitions FOREIGN KEY (CompetitionId)
        REFERENCES dbo.Competitions (CompetitionId)
        ON DELETE SET NULL,

    CONSTRAINT FK_PrizeAwards_ExternalUsers FOREIGN KEY (ExternalUserId)
        REFERENCES dbo.ExternalUsers (ExternalUserId)
        ON DELETE SET NULL
);
GO

CREATE INDEX IX_PrizeAwards_CellNumber
    ON dbo.PrizeAwards (CellNumber);
GO

CREATE INDEX IX_PrizeAwards_PrizeId_Status
    ON dbo.PrizeAwards (PrizeId, Status);
GO

-- Redemption events (one prize may have 0 or 1 successful redemption)
CREATE TABLE dbo.PrizeRedemptions
(
    PrizeRedemptionId BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    PrizeAwardId      BIGINT NOT NULL,
    RedeemedAt        DATETIME2(0) NOT NULL CONSTRAINT DF_PrizeRedemptions_RedeemedAt DEFAULT (SYSUTCDATETIME()),
    RedeemedChannel   NVARCHAR(50) NULL, -- "Web", "API"
    RedeemedFromIp    NVARCHAR(50) NULL,
    RedemptionStatus  NVARCHAR(50) NOT NULL CONSTRAINT DF_PrizeRedemptions_Status DEFAULT ('Success'), -- "Success", "Failed"
    Notes             NVARCHAR(MAX) NULL,

    CONSTRAINT FK_PrizeRedemptions_PrizeAwards FOREIGN KEY (PrizeAwardId)
        REFERENCES dbo.PrizeAwards (PrizeAwardId)
        ON DELETE CASCADE
);
GO

------------------------------------------------------------
-- 6. OTP & COMMUNICATION LOGS
------------------------------------------------------------
CREATE TABLE dbo.Otps
(
    OtpId        BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CellNumber   NVARCHAR(20) NOT NULL,
    Code         NVARCHAR(10) NOT NULL,
    Purpose      NVARCHAR(50) NOT NULL,  -- "Registration", "Redemption"
    ExpiresAt    DATETIME2(0) NOT NULL,
    ConsumedAt   DATETIME2(0) NULL,
    IsValid      BIT NOT NULL CONSTRAINT DF_Otps_IsValid DEFAULT (1),
    CreatedAt    DATETIME2(0) NOT NULL CONSTRAINT DF_Otps_CreatedAt DEFAULT (SYSUTCDATETIME())
);
GO

CREATE INDEX IX_Otps_Cell_Purpose
    ON dbo.Otps (CellNumber, Purpose, ExpiresAt);
GO

CREATE TABLE dbo.SmsMessages
(
    SmsMessageId      BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    CellNumber        NVARCHAR(20) NOT NULL,
    MessageType       NVARCHAR(50) NOT NULL, -- "OTP", "PrizeNotification", etc.
    MessageText       NVARCHAR(MAX) NOT NULL,
    SentAt            DATETIME2(0) NULL,
    ProviderMessageId NVARCHAR(200) NULL,
    Status            NVARCHAR(50) NOT NULL CONSTRAINT DF_SmsMessages_Status DEFAULT ('Pending'), -- "Pending", "Sent", "Failed"
    ErrorMessage      NVARCHAR(MAX) NULL,
    CreatedAt         DATETIME2(0) NOT NULL CONSTRAINT DF_SmsMessages_CreatedAt DEFAULT (SYSUTCDATETIME())
);
GO

CREATE INDEX IX_SmsMessages_Cell_MessageType
    ON dbo.SmsMessages (CellNumber, MessageType, CreatedAt);
GO

------------------------------------------------------------
-- 7. AUDIT LOG
--    Admin identities are from OAuth2, stored as Subject/ID string
------------------------------------------------------------
CREATE TABLE dbo.AuditLog
(
    AuditLogId      BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    AdminSubjectId  NVARCHAR(200) NULL,  -- subject/id from OAuth2 server (admin)
    ExternalUserId  BIGINT NULL,
    EventType       NVARCHAR(100) NOT NULL,  -- "PrizeCreated", "PrizeAwarded", "PrizeRedeemed", etc.
    EntityType      NVARCHAR(100) NULL,      -- "Prize", "Competition", "ExternalUser", etc.
    EntityId        NVARCHAR(100) NULL,
    Description     NVARCHAR(MAX) NULL,
    CreatedAt       DATETIME2(0) NOT NULL CONSTRAINT DF_AuditLog_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedFromIp   NVARCHAR(50) NULL,

    CONSTRAINT FK_AuditLog_ExternalUsers FOREIGN KEY (ExternalUserId)
        REFERENCES dbo.ExternalUsers (ExternalUserId)
        ON DELETE SET NULL
);
GO

CREATE INDEX IX_AuditLog_EventType
    ON dbo.AuditLog (EventType, CreatedAt);
GO

------------------------------------------------------------
-- DONE
------------------------------------------------------------
PRINT 'DigitalPrizeDB (single-tenant, external-users-only) schema created successfully.';
