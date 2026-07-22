SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- The contributors (people) attached to a Personal Education Plan, with each contributor's display name.
CREATE OR ALTER PROCEDURE [dbo].[usp_student_pep_contributor_get_by_pep_id]
    @studentPepId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [C].[Id],
           [C].[PersonId],
           [N].[Name] AS [PersonName]
    FROM [dbo].[StudentPepContributors] [C]
    CROSS APPLY [dbo].[fn_person_get_name]([C].[PersonId], 1, 0, 1) [N]
    WHERE [C].[StudentPepId] = @studentPepId;
END;
