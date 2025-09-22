ALTER TABLE dbo.Users ADD
    UserName NVARCHAR(256) NULL,
    NormalizedUserName NVARCHAR(256) NULL,
    Email NVARCHAR(256) NULL,
    NormalizedEmail NVARCHAR(256) NULL,
    EmailConfirmed BIT NOT NULL CONSTRAINT DF_Users_EmailConfirmed DEFAULT(0),
    PasswordHash NVARCHAR(MAX) NULL,
    SecurityStamp NVARCHAR(MAX) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    PhoneNumber NVARCHAR(50) NULL,
    PhoneNumberConfirmed BIT NOT NULL CONSTRAINT DF_Users_PhoneConfirmed DEFAULT(0),
    TwoFactorEnabled BIT NOT NULL CONSTRAINT DF_Users_TwoFactorEnabled DEFAULT(0),
    LockoutEnd DATETIMEOFFSET(7) NULL,
    LockoutEnabled BIT NOT NULL CONSTRAINT DF_Users_LockoutEnabled DEFAULT(1),
    AccessFailedCount INT NOT NULL CONSTRAINT DF_Users_AccessFailedCount DEFAULT(0);

ALTER TABLE dbo.Roles ADD
    Name NVARCHAR(256) NULL,
    NormalizedName NVARCHAR(256) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL;

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

CREATE TABLE dbo.UserClaims
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_UserClaims PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    ClaimType NVARCHAR(256) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_UserClaims_User FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
);