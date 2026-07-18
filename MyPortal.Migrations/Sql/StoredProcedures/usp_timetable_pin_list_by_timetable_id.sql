SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[usp_timetable_pin_list_by_timetable_id]
    @timetableId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM dbo.TimetablePins
     WHERE TimetableId = @timetableId
     ORDER BY CreatedAt;
END;
