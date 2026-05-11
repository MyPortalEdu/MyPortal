SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_register_get_lesson]
    @sessionPeriodId    UNIQUEIDENTIFIER,
    @attendanceWeekId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Resolve the (SessionPeriod x AttendanceWeek) instance: gives us the dated start/end,
    -- the underlying AttendancePeriod, the Session/Class, and the StudentGroup feeding the
    -- regular roster.
    DECLARE @actualStart        DATETIME2(7);
    DECLARE @actualEnd          DATETIME2(7);
    DECLARE @attendancePeriodId UNIQUEIDENTIFIER;
    DECLARE @sessionId          UNIQUEIDENTIFIER;
    DECLARE @studentGroupId     UNIQUEIDENTIFIER;
    DECLARE @targetDate         DATE;

    SELECT
        @actualStart        = API.ActualStartTime,
        @actualEnd          = API.ActualEndTime,
        @attendancePeriodId = API.PeriodId,
        @sessionId          = S.Id,
        @studentGroupId     = CG.StudentGroupId
    FROM dbo.SessionPeriods                  AS SP
    JOIN dbo.Sessions                        AS S   ON S.Id = SP.SessionId
    JOIN dbo.Classes                         AS C   ON C.Id = S.ClassId
    JOIN dbo.CurriculumGroups                AS CG  ON CG.Id = C.CurriculumGroupId
    JOIN dbo.vw_attendance_period_instances  AS API ON API.PeriodId = SP.PeriodId
                                                   AND API.AttendanceWeekId = @attendanceWeekId
    WHERE SP.Id = @sessionPeriodId
      AND S.StartDate <= API.ActualEndTime
      AND S.EndDate   >= API.ActualStartTime;

    IF @actualStart IS NULL
    BEGIN
        -- No instance for this (sessionPeriod, week) — caller decides 404 vs empty.
        RETURN;
    END

    SET @targetDate = CAST(@actualStart AS DATE);

    -- Materialise the day's period instances and the union roster once so the output
    -- queries below can reuse them without re-running the cycle / membership joins.
    DECLARE @dayPeriods TABLE
    (
        PeriodId  UNIQUEIDENTIFIER PRIMARY KEY,
        [Name]    NVARCHAR(128) NOT NULL,
        StartTime DATETIME2(7) NOT NULL,
        EndTime   DATETIME2(7) NOT NULL,
        IsAmReg   BIT NOT NULL,
        IsPmReg   BIT NOT NULL
    );

    INSERT INTO @dayPeriods (PeriodId, [Name], StartTime, EndTime, IsAmReg, IsPmReg)
    SELECT API.PeriodId, API.[Name], API.ActualStartTime, API.ActualEndTime, API.IsAmReg, API.IsPmReg
    FROM dbo.vw_attendance_period_instances AS API
    WHERE API.AttendanceWeekId = @attendanceWeekId
      AND CAST(API.ActualStartTime AS DATE) = @targetDate;

    DECLARE @roster TABLE
    (
        StudentId UNIQUEIDENTIFIER PRIMARY KEY,
        IsExtra   BIT NOT NULL
    );

    -- Regular roster: date-effective members of the CurriculumGroup's StudentGroup.
    INSERT INTO @roster (StudentId, IsExtra)
    SELECT St.Id, CAST(0 AS BIT)
    FROM dbo.StudentGroupMemberships AS SGM
    JOIN dbo.Students                AS St ON St.Id = SGM.StudentId AND St.IsDeleted = 0
    WHERE SGM.StudentGroupId = @studentGroupId
      AND SGM.StartDate <= @actualEnd
      AND (SGM.EndDate IS NULL OR SGM.EndDate >= @actualStart);

    -- Extras: students added ad-hoc to this Session+Week via SessionExtraNames. They appear
    -- here in addition to wherever their regular roster slot is — extras add a student to
    -- this register, they don't remove them from their usual one. NOT EXISTS guards against
    -- double-listing if someone is somehow both a regular member and an extra of the same
    -- session (data anomaly — regular wins).
    INSERT INTO @roster (StudentId, IsExtra)
    SELECT St.Id, CAST(1 AS BIT)
    FROM dbo.SessionExtraNames AS SEN
    JOIN dbo.Students          AS St ON St.Id = SEN.StudentId AND St.IsDeleted = 0
    WHERE SEN.SessionId = @sessionId
      AND SEN.AttendanceWeekId = @attendanceWeekId
      AND NOT EXISTS (SELECT 1 FROM @roster R WHERE R.StudentId = St.Id);

    -- 1) Header (cover-aware teacher/room, mirrors vw_session_period_metadata's lesson arm).
    SELECT
        SessionPeriodId    = @sessionPeriodId,
        RegGroupId         = CAST(NULL AS UNIQUEIDENTIFIER),
        AttendanceWeekId   = @attendanceWeekId,
        AttendancePeriodId = @attendancePeriodId,
        StartTime          = @actualStart,
        EndTime            = @actualEnd,
        PeriodName         = AP.[Name],
        IsAmReg            = AP.IsAmReg,
        IsPmReg            = AP.IsPmReg,
        GroupCode          = C.Code,
        TeacherId          = TSM.Id,
        TeacherName        = TName.[Name],
        RoomId             = TR.Id,
        RoomName           = TR.[Name],
        IsCover            = CASE WHEN CA.Id IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END
    FROM dbo.SessionPeriods                  AS SP
    JOIN dbo.Sessions                        AS S   ON S.Id = SP.SessionId
    JOIN dbo.Classes                         AS C   ON C.Id = S.ClassId
    JOIN dbo.AttendancePeriods               AS AP  ON AP.Id = SP.PeriodId
    LEFT JOIN dbo.CoverArrangements          AS CA  ON CA.SessionId = S.Id
                                                   AND CA.WeekId = @attendanceWeekId
    LEFT JOIN dbo.StaffMembers               AS TSM ON TSM.Id = COALESCE(CA.TeacherId, S.TeacherId)
    LEFT JOIN dbo.Rooms                      AS TR  ON TR.Id  = COALESCE(CA.RoomId, S.RoomId)
    OUTER APPLY dbo.fn_person_get_name(TSM.PersonId, 2, 0, 1) AS TName
    WHERE SP.Id = @sessionPeriodId;

    -- 2) Periods landing on the same calendar date as the target. The teacher gets the
    --    full day as context; the register UI restricts edits to the target period.
    SELECT
        AttendancePeriodId = DP.PeriodId,
        PeriodName         = DP.[Name],
        StartTime          = DP.StartTime,
        EndTime            = DP.EndTime,
        IsAmReg            = DP.IsAmReg,
        IsPmReg            = DP.IsPmReg
    FROM @dayPeriods AS DP
    ORDER BY DP.StartTime;

    -- 3) Roster (regular members + extras), with IsExtra distinguishing the two.
    SELECT
        StudentId       = St.Id,
        FirstName       = COALESCE(NULLIF(P.PreferredFirstName, ''), P.FirstName),
        LastName        = COALESCE(NULLIF(P.PreferredLastName, ''),  P.LastName),
        DisplayName     = PName.[Name],
        AdmissionNumber = St.AdmissionNumber,
        IsExtra         = R.IsExtra
    FROM @roster                     AS R
    JOIN dbo.Students                AS St ON St.Id = R.StudentId
    JOIN dbo.People                  AS P  ON P.Id  = St.PersonId
    OUTER APPLY dbo.fn_person_get_name(P.Id, 5, 1, 0) AS PName
    ORDER BY
        COALESCE(NULLIF(P.PreferredLastName, ''),  P.LastName)  ASC,
        COALESCE(NULLIF(P.PreferredFirstName, ''), P.FirstName) ASC;

    -- 4) Marks for any roster student at any of the day's periods, this week. The teacher
    --    sees their full-day attendance picture (read-only outside the target period).
    SELECT
        AttendanceMarkId   = AM.Id,
        StudentId          = AM.StudentId,
        AttendancePeriodId = AM.AttendancePeriodId,
        AttendanceCodeId   = AM.AttendanceCodeId,
        Code               = AC.Code,
        Comments           = AM.Comments,
        MinutesLate        = AM.MinutesLate
    FROM dbo.AttendanceMarks AS AM
    JOIN dbo.AttendanceCodes AS AC ON AC.Id = AM.AttendanceCodeId
    JOIN @roster             AS R  ON R.StudentId = AM.StudentId
    JOIN @dayPeriods         AS DP ON DP.PeriodId = AM.AttendancePeriodId
    WHERE AM.AttendanceWeekId = @attendanceWeekId;
END;
GO
