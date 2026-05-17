SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_register_get_reg_group]
    @regGroupId         UNIQUEIDENTIFIER,
    @attendancePeriodId UNIQUEIDENTIFIER,
    @attendanceWeekId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @actualStart    DATETIME2(7);
    DECLARE @actualEnd      DATETIME2(7);
    DECLARE @studentGroupId UNIQUEIDENTIFIER;
    DECLARE @targetDate     DATE;

    -- Reg-group registers are AM/PM only and never apply to a lesson-flagged period
    -- (those go through the lesson register taken by the subject teacher). Guard at
    -- the data layer so we don't accept arbitrary or lesson periods through the
    -- reg-group endpoint.
    SELECT
        @actualStart    = API.ActualStartTime,
        @actualEnd      = API.ActualEndTime,
        @studentGroupId = RG.StudentGroupId
    FROM dbo.RegGroups                       AS RG
    JOIN dbo.vw_attendance_period_instances  AS API ON API.PeriodId = @attendancePeriodId
                                                   AND API.AttendanceWeekId = @attendanceWeekId
                                                   AND (API.IsAmReg = 1 OR API.IsPmReg = 1)
                                                   AND API.IsLesson = 0
    WHERE RG.Id = @regGroupId;

    IF @actualStart IS NULL
    BEGIN
        RETURN;
    END

    SET @targetDate = CAST(@actualStart AS DATE);

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

    -- Reg-group rosters come purely from StudentGroupMembership; SessionExtraNames is a
    -- lesson-Session concept and doesn't apply to reg groups.
    DECLARE @roster TABLE
    (
        StudentId UNIQUEIDENTIFIER PRIMARY KEY
    );

    INSERT INTO @roster (StudentId)
    SELECT St.Id
    FROM dbo.StudentGroupMemberships AS SGM
    JOIN dbo.Students                AS St ON St.Id = SGM.StudentId AND St.IsDeleted = 0
    WHERE SGM.StudentGroupId = @studentGroupId
      AND SGM.StartDate <= @actualEnd
      AND (SGM.EndDate IS NULL OR SGM.EndDate >= @actualStart);

    -- 1) Header.
    SELECT
        SessionPeriodId    = CAST(NULL AS UNIQUEIDENTIFIER),
        RegGroupId         = @regGroupId,
        AttendanceWeekId   = @attendanceWeekId,
        AttendancePeriodId = @attendancePeriodId,
        StartTime          = @actualStart,
        EndTime            = @actualEnd,
        PeriodName         = AP.[Name],
        IsAmReg            = AP.IsAmReg,
        IsPmReg            = AP.IsPmReg,
        GroupCode          = SG.Code,
        TeacherId          = SM.Id,
        TeacherName        = TName.[Name],
        RoomId             = R.Id,
        RoomName           = R.[Name],
        IsCover            = CAST(0 AS BIT)
    FROM dbo.RegGroups               AS RG
    JOIN dbo.AttendancePeriods       AS AP  ON AP.Id = @attendancePeriodId
    LEFT JOIN dbo.StudentGroups      AS SG  ON SG.Id = RG.StudentGroupId
    LEFT JOIN dbo.StaffMembers       AS SM  ON SM.Id = SG.MainSupervisorId
    LEFT JOIN dbo.Rooms              AS R   ON R.Id  = RG.RoomId
    OUTER APPLY dbo.fn_person_get_name(SM.PersonId, 2, 0, 1) AS TName
    WHERE RG.Id = @regGroupId;

    -- 2) Periods landing on the same calendar date as the target reg.
    SELECT
        AttendancePeriodId = DP.PeriodId,
        PeriodName         = DP.[Name],
        StartTime          = DP.StartTime,
        EndTime            = DP.EndTime,
        IsAmReg            = DP.IsAmReg,
        IsPmReg            = DP.IsPmReg
    FROM @dayPeriods AS DP
    ORDER BY DP.StartTime;

    -- 3) Roster. IsExtra is always 0 for reg-group registers but emitted for shape parity
    --    with the lesson SP so the repo can read both with the same code.
    SELECT
        StudentId       = St.Id,
        FirstName       = COALESCE(NULLIF(P.PreferredFirstName, ''), P.FirstName),
        LastName        = COALESCE(NULLIF(P.PreferredLastName, ''),  P.LastName),
        DisplayName     = PName.[Name],
        AdmissionNumber = St.AdmissionNumber,
        IsExtra         = CAST(0 AS BIT)
    FROM @roster                     AS R
    JOIN dbo.Students                AS St ON St.Id = R.StudentId
    JOIN dbo.People                  AS P  ON P.Id  = St.PersonId
    OUTER APPLY dbo.fn_person_get_name(P.Id, 5, 1, 0) AS PName
    ORDER BY
        COALESCE(NULLIF(P.PreferredLastName, ''),  P.LastName)  ASC,
        COALESCE(NULLIF(P.PreferredFirstName, ''), P.FirstName) ASC;

    -- 4) Marks for any roster student at any of the day's periods.
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
