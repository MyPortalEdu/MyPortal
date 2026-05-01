-- Add ASP.NET Identity columns to dbo.Users / dbo.Roles and create the join /
-- claim tables. Idempotent: each statement guards against already-applied state
-- so the script is safe to re-run if the _DatabaseUpdates journal is ever lost.

-- dbo.Users columns
IF COL_LENGTH(N'dbo.Users', N'UserName') IS NULL
    ALTER TABLE dbo.Users ADD UserName NVARCHAR(256) NULL;

IF COL_LENGTH(N'dbo.Users', N'NormalizedUserName') IS NULL
    ALTER TABLE dbo.Users ADD NormalizedUserName NVARCHAR(256) NULL;

IF COL_LENGTH(N'dbo.Users', N'Email') IS NULL
    ALTER TABLE dbo.Users ADD Email NVARCHAR(256) NULL;

IF COL_LENGTH(N'dbo.Users', N'NormalizedEmail') IS NULL
    ALTER TABLE dbo.Users ADD NormalizedEmail NVARCHAR(256) NULL;

IF COL_LENGTH(N'dbo.Users', N'EmailConfirmed') IS NULL
    ALTER TABLE dbo.Users ADD EmailConfirmed BIT NOT NULL CONSTRAINT DF_Users_EmailConfirmed DEFAULT(0);

IF COL_LENGTH(N'dbo.Users', N'PasswordHash') IS NULL
    ALTER TABLE dbo.Users ADD PasswordHash NVARCHAR(MAX) NULL;

IF COL_LENGTH(N'dbo.Users', N'SecurityStamp') IS NULL
    ALTER TABLE dbo.Users ADD SecurityStamp NVARCHAR(MAX) NULL;

IF COL_LENGTH(N'dbo.Users', N'ConcurrencyStamp') IS NULL
    ALTER TABLE dbo.Users ADD ConcurrencyStamp NVARCHAR(MAX) NULL;

IF COL_LENGTH(N'dbo.Users', N'PhoneNumber') IS NULL
    ALTER TABLE dbo.Users ADD PhoneNumber NVARCHAR(50) NULL;

IF COL_LENGTH(N'dbo.Users', N'PhoneNumberConfirmed') IS NULL
    ALTER TABLE dbo.Users ADD PhoneNumberConfirmed BIT NOT NULL CONSTRAINT DF_Users_PhoneConfirmed DEFAULT(0);

IF COL_LENGTH(N'dbo.Users', N'TwoFactorEnabled') IS NULL
    ALTER TABLE dbo.Users ADD TwoFactorEnabled BIT NOT NULL CONSTRAINT DF_Users_TwoFactorEnabled DEFAULT(0);

IF COL_LENGTH(N'dbo.Users', N'LockoutEnd') IS NULL
    ALTER TABLE dbo.Users ADD LockoutEnd DATETIMEOFFSET(7) NULL;

IF COL_LENGTH(N'dbo.Users', N'LockoutEnabled') IS NULL
    ALTER TABLE dbo.Users ADD LockoutEnabled BIT NOT NULL CONSTRAINT DF_Users_LockoutEnabled DEFAULT(1);

IF COL_LENGTH(N'dbo.Users', N'AccessFailedCount') IS NULL
    ALTER TABLE dbo.Users ADD AccessFailedCount INT NOT NULL CONSTRAINT DF_Users_AccessFailedCount DEFAULT(0);

-- dbo.Roles columns
IF COL_LENGTH(N'dbo.Roles', N'Name') IS NULL
    ALTER TABLE dbo.Roles ADD Name NVARCHAR(256) NULL;

IF COL_LENGTH(N'dbo.Roles', N'NormalizedName') IS NULL
    ALTER TABLE dbo.Roles ADD NormalizedName NVARCHAR(256) NULL;

IF COL_LENGTH(N'dbo.Roles', N'ConcurrencyStamp') IS NULL
    ALTER TABLE dbo.Roles ADD ConcurrencyStamp NVARCHAR(MAX) NULL;

-- dbo.UserRoles
IF OBJECT_ID(N'dbo.UserRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserRoles
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_UserRoles PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        RoleId UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_UserRoles_UserRole UNIQUE (UserId, RoleId)
    );
END;

-- dbo.UserClaims
IF OBJECT_ID(N'dbo.UserClaims', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserClaims
    (
        Id UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT PK_UserClaims PRIMARY KEY,
        UserId UNIQUEIDENTIFIER NOT NULL,
        ClaimType NVARCHAR(256) NULL,
        ClaimValue NVARCHAR(MAX) NULL,
        CONSTRAINT FK_UserClaims_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
END;
