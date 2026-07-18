SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- vw_session_period_metadata already resolves cover (the effective TeacherId/RoomName) and
-- expands weekly slots to dated instances, so we just filter to this teacher + window.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_calendar_get_lessons]
    @staffMemberId UNIQUEIDENTIFIER,
    @from DATETIME2(7),
    @to DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CONCAT('lesson:',
               CONVERT(nvarchar(36), COALESCE(m.SessionId, CAST(0x0 AS uniqueidentifier))), ':',
               CONVERT(nvarchar(36), m.PeriodId), ':',
               CONVERT(nvarchar(36), m.AttendanceWeekId))   AS Id,
        COALESCE(m.ClassCode, m.PeriodName)                 AS Title,
        m.StartTime                                         AS [Start],
        m.EndTime                                           AS [End],
        CAST(0 AS bit)                                      AS AllDay,
        CASE WHEN m.IsCover = 1 THEN 'Cover' ELSE 'Lesson' END AS Category,
        m.RoomName                                          AS Location,
        CAST(NULL AS nvarchar(16))                          AS ColourCode
    FROM dbo.vw_session_period_metadata AS m
    WHERE m.TeacherId = @staffMemberId
      AND m.StartTime < @to
      AND m.EndTime > @from;
END;
GO
