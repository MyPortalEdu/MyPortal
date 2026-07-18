SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[usp_timetable_list_runs]
    @timetableId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM dbo.TimetableRuns
     WHERE TimetableId = @timetableId
     ORDER BY StartedAt DESC;
END;
