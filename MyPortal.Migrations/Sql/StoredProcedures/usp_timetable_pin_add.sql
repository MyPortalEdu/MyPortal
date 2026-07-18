SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[usp_timetable_pin_add]
    @Id UNIQUEIDENTIFIER,
    @TimetableId UNIQUEIDENTIFIER,
    @CurriculumBlockId UNIQUEIDENTIFIER,
    @SlotIndex INT,
    @ClassId UNIQUEIDENTIFIER,
    @TeacherId UNIQUEIDENTIFIER,
    @RoomId UNIQUEIDENTIFIER,
    @StartAttendancePeriodId UNIQUEIDENTIFIER,
    @CreatedById UNIQUEIDENTIFIER,
    @CreatedByIpAddress NVARCHAR(256),
    @CreatedAt DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.TimetablePins
        (Id, TimetableId, CurriculumBlockId, SlotIndex, ClassId, TeacherId, RoomId,
         StartAttendancePeriodId, CreatedById, CreatedByIpAddress, CreatedAt)
      VALUES
        (@Id, @TimetableId, @CurriculumBlockId, @SlotIndex, @ClassId, @TeacherId, @RoomId,
         @StartAttendancePeriodId, @CreatedById, @CreatedByIpAddress, @CreatedAt);
END;
