SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER VIEW dbo.vw_session_period_metadata
AS

/* ===========================
   Lesson Monitor Sessions
   =========================== */
SELECT
    S.Id                                        AS SessionId,
    API.AttendanceWeekId                        AS AttendanceWeekId,
    API.PeriodId                                AS PeriodId,
    CG.StudentGroupId                           AS StudentGroupId,
    API.ActualStartTime                         AS StartTime,
    API.ActualEndTime                           AS EndTime,
    API.[Name]                                  AS PeriodName,
    C.Code                                      AS ClassCode,
    TSM.Id                                      AS TeacherId,
    TName.[Name]                                AS TeacherName,
    TR.Id                                       AS RoomId,
    TR.[Name]                                   AS RoomName,
    CASE WHEN CA.Id IS NULL THEN 0 ELSE 1 END   AS IsCover
FROM dbo.SessionPeriods                AS SP
         JOIN dbo.Sessions                      AS S   ON S.Id = SP.SessionId
         JOIN dbo.AttendancePeriods             AS AP  ON AP.Id = SP.PeriodId
         JOIN dbo.vw_attendance_period_instances AS API ON API.PeriodId = AP.Id
         LEFT JOIN dbo.Classes                  AS C   ON C.Id = S.ClassId
         LEFT JOIN dbo.CurriculumGroups         AS CG  ON CG.Id = C.CurriculumGroupId
         LEFT JOIN dbo.CoverArrangements        AS CA  ON CA.SessionId = S.Id
    AND CA.WeekId = API.AttendanceWeekId
-- choose cover teacher/room if present, else the scheduled ones
         LEFT JOIN dbo.StaffMembers             AS TSM ON TSM.Id = COALESCE(CA.TeacherId, S.TeacherId)
         LEFT JOIN dbo.Rooms                    AS TR  ON TR.Id  = COALESCE(CA.RoomId, S.RoomId)
    OUTER APPLY dbo.fn_person_get_name(TSM.PersonId, 2, 0, 1) AS TName
WHERE
    -- session active during the instance window (half-open interval)
    S.StartDate <= API.ActualEndTime
  AND S.EndDate >= API.ActualStartTime

UNION

/* ===========================
   Reg Group Sessions
   =========================== */
SELECT
    NULL                                       AS SessionId,
    API.AttendanceWeekId                       AS AttendanceWeekId,
    API.PeriodId                               AS PeriodId,
    SG.Id                                      AS StudentGroupId,
    API.ActualStartTime                        AS StartTime,
    API.ActualEndTime                          AS EndTime,
    API.[Name]                                 AS PeriodName,
    SG.Code                                    AS ClassCode,
    SM.Id                                      AS TeacherId,
    FName.[Name]                               AS TeacherName,
    R.Id                                       AS RoomId,
    R.[Name]                                   AS RoomName,
    0                                          AS IsCover
FROM dbo.RegGroups                     AS RG
         CROSS JOIN dbo.vw_attendance_period_instances AS API
         LEFT JOIN dbo.StudentGroups            AS SG ON SG.Id = RG.StudentGroupId
         LEFT JOIN dbo.StaffMembers             AS SM ON SM.Id = SG.MainSupervisorId
         LEFT JOIN dbo.Rooms                    AS R  ON R.Id = RG.RoomId
    OUTER APPLY dbo.fn_person_get_name(SM.PersonId, 2, 0, 1) AS FName
WHERE
    API.IsAmReg = 1 OR API.IsPmReg = 1
;
GO
