SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Derived siblings (SIMS "Family Links"): distinct other students who share a parental-responsibility
-- contact with the given student, via the StudentContactRelationships join. Both sides of the shared
-- link must carry parental responsibility (matches SIMS; avoids false positives from shared emergency
-- contacts / childminders). Leavers (a leaving date on or before today) are excluded, as are
-- soft-deleted students. Name composed with fn_person_get_name format 1.
CREATE OR ALTER PROCEDURE [dbo].[usp_student_get_siblings_by_id]
    @studentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @today date = CAST(SYSUTCDATETIME() AS date);

SELECT DISTINCT
    [S].[Id],
    [S].[AdmissionNumber],
    [N].[Name] AS [DisplayName]
FROM [dbo].[StudentContactRelationships] [MINE]
    INNER JOIN [dbo].[StudentContactRelationships] [THEIRS]
        ON [THEIRS].[ContactId] = [MINE].[ContactId]
        AND [THEIRS].[StudentId] <> [MINE].[StudentId]
        AND [MINE].[HasParentalResponsibility] = 1
        AND [THEIRS].[HasParentalResponsibility] = 1
    INNER JOIN [dbo].[Students] [S]
        ON [S].[Id] = [THEIRS].[StudentId]
        AND [S].[IsDeleted] = 0
        AND ([S].[DateLeaving] IS NULL OR CAST([S].[DateLeaving] AS date) >= @today)
    CROSS APPLY [dbo].[fn_person_get_name]([S].[PersonId], 1, 0, 1) [N]
WHERE [MINE].[StudentId] = @studentId
ORDER BY [N].[Name];
END
