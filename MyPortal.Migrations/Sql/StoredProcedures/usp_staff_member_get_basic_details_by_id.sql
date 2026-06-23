SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Basic-details section of the staff profile: person bio (excluding equality fields
-- — NhsNumber and EthnicityId live in the EqualityDetails proc when that area ships)
-- plus the staff code. Other staff fields belong to their own areas.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_member_get_basic_details_by_id]
    @staffMemberId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    [SM].[Id],
    [SM].[PersonId],
    [SM].[Code],
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
    [P].[NationalityId],
    [P].[FirstLanguageId],
    [P].[MaritalStatusId]
FROM [dbo].[StaffMembers] [SM]
    INNER JOIN [dbo].[People] [P] ON [SM].[PersonId] = [P].[Id]
WHERE [SM].[Id] = @staffMemberId;
END
