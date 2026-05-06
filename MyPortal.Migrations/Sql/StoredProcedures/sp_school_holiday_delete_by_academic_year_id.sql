SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Wipes the SchoolHolidays for the AY plus the underlying DiaryEvent rows that
-- AcademicYearService creates one-to-one alongside them. SchoolHolidays.EventId
-- references DiaryEvents.Id, so the holiday rows must go first; we capture the
-- event IDs into a table variable before the cascade so we can clean those up.
CREATE OR ALTER PROCEDURE [dbo].[sp_school_holiday_delete_by_academic_year_id]
    @academicYearId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @eventIds TABLE ([Id] UNIQUEIDENTIFIER PRIMARY KEY);

    INSERT INTO @eventIds ([Id])
    SELECT EventId
    FROM dbo.SchoolHolidays
    WHERE AcademicYearId = @academicYearId;

    DELETE FROM dbo.SchoolHolidays
    WHERE AcademicYearId = @academicYearId;

    DELETE DE
      FROM dbo.DiaryEvents DE
      JOIN @eventIds       E  ON E.Id = DE.Id;
END;
