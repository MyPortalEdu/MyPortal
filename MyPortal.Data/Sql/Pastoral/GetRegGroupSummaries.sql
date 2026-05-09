SELECT
    RG.Id,
    SG.Code,
    SG.[Description]    AS [Name],
    SG.Active,
    YG.[Name]           AS YearGroupName,
    MainName.[Name]     AS MainSupervisorName
    FROM dbo.RegGroups                    AS RG
    JOIN dbo.StudentGroups                AS SG  ON SG.Id = RG.StudentGroupId
    JOIN dbo.YearGroups                   AS YG  ON YG.Id = RG.YearGroupId
    LEFT JOIN dbo.StudentGroupSupervisors AS SGS ON SGS.Id = SG.MainSupervisorId
    LEFT JOIN dbo.StaffMembers            AS SM  ON SM.Id = SGS.SupervisorId
    OUTER APPLY dbo.fn_person_get_name(SM.PersonId, 2, 0, 1) AS MainName
WHERE SG.AcademicYearId = @academicYearId
