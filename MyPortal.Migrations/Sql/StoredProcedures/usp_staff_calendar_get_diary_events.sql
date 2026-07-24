SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Public events plus private ones this person attends. Kinds with a dedicated source are
-- excluded so they aren't double-counted: 3 = Cover, 4 = Detention, 10 = ParentEvening.
-- Returns the DiaryEventKind; the calendar maps kind -> colour in the theme.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_calendar_get_diary_events]
    @personId UNIQUEIDENTIFIER,
    @from DATETIME2(7),
    @to DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT
        CONCAT('event:', CONVERT(nvarchar(36), e.Id)) AS Id,
        e.Subject                                     AS Title,
        e.StartTime                                   AS [Start],
        e.EndTime                                     AS [End],
        e.IsAllDay                                    AS AllDay,
        CASE WHEN e.Kind IN (7, 8, 9) THEN 'Holiday' ELSE 'Event' END AS Category,
        COALESCE(e.Location, r.[Name])                AS Location,
        e.Kind                                        AS Kind
    FROM dbo.DiaryEvents AS e
    LEFT JOIN dbo.Rooms AS r ON r.Id = e.RoomId
    LEFT JOIN dbo.DiaryEventAttendees AS a ON a.EventId = e.Id AND a.PersonId = @personId
    WHERE e.StartTime < @to
      AND e.EndTime > @from
      AND e.Kind NOT IN (3, 4, 10)
      AND (e.IsPublic = 1 OR a.Id IS NOT NULL);
END;
GO
