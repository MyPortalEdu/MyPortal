SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_staff_calendar_get_detentions]
    @staffMemberId UNIQUEIDENTIFIER,
    @from DATETIME2(7),
    @to DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CONCAT('detention:', CONVERT(nvarchar(36), d.Id)) AS Id,
        e.Subject                                         AS Title,
        e.StartTime                                       AS [Start],
        e.EndTime                                         AS [End],
        e.IsAllDay                                        AS AllDay,
        'Detention'                                       AS Category,
        COALESCE(e.Location, r.[Name])                    AS Location,
        e.Kind                                            AS Kind
    FROM dbo.Detentions AS d
    JOIN dbo.DiaryEvents AS e ON e.Id = d.EventId
    LEFT JOIN dbo.Rooms AS r ON r.Id = e.RoomId
    WHERE d.SupervisorId = @staffMemberId
      AND e.StartTime < @to
      AND e.EndTime > @from;
END;
GO
