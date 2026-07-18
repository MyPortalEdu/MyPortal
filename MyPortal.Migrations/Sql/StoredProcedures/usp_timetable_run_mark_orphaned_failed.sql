SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Fails any runs left queued/running after a host restart. Returns rows affected.
CREATE OR ALTER PROCEDURE [dbo].[usp_timetable_run_mark_orphaned_failed]
    @queued  INT,
    @running INT,
    @failed  INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.TimetableRuns
          SET Status = @failed,
              CompletedAt = SYSUTCDATETIME(),
              SolverDiagnostic = 'Orphaned by host restart.'
        WHERE Status IN (@queued, @running);

    SELECT @@ROWCOUNT;
END;
GO
