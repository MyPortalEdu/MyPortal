SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Returns 1 if any user data is attached to this academic year beyond the
-- calendar artefacts owned by AcademicYearService (terms, weeks, periods,
-- holidays). Used by Update/Delete to gate destructive operations. Pastoral
-- structure (StudentGroup hierarchy) itself is intentionally excluded -- only
-- data attached to those groups (memberships, marksheets, releases, etc.)
-- counts as downstream.
CREATE OR ALTER PROCEDURE [dbo].[sp_academic_year_has_downstream_data_by_id]
    @academicYearId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(CASE WHEN
           EXISTS (SELECT 1 FROM dbo.AttendanceMarks AM
                     JOIN dbo.AttendanceWeeks AW ON AW.Id = AM.AttendanceWeekId
                     JOIN dbo.AcademicTerms   AT ON AT.Id = AW.AcademicTermId
                    WHERE AT.AcademicYearId = @academicYearId)
        OR EXISTS (SELECT 1 FROM dbo.Timetables
                    WHERE AcademicYearId = @academicYearId)
        OR EXISTS (SELECT 1 FROM dbo.Achievements
                    WHERE AcademicYearId = @academicYearId AND IsDeleted = 0)
        OR EXISTS (SELECT 1 FROM dbo.Incidents
                    WHERE AcademicYearId = @academicYearId AND IsDeleted = 0)
        OR EXISTS (SELECT 1 FROM dbo.LogNotes
                    WHERE AcademicYearId = @academicYearId AND IsDeleted = 0)
        OR EXISTS (SELECT 1 FROM dbo.CurriculumBands
                    WHERE AcademicYearId = @academicYearId)
        OR EXISTS (SELECT 1 FROM dbo.StudentGroupMemberships SGM
                     JOIN dbo.StudentGroups SG ON SG.Id = SGM.StudentGroupId
                    WHERE SG.AcademicYearId = @academicYearId)
        OR EXISTS (SELECT 1 FROM dbo.Marksheets M
                     JOIN dbo.StudentGroups SG ON SG.Id = M.StudentGroupId
                    WHERE SG.AcademicYearId = @academicYearId)
        OR EXISTS (SELECT 1 FROM dbo.CurriculumGroups CG
                     JOIN dbo.StudentGroups SG ON SG.Id = CG.StudentGroupId
                    WHERE SG.AcademicYearId = @academicYearId)
        OR EXISTS (SELECT 1 FROM dbo.Activities A
                     JOIN dbo.StudentGroups SG ON SG.Id = A.StudentGroupId
                    WHERE SG.AcademicYearId = @academicYearId)
        OR EXISTS (SELECT 1 FROM dbo.ParentEveningGroups PEG
                     JOIN dbo.StudentGroups SG ON SG.Id = PEG.StudentGroupId
                    WHERE SG.AcademicYearId = @academicYearId)
        OR EXISTS (SELECT 1 FROM dbo.ResultSetReleases RSR
                     JOIN dbo.StudentGroups SG ON SG.Id = RSR.StudentGroupId
                    WHERE SG.AcademicYearId = @academicYearId)
        THEN 1 ELSE 0 END AS bit);
END;
