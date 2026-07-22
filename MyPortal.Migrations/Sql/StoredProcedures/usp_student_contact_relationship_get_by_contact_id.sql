SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- A contact's associated students (reverse of the Family tab) for the contact profile's Associated
-- Students panel: the join flags + priority with the student's composed display name, admission number
-- and the relationship-type description. Soft-deleted students are excluded. Ordered by student name.
CREATE OR ALTER PROCEDURE [dbo].[usp_student_contact_relationship_get_by_contact_id]
    @contactId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    [SCR].[Id],
    [SCR].[StudentId],
    [N].[Name] AS [StudentName],
    [S].[AdmissionNumber],
    [SCR].[RelationshipTypeId],
    [RT].[Description] AS [RelationshipTypeName],
    [SCR].[HasCorrespondence],
    [SCR].[HasParentalResponsibility],
    [SCR].[HasPupilReport],
    [SCR].[HasCourtOrder],
    [SCR].[ContactOrder]
FROM [dbo].[StudentContactRelationships] [SCR]
    INNER JOIN [dbo].[Students] [S] ON [S].[Id] = [SCR].[StudentId] AND [S].[IsDeleted] = 0
    CROSS APPLY [dbo].[fn_person_get_name]([S].[PersonId], 1, 0, 1) [N]
    LEFT JOIN [dbo].[RelationshipTypes] [RT] ON [RT].[Id] = [SCR].[RelationshipTypeId]
WHERE [SCR].[ContactId] = @contactId
ORDER BY [N].[Name];
END
