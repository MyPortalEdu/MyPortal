-- ============================================================================
-- StaffNonContactAllocations — per-staff, per-period, per-timetable rows for
-- PPA and other non-teaching codes (cover, TLR, supervision). Materialisation
-- writes PPA rows alongside Sessions; later codes can be added by promoting
-- the Code column to a lookup table.
-- ============================================================================

IF OBJECT_ID(N'[dbo].[StaffNonContactAllocations]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[StaffNonContactAllocations] (
    [Id] uniqueidentifier NOT NULL,
    [StaffMemberId] uniqueidentifier NOT NULL,
    -- Nullable so admins can hand-author allocations outside of the timetable workflow.
    [TimetableId] uniqueidentifier NULL,
    [AttendancePeriodId] uniqueidentifier NOT NULL,
    [Code] nvarchar(20) NOT NULL,
    [StartDate] date NOT NULL,
    [EndDate] date NOT NULL,
    CONSTRAINT PK_StaffNonContactAllocations PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_StaffNonContactAllocations_StaffMembers'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffNonContactAllocations]'))
BEGIN
ALTER TABLE [dbo].[StaffNonContactAllocations]
    ADD CONSTRAINT [FK_StaffNonContactAllocations_StaffMembers]
    FOREIGN KEY ([StaffMemberId]) REFERENCES [dbo].[StaffMembers]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_StaffNonContactAllocations_Timetables'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffNonContactAllocations]'))
BEGIN
ALTER TABLE [dbo].[StaffNonContactAllocations]
    ADD CONSTRAINT [FK_StaffNonContactAllocations_Timetables]
    FOREIGN KEY ([TimetableId]) REFERENCES [dbo].[Timetables]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_StaffNonContactAllocations_AttendancePeriods'
    AND parent_object_id = OBJECT_ID(N'[dbo].[StaffNonContactAllocations]'))
BEGIN
ALTER TABLE [dbo].[StaffNonContactAllocations]
    ADD CONSTRAINT [FK_StaffNonContactAllocations_AttendancePeriods]
    FOREIGN KEY ([AttendancePeriodId]) REFERENCES [dbo].[AttendancePeriods]([Id]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
    WHERE name = N'IX_StaffNonContactAllocations_StaffMemberId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffNonContactAllocations]'))
CREATE INDEX [IX_StaffNonContactAllocations_StaffMemberId]
    ON [dbo].[StaffNonContactAllocations]([StaffMemberId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
    WHERE name = N'IX_StaffNonContactAllocations_TimetableId'
    AND object_id = OBJECT_ID(N'[dbo].[StaffNonContactAllocations]'))
CREATE INDEX [IX_StaffNonContactAllocations_TimetableId]
    ON [dbo].[StaffNonContactAllocations]([TimetableId]);
GO
