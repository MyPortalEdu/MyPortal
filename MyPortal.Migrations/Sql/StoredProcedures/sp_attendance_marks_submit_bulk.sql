SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Bulk attendance mark editor (reception/admin). Each TVP row carries a single
-- (Student, Week, Period) cell:
--   * AttendanceCodeId IS NOT NULL -> upsert via MERGE.
--   * AttendanceCodeId IS NULL     -> delete the existing mark for that cell.
--
-- Validates that every cell's (Week, Period) is a real instance for the
-- StudentGroup's AY landing in [@from, @to], and that the student is in the
-- group's roster (regular SGM membership or a SessionExtraName for a Session
-- of the group's curriculum within range). Code policy (active / restricted)
-- lives in the service layer for parity with sp_register_submit.
CREATE OR ALTER PROCEDURE [dbo].[sp_attendance_marks_submit_bulk]
    @studentGroupId UNIQUEIDENTIFIER,
    @from           DATE,
    @to             DATE,
    @marks          [dbo].[BulkAttendanceMarkList] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @academicYearId UNIQUEIDENTIFIER;

    SELECT @academicYearId = SG.AcademicYearId
    FROM dbo.StudentGroups AS SG
    WHERE SG.Id = @studentGroupId;

    IF @academicYearId IS NULL
    BEGIN
        THROW 50020, 'Student group not found.', 1;
    END

    -- Empty payload is a no-op; saves us materialising scope tables for nothing.
    IF NOT EXISTS (SELECT 1 FROM @marks)
    BEGIN
        RETURN;
    END

    -- Valid (Week, Period) instances for this group's AY in [@from, @to].
    DECLARE @validInstances TABLE
    (
        AttendanceWeekId   UNIQUEIDENTIFIER NOT NULL,
        AttendancePeriodId UNIQUEIDENTIFIER NOT NULL,
        PRIMARY KEY (AttendanceWeekId, AttendancePeriodId)
    );

    INSERT INTO @validInstances (AttendanceWeekId, AttendancePeriodId)
    SELECT API.AttendanceWeekId, API.PeriodId
    FROM dbo.vw_attendance_period_instances AS API
    WHERE API.AcademicYearId = @academicYearId
      AND CAST(API.ActualStartTime AS DATE) BETWEEN @from AND @to;

    IF EXISTS (
        SELECT 1
        FROM @marks AS M
        WHERE NOT EXISTS (
            SELECT 1 FROM @validInstances AS V
            WHERE V.AttendanceWeekId = M.AttendanceWeekId
              AND V.AttendancePeriodId = M.AttendancePeriodId
        )
    )
    BEGIN
        THROW 50021, 'One or more marks reference a (week, period) outside the requested range.', 1;
    END

    -- Roster (regular + extras), same shape as sp_attendance_marks_get_bulk.
    DECLARE @roster TABLE (StudentId UNIQUEIDENTIFIER PRIMARY KEY);

    INSERT INTO @roster (StudentId)
    SELECT St.Id
    FROM dbo.StudentGroupMemberships AS SGM
    JOIN dbo.Students                AS St ON St.Id = SGM.StudentId AND St.IsDeleted = 0
    WHERE SGM.StudentGroupId = @studentGroupId
      AND SGM.StartDate <= DATEADD(DAY, 1, @to)
      AND (SGM.EndDate IS NULL OR SGM.EndDate >= @from);

    INSERT INTO @roster (StudentId)
    SELECT DISTINCT St.Id
    FROM dbo.SessionExtraNames AS SEN
    JOIN dbo.Sessions          AS S  ON S.Id  = SEN.SessionId
    JOIN dbo.Classes           AS C  ON C.Id  = S.ClassId
    JOIN dbo.CurriculumGroups  AS CG ON CG.Id = C.CurriculumGroupId
    JOIN dbo.AttendanceWeeks   AS AW ON AW.Id = SEN.AttendanceWeekId
    JOIN dbo.Students          AS St ON St.Id = SEN.StudentId AND St.IsDeleted = 0
    WHERE CG.StudentGroupId = @studentGroupId
      AND AW.Beginning BETWEEN DATEADD(DAY, -6, @from) AND @to
      AND NOT EXISTS (SELECT 1 FROM @roster R WHERE R.StudentId = St.Id);

    IF EXISTS (
        SELECT 1
        FROM @marks AS M
        WHERE NOT EXISTS (SELECT 1 FROM @roster R WHERE R.StudentId = M.StudentId)
    )
    BEGIN
        THROW 50022, 'One or more marks reference a student outside the group roster.', 1;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Deletes (NULL code = clear the cell).
        DELETE AM
        FROM dbo.AttendanceMarks AS AM
        JOIN @marks              AS M  ON M.StudentId          = AM.StudentId
                                       AND M.AttendanceWeekId   = AM.AttendanceWeekId
                                       AND M.AttendancePeriodId = AM.AttendancePeriodId
        WHERE M.AttendanceCodeId IS NULL;

        -- Upserts (non-null code).
        MERGE INTO dbo.AttendanceMarks AS Target
        USING (
            SELECT M.StudentId, M.AttendanceWeekId, M.AttendancePeriodId,
                   M.AttendanceCodeId, M.Comments, M.MinutesLate
            FROM @marks AS M
            WHERE M.AttendanceCodeId IS NOT NULL
        ) AS Source
        ON  Target.StudentId          = Source.StudentId
        AND Target.AttendanceWeekId   = Source.AttendanceWeekId
        AND Target.AttendancePeriodId = Source.AttendancePeriodId

        WHEN MATCHED AND (
                Target.AttendanceCodeId <> Source.AttendanceCodeId
             OR ISNULL(Target.Comments, N'')   <> ISNULL(Source.Comments, N'')
             OR ISNULL(Target.MinutesLate, -1) <> ISNULL(Source.MinutesLate, -1)
            )
        THEN UPDATE SET
            AttendanceCodeId = Source.AttendanceCodeId,
            Comments         = Source.Comments,
            MinutesLate      = Source.MinutesLate

        WHEN NOT MATCHED BY TARGET THEN
            INSERT (Id, StudentId, AttendanceWeekId, AttendancePeriodId,
                    AttendanceCodeId, Comments, MinutesLate)
            VALUES (NEWID(), Source.StudentId, Source.AttendanceWeekId, Source.AttendancePeriodId,
                    Source.AttendanceCodeId, Source.Comments, Source.MinutesLate);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
