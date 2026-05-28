SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Staff member details for the staff profile page. Returns two result sets:
--   1) StaffMember header (core + HR fields)
--   2) Person header (biographical fields, same shape as
--      usp_person_get_details_by_id)
-- The repository reads both via QueryMultiple and assembles the nested
-- StaffMemberDetailsResponse. Employment/contract/compliance result sets can be
-- appended here as those slices land without changing the call shape.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_member_get_details_by_id]
    @staffMemberId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

-- 1) StaffMember header
SELECT
    [SM].[Id],
    [SM].[PersonId],
    [SM].[LineManagerId],
    [SM].[InductionStatusId],
    [SM].[Code],
    [SM].[BankName],
    [SM].[BankAccount],
    [SM].[BankSortCode],
    [SM].[NiNumber],
    [SM].[TeacherReferenceNumber],
    [SM].[Qualifications],
    [SM].[IsTeachingStaff],
    [SM].[HasQts],
    [SM].[QtsAwardedDate],
    [SM].[InductionStartDate],
    [SM].[InductionCompletedDate],
    [SM].[HasDisability],
    [SM].[DisabilityDetails],
    [SM].[PpaPeriodsPerWeek],
    [SM].[IsDeleted]
FROM [dbo].[StaffMembers] [SM]
WHERE [SM].[Id] = @staffMemberId;

-- 2) Person header (biographical)
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
FROM [dbo].[StaffMembers] [SM]
    INNER JOIN [dbo].[People] [P] ON [SM].[PersonId] = [P].[Id]
WHERE [SM].[Id] = @staffMemberId;
END
