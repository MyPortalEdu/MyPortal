CREATE TABLE [ApiApplications] (
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

CREATE TABLE [ApiScopes] (
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

CREATE TABLE [ApiAuthorizations] (
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
    CONSTRAINT [FK_ApiAuthorizations_ApiApplications_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [ApiApplications] ([Id])
);

CREATE TABLE [ApiTokens] (
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
    CONSTRAINT [FK_ApiTokens_ApiApplications_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [ApiApplications] ([Id]),
    CONSTRAINT [FK_ApiTokens_ApiAuthorizations_AuthorizationId] FOREIGN KEY ([AuthorizationId]) REFERENCES [ApiAuthorizations] ([Id])
);

CREATE UNIQUE INDEX [IX_ApiApplications_ClientId] ON [ApiApplications] ([ClientId]) WHERE [ClientId] IS NOT NULL;

CREATE INDEX [IX_ApiAuthorizations_ApplicationId_Status_Subject_Type] ON [ApiAuthorizations] ([ApplicationId], [Status], [Subject], [Type]);

CREATE UNIQUE INDEX [IX_ApiScopes_Name] ON [ApiScopes] ([Name]) WHERE [Name] IS NOT NULL;

CREATE INDEX [IX_ApiTokens_ApplicationId_Status_Subject_Type] ON [ApiTokens] ([ApplicationId], [Status], [Subject], [Type]);

CREATE INDEX [IX_ApiTokens_AuthorizationId] ON [ApiTokens] ([AuthorizationId]);

CREATE UNIQUE INDEX [IX_ApiTokens_ReferenceId] ON [ApiTokens] ([ReferenceId]) WHERE [ReferenceId] IS NOT NULL;

