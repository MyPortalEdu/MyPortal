SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_register_get_reg_group]
    @regGroupId         UNIQUEIDENTIFIER,
    @attendancePeriodId UNIQUEIDENTIFIER,
    @attendanceWeekId   UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @actualStart    DATETIME2(7);
    DECLARE @actualEnd      DATETIME2(7);
    DECLARE @studentGroupId UNIQUEIDENTIFIER;

    -- Reg-group registers are AM/PM only — guard at the data layer too so we don't accept
    -- arbitrary lesson periods through the reg-group endpoint.
    SELECT
        @actualStart    = API.ActualStartTime,
        @actualEnd      = API.ActualEndTime,
        @studentGroupId = RG.StudentGroupId
    FROM dbo.RegGroups                       AS RG
    JOIN dbo.vw_attendance_period_instances  AS API ON API.PeriodId = @attendancePeriodId
                                                   AND API.AttendanceWeekId = @attendanceWeekId
                                                   AND (API.IsAmReg = 1 OR API.IsPmReg = 1)
    WHERE RG.Id = @regGroupId;

    IF @actualStart IS NULL
    BEGIN
        RETURN;
    END

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

    -- 2) Roster.
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

    -- 3) Existing marks.
    SELECT
        AttendanceMarkId = AM.Id,
        StudentId        = AM.StudentId,
        AttendanceCodeId = AM.AttendanceCodeId,
        Code             = AC.Code,
        Comments         = AM.Comments,
        MinutesLate      = AM.MinutesLate
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
