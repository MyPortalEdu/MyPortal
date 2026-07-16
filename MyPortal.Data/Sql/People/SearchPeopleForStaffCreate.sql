-- People search for the "new staff member" create flow. HR searches existing People (any
-- subtype) so a joiner who already exists (contact, agent, former student, ...) gets a staff
-- role attached to their existing Person rather than a duplicate Person row.
--
-- LEFT JOIN StaffMembers surfaces ExistingStaffMemberId so the UI can block a duplicate staff
-- record and offer a deep-link to the existing profile instead. @like is the caller-built
-- contains pattern ('%term%'); the service guards against empty/too-short terms so this never
-- runs as an unfiltered table scan.
SELECT TOP 25
    P.[Id]            AS PersonId,
    P.[Title],
    P.[FirstName],
    P.[MiddleName],
    P.[LastName],
    P.[PreferredFirstName],
    P.[PreferredLastName],
    P.[Dob],
    SM.[Id]           AS ExistingStaffMemberId
FROM [dbo].[People] P
    LEFT JOIN [dbo].[StaffMembers] SM ON SM.[PersonId] = P.[Id] AND SM.[IsDeleted] = 0
WHERE P.[IsDeleted] = 0
  AND (
        P.[FirstName] LIKE @like
     OR P.[LastName] LIKE @like
     OR P.[PreferredFirstName] LIKE @like
     OR P.[PreferredLastName] LIKE @like
     OR CONCAT(P.[FirstName], ' ', P.[LastName]) LIKE @like
  )
ORDER BY P.[LastName], P.[FirstName];
