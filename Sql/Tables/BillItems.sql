﻿CREATE TABLE [dbo].[BillItems] (
    [Id]               UNIQUEIDENTIFIER NOT NULL,
    [ClusterId]        INT              IDENTITY (1, 1) NOT NULL,
    [BillId]           UNIQUEIDENTIFIER NOT NULL,
    [ProductId]        UNIQUEIDENTIFIER NOT NULL,
    [Quantity]         INT              NOT NULL,
    [NetAmount]        DECIMAL (10, 2)  NOT NULL,
    [VatAmount]        DECIMAL (10, 2)  NOT NULL,
    [CustomerReceived] BIT              NOT NULL,
    [Refunded]         BIT              NOT NULL,
    CONSTRAINT [PK_BillItems] PRIMARY KEY NONCLUSTERED ([Id] ASC),
    CONSTRAINT [FK_BillItems_Bills_BillId] FOREIGN KEY ([BillId]) REFERENCES [dbo].[Bills] ([Id]),
    CONSTRAINT [FK_BillItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id])
);


GO
CREATE UNIQUE CLUSTERED INDEX [CIX_ClusterId]
    ON [dbo].[BillItems]([ClusterId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_BillItems_BillId]
    ON [dbo].[BillItems]([BillId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_BillItems_ProductId]
    ON [dbo].[BillItems]([ProductId] ASC);

