SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_register_submit]
    @sessionPeriodId    UNIQUEIDENTIFIER  = NULL,  -- exactly one of these two must be provided
    @regGroupId         UNIQUEIDENTIFIER  = NULL,
    @attendancePeriodId UNIQUEIDENTIFIER,          -- required for reg-group; ignored for lesson (derived from SessionPeriod)
    @attendanceWeekId   UNIQUEIDENTIFIER,
    @marks              [dbo].[AttendanceMarkList] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    IF (@sessionPeriodId IS NULL AND @regGroupId IS NULL)
       OR (@sessionPeriodId IS NOT NULL AND @regGroupId IS NOT NULL)
    BEGIN
        THROW 50001, 'Exactly one of @sessionPeriodId or @regGroupId must be supplied.', 1;
    END

    DECLARE @actualStart    DATETIME2(7);
    DECLARE @actualEnd      DATETIME2(7);
    DECLARE @periodId       UNIQUEIDENTIFIER;
    DECLARE @studentGroupId UNIQUEIDENTIFIER;
    DECLARE @sessionId      UNIQUEIDENTIFIER;

    IF @sessionPeriodId IS NOT NULL
    BEGIN
        SELECT
            @actualStart    = API.ActualStartTime,
            @actualEnd      = API.ActualEndTime,
            @periodId       = API.PeriodId,
            @studentGroupId = CG.StudentGroupId,
            @sessionId      = S.Id
        FROM dbo.SessionPeriods                  AS SP
        JOIN dbo.Sessions                        AS S   ON S.Id = SP.SessionId
        JOIN dbo.Classes                         AS C   ON C.Id = S.ClassId
        JOIN dbo.CurriculumGroups                AS CG  ON CG.Id = C.CurriculumGroupId
        JOIN dbo.vw_attendance_period_instances  AS API ON API.PeriodId = SP.PeriodId
                                                       AND API.AttendanceWeekId = @attendanceWeekId
        WHERE SP.Id = @sessionPeriodId
          AND S.StartDate <= API.ActualEndTime
          AND S.EndDate   >= API.ActualStartTime;
    END
    ELSE
    BEGIN
        SELECT
            @actualStart    = API.ActualStartTime,
            @actualEnd      = API.ActualEndTime,
            @periodId       = API.PeriodId,
            @studentGroupId = RG.StudentGroupId
        FROM dbo.RegGroups                       AS RG
        JOIN dbo.vw_attendance_period_instances  AS API ON API.PeriodId = @attendancePeriodId
                                                       AND API.AttendanceWeekId = @attendanceWeekId
                                                       AND (API.IsAmReg = 1 OR API.IsPmReg = 1)
                                                       AND API.IsLesson = 0
        WHERE RG.Id = @regGroupId;
    END

    IF @actualStart IS NULL
    BEGIN
        THROW 50002, 'No attendance-period instance for the supplied register / week.', 1;
    END

    -- Materialise the date-effective roster once. For lesson registers this is the
    -- StudentGroup's members PLUS any SessionExtraNames for this Session+Week — extras
    -- need to be acceptable submit targets too. Reg-group registers don't carry a Session,
    -- so the extras leg is skipped (@sessionId stays NULL).
    DECLARE @roster TABLE ([StudentId] UNIQUEIDENTIFIER PRIMARY KEY);

    INSERT INTO @roster ([StudentId])
    SELECT SGM.StudentId
    FROM dbo.StudentGroupMemberships AS SGM
    JOIN dbo.Students                AS St ON St.Id = SGM.StudentId AND St.IsDeleted = 0
    WHERE SGM.StudentGroupId = @studentGroupId
      AND SGM.StartDate <= @actualEnd
      AND (SGM.EndDate IS NULL OR SGM.EndDate >= @actualStart);

    IF @sessionId IS NOT NULL
    BEGIN
        INSERT INTO @roster ([StudentId])
        SELECT SEN.StudentId
        FROM dbo.SessionExtraNames AS SEN
        JOIN dbo.Students          AS St ON St.Id = SEN.StudentId AND St.IsDeleted = 0
        WHERE SEN.SessionId = @sessionId
          AND SEN.AttendanceWeekId = @attendanceWeekId
          AND NOT EXISTS (SELECT 1 FROM @roster R WHERE R.StudentId = SEN.StudentId);
    END

    -- Reject students that aren't on the roster.
    IF EXISTS (SELECT 1 FROM @marks M WHERE NOT EXISTS (SELECT 1 FROM @roster R WHERE R.StudentId = M.StudentId))
    BEGIN
        THROW 50003, 'One or more students are not enrolled in this register.', 1;
    END

    -- Reject unknown / inactive / restricted codes.
    IF EXISTS (
        SELECT 1
        FROM @marks M
        LEFT JOIN dbo.AttendanceCodes AC ON AC.Id = M.AttendanceCodeId
        WHERE AC.Id IS NULL OR AC.IsActive = 0 OR AC.IsRestricted = 1
    )
    BEGIN
        THROW 50004, 'One or more attendance codes are unknown, inactive, or restricted.', 1;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Upsert keyed by (StudentId, AttendanceWeekId, AttendancePeriodId).
        MERGE INTO dbo.AttendanceMarks AS Target
        USING (
            SELECT
                M.StudentId,
                M.AttendanceCodeId,
                M.Comments,
                M.MinutesLate
            FROM @marks AS M
        ) AS Source
        ON  Target.StudentId          = Source.StudentId
        AND Target.AttendanceWeekId   = @attendanceWeekId
        AND Target.AttendancePeriodId = @periodId

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
            INSERT (Id, StudentId, AttendanceWeekId, AttendancePeriodId, AttendanceCodeId, Comments, MinutesLate)
            VALUES (NEWID(), Source.StudentId, @attendanceWeekId, @periodId,
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
