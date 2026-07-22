SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Basic-details section of the contact profile: person bio (excluding cultural/medical fields —
-- ethnicity, NhsNumber, language, religion, etc. live in their own area procs) plus the Contact-row
-- fields (parental ballot, place of work, job title, NI number).
CREATE OR ALTER PROCEDURE [dbo].[usp_contact_get_basic_details_by_id]
    @contactId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    [C].[Id],
    [C].[PersonId],
    [P].[Title],
    [P].[FirstName],
    [P].[MiddleName],
    [P].[LastName],
    [P].[PreferredFirstName],
    [P].[PreferredLastName],
    [P].[PhotoId],
    [P].[Gender],
    [P].[Dob],
    [P].[Deceased],
    [C].[ParentalBallot],
    [C].[PlaceOfWork],
    [C].[JobTitle],
    [C].[NiNumber]
FROM [dbo].[Contacts] [C]
    INNER JOIN [dbo].[People] [P] ON [C].[PersonId] = [P].[Id]
WHERE [C].[Id] = @contactId;
END
