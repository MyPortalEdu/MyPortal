SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- A student's contact links for the Family tab: the join flags + priority, with the contact's
-- composed display name (fn_person_get_name format 1) and job title, and the relationship-type
-- description. Ordered by priority (unranked 0s last), then contact name.
CREATE OR ALTER PROCEDURE [dbo].[usp_student_contact_relationship_get_by_student_id]
    @studentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    [SCR].[Id],
    [SCR].[ContactId],
    [N].[Name] AS [ContactName],
    [C].[JobTitle],
    [SCR].[RelationshipTypeId],
    [RT].[Description] AS [RelationshipTypeName],
    [SCR].[HasCorrespondence],
    [SCR].[HasParentalResponsibility],
    [SCR].[HasPupilReport],
    [SCR].[HasCourtOrder],
    [SCR].[ContactOrder]
FROM [dbo].[StudentContactRelationships] [SCR]
    INNER JOIN [dbo].[Contacts] [C] ON [C].[Id] = [SCR].[ContactId] AND [C].[IsDeleted] = 0
    CROSS APPLY [dbo].[fn_person_get_name]([C].[PersonId], 1, 0, 1) [N]
    LEFT JOIN [dbo].[RelationshipTypes] [RT] ON [RT].[Id] = [SCR].[RelationshipTypeId]
WHERE [SCR].[StudentId] = @studentId
ORDER BY CASE WHEN [SCR].[ContactOrder] = 0 THEN 1 ELSE 0 END, [SCR].[ContactOrder], [N].[Name];
END
