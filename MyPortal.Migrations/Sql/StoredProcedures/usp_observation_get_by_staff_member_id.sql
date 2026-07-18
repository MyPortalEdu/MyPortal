SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Observations where the staff member is the observee. Lean record table (no audit / soft-delete);
-- rows are hard-deleted on reconcile.
CREATE OR ALTER PROCEDURE [dbo].[usp_observation_get_by_staff_member_id]
    @staffMemberId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [Date], [ObserveeId], [ObserverId], [OutcomeId], [Focus], [SubjectObserved],
        [Strengths], [AreasForDevelopment], [Notes] FROM [dbo].[Observations]
    WHERE [ObserveeId] = @staffMemberId;
END;
