SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Allocations are recurring period blocks; vw_attendance_period_instances supplies the dated
-- occurrences. Clamp each occurrence to the allocation's effective date window.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_calendar_get_non_contact]
    @staffMemberId UNIQUEIDENTIFIER,
    @from DATETIME2(7),
    @to DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CONCAT('ncc:', CONVERT(nvarchar(36), s.Id), ':', CONVERT(nvarchar(36), i.AttendanceWeekId)) AS Id,
        s.Code                          AS Title,
        i.ActualStartTime               AS [Start],
        i.ActualEndTime                 AS [End],
        CAST(0 AS bit)                  AS AllDay,
        'NonContact'                    AS Category,
        CAST(NULL AS nvarchar(256))     AS Location,
        CAST(NULL AS nvarchar(16))      AS ColourCode
    FROM dbo.StaffNonContactAllocations AS s
    JOIN dbo.vw_attendance_period_instances AS i ON i.PeriodId = s.AttendancePeriodId
    WHERE s.StaffMemberId = @staffMemberId
      AND i.ActualStartTime < @to
      AND i.ActualEndTime > @from
      AND s.StartDate <= i.ActualStartTime
      AND s.EndDate >= i.ActualEndTime;
END;
GO
