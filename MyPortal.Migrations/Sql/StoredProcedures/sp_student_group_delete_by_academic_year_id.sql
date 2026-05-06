SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Tears down the entire pastoral hierarchy for the AY. StudentGroup.MainSupervisorId
-- points at StudentGroupSupervisor.Id, but the supervisor also points back via
-- StudentGroupId -- circular FK. Null out the back-reference first, then delete
-- children in FK-safe order. The caller is responsible for first checking that
-- no membership / marksheet / etc. data references these groups (see
-- sp_academic_year_has_downstream_data_by_id).
CREATE OR ALTER PROCEDURE [dbo].[sp_student_group_delete_by_academic_year_id]
    @academicYearId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.StudentGroups
       SET MainSupervisorId = NULL
     WHERE AcademicYearId = @academicYearId
       AND MainSupervisorId IS NOT NULL;

    -- RegGroups reference both StudentGroup and YearGroup, so they go before YearGroups.
    DELETE RG
      FROM dbo.RegGroups RG
      JOIN dbo.StudentGroups SG ON SG.Id = RG.StudentGroupId
     WHERE SG.AcademicYearId = @academicYearId;

    DELETE YG
      FROM dbo.YearGroups YG
      JOIN dbo.StudentGroups SG ON SG.Id = YG.StudentGroupId
     WHERE SG.AcademicYearId = @academicYearId;

    DELETE H
      FROM dbo.Houses H
      JOIN dbo.StudentGroups SG ON SG.Id = H.StudentGroupId
     WHERE SG.AcademicYearId = @academicYearId;

    DELETE SGS
      FROM dbo.StudentGroupSupervisors SGS
      JOIN dbo.StudentGroups SG ON SG.Id = SGS.StudentGroupId
     WHERE SG.AcademicYearId = @academicYearId;

    DELETE FROM dbo.StudentGroups
    WHERE AcademicYearId = @academicYearId;
END;
