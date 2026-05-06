SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Full read of one house as 2 result sets:
--   1) header (house + StudentGroup scalars), 2) supervisors with their staff
--      member name and a flag indicating which row is the main supervisor.
-- Returns nothing (no header) when the house doesn't exist -- caller maps to 404.
CREATE OR ALTER PROCEDURE [dbo].[sp_house_get_details_by_id]
    @houseId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Houses WHERE Id = @houseId)
    BEGIN
        RETURN;
    END

    -- 1) Header.
    SELECT
        Id             = H.Id,
        AcademicYearId = SG.AcademicYearId,
        Code           = SG.Code,
        [Name]         = SG.[Description],
        Active         = SG.Active,
        Notes          = SG.Notes,
        ColourCode     = H.ColourCode
    FROM dbo.Houses        AS H
    JOIN dbo.StudentGroups AS SG ON SG.Id = H.StudentGroupId
    WHERE H.Id = @houseId;

    -- 2) Supervisors. Compares each row's Id to the StudentGroup's MainSupervisorId
    --    so the IsMainSupervisor flag survives a missing main supervisor (FK is nullable).
    SELECT
        Id               = SGS.Id,
        StaffMemberId    = SGS.SupervisorId,
        StaffMemberName  = SName.[Name],
        Title            = SGS.Title,
        IsMainSupervisor = CAST(CASE WHEN SGS.Id = SG.MainSupervisorId THEN 1 ELSE 0 END AS bit)
    FROM dbo.Houses                       AS H
    JOIN dbo.StudentGroups                AS SG  ON SG.Id = H.StudentGroupId
    JOIN dbo.StudentGroupSupervisors      AS SGS ON SGS.StudentGroupId = SG.Id
    LEFT JOIN dbo.StaffMembers            AS SM  ON SM.Id = SGS.SupervisorId
    OUTER APPLY dbo.fn_person_get_name(SM.PersonId, 2, 0, 1) AS SName
    WHERE H.Id = @houseId
    ORDER BY SGS.Title;
END;
