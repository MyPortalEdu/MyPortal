SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Returns events that overlap the [@StartTime, @EndTime] window.
-- All-day events are treated as [StartTime, EndDate + 1 day] so they cover the whole calendar day.
CREATE OR ALTER FUNCTION dbo.fn_diary_event_get_overlapping
    (
    @StartTime      datetime2(7),
    @EndTime        datetime2(7),
    @EventTypeFilter uniqueidentifier = NULL
    )
    RETURNS TABLE
    WITH SCHEMABINDING
    AS
    RETURN
SELECT
    DE.Id,
    DE.EventTypeId,
    DE.CreatedById,
    DE.CreatedAt,
    DE.RoomId,
    DE.Subject,
    DE.Description,
    DE.Location,
    DE.StartTime,
    DE.EndTime,
    DE.IsAllDay,
    DE.[IsPublic],
    DE.[IsSystem]
FROM dbo.DiaryEvents AS DE
WHERE
    (
-- Non-all-day: overlap if Start < @End AND End > @Start  (half-open interval)
    DE.IsAllDay = 0
  AND DE.StartTime < @EndTime
  AND DE.EndTime   > @StartTime
    )
   OR
    (
-- All-day: treat as [Start, End + 1d)
    DE.IsAllDay = 1
  AND DE.StartTime < DATEADD(day, 1, @EndTime)
  AND DE.EndTime   > @StartTime
    )
  AND (
    @EventTypeFilter IS NULL OR DE.EventTypeId = @EventTypeFilter
    );
GO
