SELECT
    H.Id,
    SG.Code,
    SG.[Description] AS [Name],
    SG.Active,
    H.ColourCode,
    MainName.[Name]  AS MainSupervisorName
FROM dbo.Houses                       AS H
JOIN dbo.StudentGroups                AS SG  ON SG.Id = H.StudentGroupId
LEFT JOIN dbo.StudentGroupSupervisors AS SGS ON SGS.Id = SG.MainSupervisorId
LEFT JOIN dbo.StaffMembers            AS SM  ON SM.Id = SGS.SupervisorId
OUTER APPLY dbo.fn_person_get_name(SM.PersonId, 2, 0, 1) AS MainName
WHERE SG.AcademicYearId = @academicYearId
