SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Reads a single timetable run by id.
CREATE OR ALTER PROCEDURE [dbo].[usp_timetable_run_get_by_id]
    @runId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM dbo.TimetableRuns WHERE Id = @runId;
END;
GO
