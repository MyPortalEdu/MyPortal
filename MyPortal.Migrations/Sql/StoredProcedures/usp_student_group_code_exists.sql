SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Returns 1 if another student group in the same academic year already uses this code.
-- Codes must be unique per academic year across the whole pastoral hierarchy (houses,
-- year groups and reg groups all persist as StudentGroups). Pass @excludeStudentGroupId
-- on update so a group doesn't clash with itself. Comparison uses the column's default
-- (case-insensitive) collation, so "7A" and "7a" are treated as the same code.
CREATE OR ALTER PROCEDURE [dbo].[usp_student_group_code_exists]
    @academicYearId        UNIQUEIDENTIFIER,
    @code                  NVARCHAR(10),
    @excludeStudentGroupId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(CASE WHEN EXISTS (
        SELECT 1
        FROM dbo.StudentGroups AS SG
        WHERE SG.AcademicYearId = @academicYearId
          AND SG.Code = @code
          AND (@excludeStudentGroupId IS NULL OR SG.Id <> @excludeStudentGroupId)
    ) THEN 1 ELSE 0 END AS bit);
END;
GO
