SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Bulk attendance grid: every (week, period) instance for @studentGroupId's academic
-- year that lands in [@from, @to], the group's roster (regular + extras), and any
-- marks already recorded for those students at those instances. Returns four result
-- sets in order: header, periods, students, marks. Returns nothing (no header) if
-- the StudentGroup doesn't exist -- caller treats that as 404.
CREATE OR ALTER PROCEDURE [dbo].[usp_attendance_marks_get_bulk]
    @studentGroupId UNIQUEIDENTIFIER,
    @from           DATE,
    @to             DATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @academicYearId UNIQUEIDENTIFIER;
    DECLARE @groupCode      NVARCHAR(10);

    SELECT
        @academicYearId = SG.AcademicYearId,
        @groupCode      = SG.Code
    FROM dbo.StudentGroups AS SG
    WHERE SG.Id = @studentGroupId;

    IF @academicYearId IS NULL
    BEGIN
        RETURN;
    END

    DECLARE @periods TABLE
    (
        AttendanceWeekId   UNIQUEIDENTIFIER NOT NULL,
        AttendancePeriodId UNIQUEIDENTIFIER NOT NULL,
        [Name]             NVARCHAR(128)    NOT NULL,
        StartTime          DATETIME2(7)     NOT NULL,
        EndTime            DATETIME2(7)     NOT NULL,
        IsAmReg            BIT              NOT NULL,
        IsPmReg            BIT              NOT NULL,
        PRIMARY KEY (AttendanceWeekId, AttendancePeriodId)
    );

    INSERT INTO @periods (AttendanceWeekId, AttendancePeriodId, [Name], StartTime, EndTime, IsAmReg, IsPmReg)
    SELECT API.AttendanceWeekId, API.PeriodId, API.[Name], API.ActualStartTime, API.ActualEndTime,
           API.IsAmReg, API.IsPmReg
    FROM dbo.vw_attendance_period_instances AS API
    WHERE API.AcademicYearId = @academicYearId
      AND CAST(API.ActualStartTime AS DATE) BETWEEN @from AND @to;

    -- Roster: regular SGM members whose date-effective window overlaps [from, to],
    -- plus students added as extras to a Session of this group's curriculum during
    -- a week touching the range. Regular wins on conflict.
    DECLARE @roster TABLE
    (
        StudentId UNIQUEIDENTIFIER PRIMARY KEY,
        IsExtra   BIT NOT NULL
    );

    INSERT INTO @roster (StudentId, IsExtra)
    SELECT St.Id, CAST(0 AS BIT)
    FROM dbo.StudentGroupMemberships AS SGM
    JOIN dbo.Students                AS St ON St.Id = SGM.StudentId AND St.IsDeleted = 0
    WHERE SGM.StudentGroupId = @studentGroupId
      AND SGM.StartDate <= DATEADD(DAY, 1, @to)
      AND (SGM.EndDate IS NULL OR SGM.EndDate >= @from);

    INSERT INTO @roster (StudentId, IsExtra)
    SELECT DISTINCT St.Id, CAST(1 AS BIT)
    FROM dbo.SessionExtraNames AS SEN
    JOIN dbo.Sessions          AS S  ON S.Id  = SEN.SessionId
    JOIN dbo.Classes           AS C  ON C.Id  = S.ClassId
    JOIN dbo.CurriculumGroups  AS CG ON CG.Id = C.CurriculumGroupId
    JOIN dbo.AttendanceWeeks   AS AW ON AW.Id = SEN.AttendanceWeekId
    JOIN dbo.Students          AS St ON St.Id = SEN.StudentId AND St.IsDeleted = 0
    WHERE CG.StudentGroupId = @studentGroupId
      -- AttendanceWeek.Beginning is the Monday; widen the lower bound so any
      -- week containing a day in [@from, @to] is considered in scope.
      AND AW.Beginning BETWEEN DATEADD(DAY, -6, @from) AND @to
      AND NOT EXISTS (SELECT 1 FROM @roster R WHERE R.StudentId = St.Id);

    -- 1) Header.
    SELECT
        StudentGroupId = @studentGroupId,
        GroupCode      = @groupCode,
        [From]         = @from,
        [To]           = @to;

    -- 2) Period instances forming the column axis of the grid.
    SELECT
        AttendanceWeekId   = P.AttendanceWeekId,
        AttendancePeriodId = P.AttendancePeriodId,
        PeriodName         = P.[Name],
        StartTime          = P.StartTime,
        EndTime            = P.EndTime,
        IsAmReg            = P.IsAmReg,
        IsPmReg            = P.IsPmReg
    FROM @periods AS P
    ORDER BY P.StartTime, P.[Name];

    -- 3) Roster.
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

    -- 4) Marks for any roster student at any of the in-range period instances.
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
    JOIN @periods            AS P  ON P.AttendanceWeekId   = AM.AttendanceWeekId
                                  AND P.AttendancePeriodId = AM.AttendancePeriodId;
END;
GO
