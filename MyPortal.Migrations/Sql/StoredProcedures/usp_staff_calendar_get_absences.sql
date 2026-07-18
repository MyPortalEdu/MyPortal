SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- All-day spans. EndDate is inclusive in the model; the calendar treats all-day end as
-- exclusive, so push it forward a day for display.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_calendar_get_absences]
    @staffMemberId UNIQUEIDENTIFIER,
    @from DATETIME2(7),
    @to DATETIME2(7),
    @includeConfidential BIT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CONCAT('absence:', CONVERT(nvarchar(36), ab.Id)) AS Id,
        COALESCE(t.[Description], 'Absence')             AS Title,
        ab.StartDate                                    AS [Start],
        DATEADD(DAY, 1, ab.EndDate)                     AS [End],
        CAST(1 AS bit)                                  AS AllDay,
        'Absence'                                       AS Category,
        CAST(NULL AS nvarchar(256))                     AS Location,
        CAST(NULL AS nvarchar(16))                      AS ColourCode
    FROM dbo.StaffAbsences AS ab
    LEFT JOIN dbo.StaffAbsenceTypes AS t ON t.Id = ab.AbsenceTypeId
    WHERE ab.StaffMemberId = @staffMemberId
      AND ab.StartDate <= @to
      AND ab.EndDate >= @from
      AND (@includeConfidential = 1 OR ab.IsConfidential = 0);
END;
GO
