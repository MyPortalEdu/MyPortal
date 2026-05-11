-- OpenIddict EF Core entity tables. Idempotent: each table and index is guarded
-- so re-running the script after a journal loss does not error out. Schema-qualified
-- (dbo.) for clarity rather than relying on the connection's default schema.

IF OBJECT_ID(N'dbo.ApiApplications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApiApplications (
        [Id] uniqueidentifier NOT NULL,
        [ApplicationType] nvarchar(50) NULL,
        [ClientId] nvarchar(100) NULL,
        [ClientSecret] nvarchar(max) NULL,
        [ClientType] nvarchar(50) NULL,
        [ConcurrencyToken] nvarchar(50) NULL,
        [ConsentType] nvarchar(50) NULL,
        [DisplayName] nvarchar(max) NULL,
        [DisplayNames] nvarchar(max) NULL,
        [JsonWebKeySet] nvarchar(max) NULL,
        [Permissions] nvarchar(max) NULL,
        [PostLogoutRedirectUris] nvarchar(max) NULL,
        [Properties] nvarchar(max) NULL,
        [RedirectUris] nvarchar(max) NULL,
        [Requirements] nvarchar(max) NULL,
        [Settings] nvarchar(max) NULL,
        CONSTRAINT [PK_ApiApplications] PRIMARY KEY ([Id])
    );
END;

IF OBJECT_ID(N'dbo.ApiScopes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApiScopes (
        [Id] uniqueidentifier NOT NULL,
        [ConcurrencyToken] nvarchar(50) NULL,
        [Description] nvarchar(max) NULL,
        [Descriptions] nvarchar(max) NULL,
        [DisplayName] nvarchar(max) NULL,
        [DisplayNames] nvarchar(max) NULL,
        [Name] nvarchar(200) NULL,
        [Properties] nvarchar(max) NULL,
        [Resources] nvarchar(max) NULL,
        CONSTRAINT [PK_ApiScopes] PRIMARY KEY ([Id])
    );
END;

IF OBJECT_ID(N'dbo.ApiAuthorizations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApiAuthorizations (
        [Id] uniqueidentifier NOT NULL,
        [ApplicationId] uniqueidentifier NULL,
        [ConcurrencyToken] nvarchar(50) NULL,
        [CreationDate] datetime2 NULL,
        [Properties] nvarchar(max) NULL,
        [Scopes] nvarchar(max) NULL,
        [Status] nvarchar(50) NULL,
        [Subject] nvarchar(400) NULL,
        [Type] nvarchar(50) NULL,
        CONSTRAINT [PK_ApiAuthorizations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ApiAuthorizations_ApiApplications_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES dbo.ApiApplications ([Id])
    );
END;

IF OBJECT_ID(N'dbo.ApiTokens', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApiTokens (
        [Id] uniqueidentifier NOT NULL,
        [ApplicationId] uniqueidentifier NULL,
        [AuthorizationId] uniqueidentifier NULL,
        [ConcurrencyToken] nvarchar(50) NULL,
        [CreationDate] datetime2 NULL,
        [ExpirationDate] datetime2 NULL,
        [Payload] nvarchar(max) NULL,
        [Properties] nvarchar(max) NULL,
        [RedemptionDate] datetime2 NULL,
        [ReferenceId] nvarchar(100) NULL,
        [Status] nvarchar(50) NULL,
        [Subject] nvarchar(400) NULL,
        [Type] nvarchar(150) NULL,
        CONSTRAINT [PK_ApiTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ApiTokens_ApiApplications_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES dbo.ApiApplications ([Id]),
        CONSTRAINT [FK_ApiTokens_ApiAuthorizations_AuthorizationId] FOREIGN KEY ([AuthorizationId]) REFERENCES dbo.ApiAuthorizations ([Id])
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApiApplications_ClientId' AND object_id = OBJECT_ID(N'dbo.ApiApplications'))
    CREATE UNIQUE INDEX [IX_ApiApplications_ClientId] ON dbo.ApiApplications ([ClientId]) WHERE [ClientId] IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApiAuthorizations_ApplicationId_Status_Subject_Type' AND object_id = OBJECT_ID(N'dbo.ApiAuthorizations'))
    CREATE INDEX [IX_ApiAuthorizations_ApplicationId_Status_Subject_Type] ON dbo.ApiAuthorizations ([ApplicationId], [Status], [Subject], [Type]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApiScopes_Name' AND object_id = OBJECT_ID(N'dbo.ApiScopes'))
    CREATE UNIQUE INDEX [IX_ApiScopes_Name] ON dbo.ApiScopes ([Name]) WHERE [Name] IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApiTokens_ApplicationId_Status_Subject_Type' AND object_id = OBJECT_ID(N'dbo.ApiTokens'))
    CREATE INDEX [IX_ApiTokens_ApplicationId_Status_Subject_Type] ON dbo.ApiTokens ([ApplicationId], [Status], [Subject], [Type]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApiTokens_AuthorizationId' AND object_id = OBJECT_ID(N'dbo.ApiTokens'))
    CREATE INDEX [IX_ApiTokens_AuthorizationId] ON dbo.ApiTokens ([AuthorizationId]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ApiTokens_ReferenceId' AND object_id = OBJECT_ID(N'dbo.ApiTokens'))
    CREATE UNIQUE INDEX [IX_ApiTokens_ReferenceId] ON dbo.ApiTokens ([ReferenceId]) WHERE [ReferenceId] IS NOT NULL;
