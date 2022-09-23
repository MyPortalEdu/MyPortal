﻿CREATE TABLE [dbo].[BillDiscounts] (
    [Id]          UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [BillId]      UNIQUEIDENTIFIER NOT NULL,
    [DiscountId]  UNIQUEIDENTIFIER NOT NULL,
    [GrossAmount] DECIMAL (10, 2)  NOT NULL,
    CONSTRAINT [PK_BillDiscounts] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_BillDiscounts_Bills_BillId] FOREIGN KEY ([BillId]) REFERENCES [dbo].[Bills] ([Id]),
    CONSTRAINT [FK_BillDiscounts_Discounts_DiscountId] FOREIGN KEY ([DiscountId]) REFERENCES [dbo].[Discounts] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_BillDiscounts_BillId]
    ON [dbo].[BillDiscounts]([BillId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_BillDiscounts_DiscountId]
    ON [dbo].[BillDiscounts]([DiscountId] ASC);
