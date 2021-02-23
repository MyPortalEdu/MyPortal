﻿CREATE TABLE [dbo].[Roles] (
    [Id]               UNIQUEIDENTIFIER NOT NULL,
    [Name]             NVARCHAR (256)   NULL,
    [NormalizedName]   NVARCHAR (256)   NULL,
    [ConcurrencyStamp] NVARCHAR (MAX)   NULL,
    [Description]      NVARCHAR (MAX)   NULL,
    [System]           BIT              NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex]
    ON [dbo].[Roles]([NormalizedName] ASC) WHERE ([NormalizedName] IS NOT NULL);
