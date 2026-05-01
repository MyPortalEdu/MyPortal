SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_register_get_lesson]
    @sessionPeriodId    UNIQUEIDENTIFIER,
    @attendanceWeekId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    -- Resolve the (SessionPeriod x AttendanceWeek) instance: gives us the dated start/end,
    -- the underlying AttendancePeriod, the Class, and (cover-aware) teacher / room.
    DECLARE @actualStart        DATETIME2(7);
    DECLARE @actualEnd          DATETIME2(7);
    DECLARE @attendancePeriodId UNIQUEIDENTIFIER;
    DECLARE @classId            UNIQUEIDENTIFIER;
    DECLARE @studentGroupId     UNIQUEIDENTIFIER;

    SELECT
        @actualStart        = API.ActualStartTime,
        @actualEnd          = API.ActualEndTime,
        @attendancePeriodId = API.PeriodId,
        @classId            = S.ClassId,
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

    -- 2) Roster (date-effective members of the CurriculumGroup's StudentGroup).
    SELECT
        StudentId       = St.Id,
        FirstName       = COALESCE(NULLIF(P.PreferredFirstName, ''), P.FirstName),
        LastName        = COALESCE(NULLIF(P.PreferredLastName, ''),  P.LastName),
        DisplayName     = PName.[Name],
        AdmissionNumber = St.AdmissionNumber
    FROM dbo.StudentGroupMemberships AS SGM
    JOIN dbo.Students                AS St ON St.Id = SGM.StudentId AND St.IsDeleted = 0
    JOIN dbo.People                  AS P  ON P.Id = St.PersonId
    OUTER APPLY dbo.fn_person_get_name(P.Id, 5, 1, 0) AS PName
    WHERE SGM.StudentGroupId = @studentGroupId
      AND SGM.StartDate <= @actualEnd
      AND (SGM.EndDate IS NULL OR SGM.EndDate >= @actualStart)
    ORDER BY
        COALESCE(NULLIF(P.PreferredLastName, ''),  P.LastName)  ASC,
        COALESCE(NULLIF(P.PreferredFirstName, ''), P.FirstName) ASC;

    -- 3) Existing marks for this (week, period) limited to the resolved roster.
    SELECT
        AttendanceMarkId   = AM.Id,
        StudentId          = AM.StudentId,
        AttendanceCodeId   = AM.AttendanceCodeId,
        Code               = AC.Code,
        Comments           = AM.Comments,
        MinutesLate        = AM.MinutesLate
    FROM dbo.AttendanceMarks         AS AM
    JOIN dbo.AttendanceCodes         AS AC  ON AC.Id = AM.AttendanceCodeId
    JOIN dbo.StudentGroupMemberships AS SGM ON SGM.StudentId = AM.StudentId
                                           AND SGM.StudentGroupId = @studentGroupId
                                           AND SGM.StartDate <= @actualEnd
                                           AND (SGM.EndDate IS NULL OR SGM.EndDate >= @actualStart)
    WHERE AM.AttendanceWeekId   = @attendanceWeekId
      AND AM.AttendancePeriodId = @attendancePeriodId;
END;
GO
