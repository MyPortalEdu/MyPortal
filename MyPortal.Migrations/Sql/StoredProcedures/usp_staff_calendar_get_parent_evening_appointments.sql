SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_staff_calendar_get_parent_evening_appointments]
    @staffMemberId UNIQUEIDENTIFIER,
    @from DATETIME2(7),
    @to DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CONCAT('parenteve:', CONVERT(nvarchar(36), a.Id)) AS Id,
        CONCAT('Parent meeting',
               CASE WHEN nm.[Name] IS NOT NULL THEN CONCAT(': ', nm.[Name]) ELSE '' END) AS Title,
        a.[Start]                       AS [Start],
        a.[End]                         AS [End],
        CAST(0 AS bit)                  AS AllDay,
        'ParentEvening'                 AS Category,
        CAST(NULL AS nvarchar(256))     AS Location,
        CAST(NULL AS nvarchar(16))      AS ColourCode
    FROM dbo.ParentEveningAppointments AS a
    JOIN dbo.ParentEveningStaffMembers AS psm ON psm.Id = a.ParentEveningStaffMemberId
    LEFT JOIN dbo.Students AS st ON st.Id = a.StudentId
    OUTER APPLY dbo.fn_person_get_name(st.PersonId, 3, 1, 0) AS nm
    WHERE psm.StaffMemberId = @staffMemberId
      AND a.[Start] < @to
      AND a.[End] > @from;
END;
GO
