SELECT
    YG.Id,
    SG.Code,
    SG.[Description]    AS [Name],
    SG.Active,
    CYG.[Name]          AS CurriculumYearGroupName,
    MainName.[Name]     AS MainSupervisorName
FROM dbo.YearGroups                   AS YG
JOIN dbo.StudentGroups                AS SG  ON SG.Id = YG.StudentGroupId
JOIN dbo.CurriculumYearGroups         AS CYG ON CYG.Id = YG.CurriculumYearGroupId
LEFT JOIN dbo.StudentGroupSupervisors AS SGS ON SGS.Id = SG.MainSupervisorId
LEFT JOIN dbo.StaffMembers            AS SM  ON SM.Id = SGS.SupervisorId
OUTER APPLY dbo.fn_person_get_name(SM.PersonId, 2, 0, 1) AS MainName
WHERE SG.AcademicYearId = @academicYearId
