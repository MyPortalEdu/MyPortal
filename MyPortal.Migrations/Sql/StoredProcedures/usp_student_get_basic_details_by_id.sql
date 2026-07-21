SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Basic-details section of the student profile: person bio (excluding cultural/medical fields —
-- ethnicity, NhsNumber, language, religion, etc. live in their own area procs) plus the admission
-- number. Registration, UPN/ULN and pastoral placement belong to the Registration area.
CREATE OR ALTER PROCEDURE [dbo].[usp_student_get_basic_details_by_id]
    @studentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    [S].[Id],
    [S].[PersonId],
    [S].[AdmissionNumber],
    [P].[Title],
    [P].[FirstName],
    [P].[MiddleName],
    [P].[LastName],
    [P].[PreferredFirstName],
    [P].[PreferredLastName],
    [P].[PhotoId],
    [P].[Gender],
    [P].[Dob],
    [P].[Deceased]
FROM [dbo].[Students] [S]
    INNER JOIN [dbo].[People] [P] ON [S].[PersonId] = [P].[Id]
WHERE [S].[Id] = @studentId;
END
