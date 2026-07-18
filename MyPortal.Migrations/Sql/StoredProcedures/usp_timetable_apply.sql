SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Activate @timetableId as the live timetable for its academic year, superseding whichever
-- timetable is currently Active. Runs as one set-based unit; the caller owns the transaction
-- so the whole flip stays atomic (and participates in an ambient unit-of-work transaction
-- when one is supplied). Mirrors the previous multi-statement C# apply exactly.
CREATE OR ALTER PROCEDURE [dbo].[usp_timetable_apply]
    @timetableId    UNIQUEIDENTIFIER,
    @academicYearId UNIQUEIDENTIFIER,
    @effectiveFrom  DATETIME2(7),
    @effectiveTo    DATETIME2(7),
    @active         INT,
    @superseded     INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Capture the timetables we're about to supersede — we need them after the status flip
    -- to truncate downstream Sessions / NonContactAllocations.
    DECLARE @supersededIds TABLE (Id UNIQUEIDENTIFIER PRIMARY KEY);

    INSERT INTO @supersededIds (Id)
    SELECT Id
    FROM dbo.Timetables
    WHERE AcademicYearId = @academicYearId
      AND Status = @active
      AND Id <> @timetableId;

    -- Close out any currently-active timetable for this academic year. EffectiveTo gets the
    -- day before applyDate so there's no overlap window where two timetables claim Active.
    UPDATE dbo.Timetables
       SET Status = @superseded,
           EffectiveTo = DATEADD(DAY, -1, @effectiveFrom),
           LastModifiedAt = SYSUTCDATETIME()
     WHERE AcademicYearId = @academicYearId
       AND Status = @active
       AND Id <> @timetableId;

    -- Truncate prior Sessions and NonContactAllocations so the date-range filter in the register
    -- query doesn't see two timetables overlapping after the cutover. Only shrink — never extend
    -- — and skip rows already ended before applyDate (untouched history). An empty superseded set
    -- simply updates no rows.
    UPDATE dbo.Sessions
       SET EndDate = DATEADD(DAY, -1, @effectiveFrom)
     WHERE TimetableId IN (SELECT Id FROM @supersededIds)
       AND EndDate >= @effectiveFrom;

    UPDATE dbo.StaffNonContactAllocations
       SET EndDate = DATEADD(DAY, -1, @effectiveFrom)
     WHERE TimetableId IN (SELECT Id FROM @supersededIds)
       AND EndDate >= @effectiveFrom;

    -- Activate the new timetable.
    UPDATE dbo.Timetables
       SET Status = @active,
           EffectiveFrom = @effectiveFrom,
           EffectiveTo = @effectiveTo,
           LastModifiedAt = SYSUTCDATETIME()
     WHERE Id = @timetableId;
END;
