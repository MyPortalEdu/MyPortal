SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Person header for the details page. First (and currently only) result set is
-- the PersonDetailsResponse header; the repository reads it via QueryMultiple so
-- staff/student sub-resource result sets can be appended here later without
-- changing the call shape.
CREATE OR ALTER PROCEDURE [dbo].[usp_person_get_details_by_id]
    @personId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    [P].[Id],
    [P].[Title],
    [P].[PreferredFirstName],
    [P].[PreferredLastName],
    [P].[FirstName],
    [P].[MiddleName],
    [P].[LastName],
    [P].[PhotoId],
    [P].[NhsNumber],
    [P].[Gender],
    [P].[Dob],
    [P].[Deceased],
    [P].[EthnicityId],
    [P].[NationalityId],
    [P].[FirstLanguageId],
    [P].[MaritalStatusId],
    [P].[IsDeleted]
FROM [dbo].[People] [P]
WHERE [P].[Id] = @personId;
END
