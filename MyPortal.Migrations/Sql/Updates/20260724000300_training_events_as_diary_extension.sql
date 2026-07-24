-- Reshape TrainingEvents into a thin extension of a DiaryEvent (scheduling moves to the diary event). Idempotent.

-- Drop the standalone scheduling columns now owned by the DiaryEvent.
DROP INDEX IF EXISTS [IX_TrainingEvents_StartDate] ON [dbo].[TrainingEvents];
GO
IF COL_LENGTH(N'[dbo].[TrainingEvents]', N'Title')     IS NOT NULL ALTER TABLE [dbo].[TrainingEvents] DROP COLUMN [Title];
IF COL_LENGTH(N'[dbo].[TrainingEvents]', N'StartDate') IS NOT NULL ALTER TABLE [dbo].[TrainingEvents] DROP COLUMN [StartDate];
IF COL_LENGTH(N'[dbo].[TrainingEvents]', N'EndDate')   IS NOT NULL ALTER TABLE [dbo].[TrainingEvents] DROP COLUMN [EndDate];
IF COL_LENGTH(N'[dbo].[TrainingEvents]', N'Venue')     IS NOT NULL ALTER TABLE [dbo].[TrainingEvents] DROP COLUMN [Venue];
IF COL_LENGTH(N'[dbo].[TrainingEvents]', N'Notes')     IS NOT NULL ALTER TABLE [dbo].[TrainingEvents] DROP COLUMN [Notes];
GO

-- Add the 1:1 diary-event link. Table is empty, so the temporary default just satisfies NOT NULL.
IF COL_LENGTH(N'[dbo].[TrainingEvents]', N'DiaryEventId') IS NULL
    ALTER TABLE [dbo].[TrainingEvents] ADD [DiaryEventId] uniqueidentifier NOT NULL
        CONSTRAINT DF_TrainingEvents_DiaryEventId DEFAULT ('00000000-0000-0000-0000-000000000000');
GO
IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_TrainingEvents_DiaryEventId')
    ALTER TABLE [dbo].[TrainingEvents] DROP CONSTRAINT [DF_TrainingEvents_DiaryEventId];
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_TrainingEvents_DiaryEventId_DiaryEvents')
    ALTER TABLE [dbo].[TrainingEvents] ADD CONSTRAINT [FK_TrainingEvents_DiaryEventId_DiaryEvents]
        FOREIGN KEY ([DiaryEventId]) REFERENCES [dbo].[DiaryEvents]([Id]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_TrainingEvents_DiaryEventId')
    CREATE UNIQUE INDEX [UX_TrainingEvents_DiaryEventId] ON [dbo].[TrainingEvents]([DiaryEventId]);
GO
