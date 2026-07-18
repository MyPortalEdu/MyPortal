SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Returns 1 if another school already uses this URN. A school's URN (Unique Reference Number) is
-- nationally unique, but the table has no unique index, so guard in the service. Pass
-- @excludeSchoolId on update so a school doesn't clash with itself.
CREATE OR ALTER PROCEDURE [dbo].[usp_school_urn_exists]
    @urn            NVARCHAR(128),
    @excludeSchoolId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(CASE WHEN EXISTS (
        SELECT 1
        FROM dbo.Schools AS S
        WHERE S.Urn = @urn
          AND (@excludeSchoolId IS NULL OR S.Id <> @excludeSchoolId)
    ) THEN 1 ELSE 0 END AS bit);
END;
GO
