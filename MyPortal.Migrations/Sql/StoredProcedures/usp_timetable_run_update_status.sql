SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Updates the status of a single timetable run.
CREATE OR ALTER PROCEDURE [dbo].[usp_timetable_run_update_status]
    @runId  UNIQUEIDENTIFIER,
    @status INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.TimetableRuns SET Status = @status WHERE Id = @runId;
END;
GO
