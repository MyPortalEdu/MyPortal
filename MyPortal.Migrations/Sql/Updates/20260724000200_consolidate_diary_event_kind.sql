-- Move diary-event typing onto DiaryEvents/DiaryEventTemplates.Kind and drop the DiaryEventTypes table. Idempotent.
-- fn_diary_event_get_overlapping is SCHEMABINDING on EventTypeId, so it is dropped here to release the
-- binding before the column goes; the Functions pass recreates it (on Kind) later in the same run.

-- 1) Kind column on the two carriers, backfilled from the lookup's Kind.
IF COL_LENGTH(N'[dbo].[DiaryEvents]', N'Kind') IS NULL
    ALTER TABLE [dbo].[DiaryEvents] ADD [Kind] tinyint NOT NULL CONSTRAINT DF_DiaryEvents_Kind DEFAULT (0);
GO

IF COL_LENGTH(N'[dbo].[DiaryEventTemplates]', N'Kind') IS NULL
    ALTER TABLE [dbo].[DiaryEventTemplates] ADD [Kind] tinyint NOT NULL CONSTRAINT DF_DiaryEventTemplates_Kind DEFAULT (0);
GO

IF OBJECT_ID(N'[dbo].[DiaryEventTypes]', N'U') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[DiaryEvents]', N'EventTypeId') IS NOT NULL
    UPDATE e SET e.[Kind] = t.[Kind]
    FROM [dbo].[DiaryEvents] e
    JOIN [dbo].[DiaryEventTypes] t ON t.[Id] = e.[EventTypeId];
GO

IF OBJECT_ID(N'[dbo].[DiaryEventTypes]', N'U') IS NOT NULL
   AND COL_LENGTH(N'[dbo].[DiaryEventTemplates]', N'DiaryEventTypeId') IS NOT NULL
    UPDATE dt SET dt.[Kind] = t.[Kind]
    FROM [dbo].[DiaryEventTemplates] dt
    JOIN [dbo].[DiaryEventTypes] t ON t.[Id] = dt.[DiaryEventTypeId];
GO

-- 2) Release the schemabound function so EventTypeId can be dropped (recreated by the Functions pass).
DROP FUNCTION IF EXISTS [dbo].[fn_diary_event_get_overlapping];
GO

-- 3) Drop the EventTypeId / DiaryEventTypeId FKs + columns.
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DiaryEvents_EventTypeId_DiaryEventTypes')
    ALTER TABLE [dbo].[DiaryEvents] DROP CONSTRAINT [FK_DiaryEvents_EventTypeId_DiaryEventTypes];
GO
IF COL_LENGTH(N'[dbo].[DiaryEvents]', N'EventTypeId') IS NOT NULL
    ALTER TABLE [dbo].[DiaryEvents] DROP COLUMN [EventTypeId];
GO

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DiaryEventTemplates_DiaryEventTypeId_DiaryEventTypes')
    ALTER TABLE [dbo].[DiaryEventTemplates] DROP CONSTRAINT [FK_DiaryEventTemplates_DiaryEventTypeId_DiaryEventTypes];
GO
DROP INDEX IF EXISTS [IX_DiaryEventTemplates_DiaryEventTypeId] ON [dbo].[DiaryEventTemplates];
GO
IF COL_LENGTH(N'[dbo].[DiaryEventTemplates]', N'DiaryEventTypeId') IS NOT NULL
    ALTER TABLE [dbo].[DiaryEventTemplates] DROP COLUMN [DiaryEventTypeId];
GO

-- 4) Drop the retired lookup.
DROP TABLE IF EXISTS [dbo].[DiaryEventTypes];
GO
