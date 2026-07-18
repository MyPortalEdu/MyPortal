SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Marks a timetable run completed: sets terminal status, completion time and diagnostic.
CREATE OR ALTER PROCEDURE [dbo].[usp_timetable_run_mark_completed]
    @runId       UNIQUEIDENTIFIER,
    @status      INT,
    @completedAt DATETIME2(7),
    @diagnostic  NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.TimetableRuns
          SET Status = @status,
              CompletedAt = @completedAt,
              SolverDiagnostic = @diagnostic
        WHERE Id = @runId;
END;
GO
