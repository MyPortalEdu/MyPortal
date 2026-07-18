SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Inserts a single timetable run row from the entity supplied by the caller.
CREATE OR ALTER PROCEDURE [dbo].[usp_timetable_run_add]
    @Id               UNIQUEIDENTIFIER,
    @TimetableId      UNIQUEIDENTIFIER,
    @Status           INT,
    @StartedAt        DATETIME2(7),
    @CompletedAt      DATETIME2(7),
    @SolverDiagnostic NVARCHAR(MAX),
    @InputSnapshot    NVARCHAR(MAX),
    @TriggeredById    UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.TimetableRuns
        (Id, TimetableId, Status, StartedAt, CompletedAt, SolverDiagnostic, InputSnapshot, TriggeredById)
      VALUES
        (@Id, @TimetableId, @Status, @StartedAt, @CompletedAt, @SolverDiagnostic, @InputSnapshot, @TriggeredById);
END;
GO
