﻿CREATE TABLE [dbo].[EmailAddresses] (
    [Id]        UNIQUEIDENTIFIER NOT NULL,
    [ClusterId] INT              IDENTITY (1, 1) NOT NULL,
    [TypeId]    UNIQUEIDENTIFIER NOT NULL,
    [PersonId]  UNIQUEIDENTIFIER NULL,
    [AgencyId]  UNIQUEIDENTIFIER NULL,
    [Address]   NVARCHAR (128)   NOT NULL,
    [Main]      BIT              NOT NULL,
    [Notes]     NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_EmailAddresses] PRIMARY KEY NONCLUSTERED ([Id] ASC),
    CONSTRAINT [FK_EmailAddresses_Agencies_AgencyId] FOREIGN KEY ([AgencyId]) REFERENCES [dbo].[Agencies] ([Id]),
    CONSTRAINT [FK_EmailAddresses_EmailAddressTypes_TypeId] FOREIGN KEY ([TypeId]) REFERENCES [dbo].[EmailAddressTypes] ([Id]),
    CONSTRAINT [FK_EmailAddresses_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[People] ([Id])
);


GO
CREATE UNIQUE CLUSTERED INDEX [CIX_ClusterId]
    ON [dbo].[EmailAddresses]([ClusterId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_EmailAddresses_AgencyId]
    ON [dbo].[EmailAddresses]([AgencyId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_EmailAddresses_PersonId]
    ON [dbo].[EmailAddresses]([PersonId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_EmailAddresses_TypeId]
    ON [dbo].[EmailAddresses]([TypeId] ASC);

