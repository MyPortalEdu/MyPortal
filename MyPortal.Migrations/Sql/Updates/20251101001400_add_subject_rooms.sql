-- ============================================================================
-- SubjectRooms — many-to-many between Subjects and Rooms, marking which rooms
-- are suitable for which subjects (e.g., DT workshops, sports halls, science labs).
-- ============================================================================

IF OBJECT_ID(N'[dbo].[SubjectRooms]', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[SubjectRooms] (
    [Id] uniqueidentifier NOT NULL,
    [SubjectId] uniqueidentifier NOT NULL,
    [RoomId] uniqueidentifier NOT NULL,
    CONSTRAINT PK_SubjectRooms PRIMARY KEY CLUSTERED ([Id])
);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SubjectRooms_SubjectId_Subjects'
    AND parent_object_id = OBJECT_ID(N'[dbo].[SubjectRooms]'))
BEGIN
ALTER TABLE [dbo].[SubjectRooms]
    ADD CONSTRAINT [FK_SubjectRooms_SubjectId_Subjects]
    FOREIGN KEY ([SubjectId]) REFERENCES [dbo].[Subjects]([Id]) ON DELETE CASCADE;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SubjectRooms_RoomId_Rooms'
    AND parent_object_id = OBJECT_ID(N'[dbo].[SubjectRooms]'))
BEGIN
ALTER TABLE [dbo].[SubjectRooms]
    ADD CONSTRAINT [FK_SubjectRooms_RoomId_Rooms]
    FOREIGN KEY ([RoomId]) REFERENCES [dbo].[Rooms]([Id]) ON DELETE CASCADE;
END
GO

-- Uniqueness — each (subject, room) pairing exists at most once.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_SubjectRooms_SubjectId_RoomId'
    AND object_id = OBJECT_ID(N'[dbo].[SubjectRooms]'))
CREATE UNIQUE INDEX [UX_SubjectRooms_SubjectId_RoomId]
    ON [dbo].[SubjectRooms]([SubjectId], [RoomId]);
GO

-- The hot lookup is "which rooms are suitable for subject X" — covered by the unique index
-- above (its leading column is SubjectId). RoomId-leading lookup is rare so no second index.
