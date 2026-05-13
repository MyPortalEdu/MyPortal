-- Unified summary across StudentGroups. Subtype tables (Houses, YearGroups,
-- RegGroups, CurriculumGroups) each hold a 1:1 FK back to StudentGroups, so
-- LEFT JOINs against the lot let us materialise a Kind discriminator without
-- needing a column on the base table. New subtype? Add a JOIN and a CASE
-- branch — the enum values are defined in MyPortal.Common.Enums.StudentGroupKind.
--
-- The CASE → alias [Kind] is wrapped in a derived table (Base) so it surfaces
-- as a *real* column at the outer SELECT. Without this, sorting on Kind
-- breaks: QueryKit's paging path emits ORDER BY in a context where SQL Server
-- can't resolve SELECT-level aliases (window function over a top-level alias),
-- producing "Invalid column name 'Kind'". Code/Name/Active are forwarded
-- through Base for symmetry and to keep the column shape stable.
SELECT
    Base.Id,
    Base.Code,
    Base.Name,
    Base.Active,
    Base.Kind
FROM (
    SELECT
        SG.Id,
        SG.Code,
        SG.[Description] AS [Name],
        SG.Active,
        CASE
            WHEN H.Id  IS NOT NULL THEN 1   -- StudentGroupKind.House
            WHEN YG.Id IS NOT NULL THEN 2   -- StudentGroupKind.YearGroup
            WHEN RG.Id IS NOT NULL THEN 3   -- StudentGroupKind.RegGroup
            WHEN CG.Id IS NOT NULL THEN 4   -- StudentGroupKind.CurriculumGroup
            ELSE 0                          -- StudentGroupKind.Other
        END AS [Kind]
    FROM dbo.StudentGroups         AS SG
    LEFT JOIN dbo.Houses           AS H  ON H.StudentGroupId  = SG.Id
    LEFT JOIN dbo.YearGroups       AS YG ON YG.StudentGroupId = SG.Id
    LEFT JOIN dbo.RegGroups        AS RG ON RG.StudentGroupId = SG.Id
    LEFT JOIN dbo.CurriculumGroups AS CG ON CG.StudentGroupId = SG.Id
    WHERE SG.AcademicYearId = @academicYearId
) AS Base
