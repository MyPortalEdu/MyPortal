SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Returns 1 if any data is attached to this student group that would block its
-- hard-delete. StudentGroup is not soft-deletable, so any of these FK references
-- would either throw a constraint error or silently orphan history if cascaded.
-- Mirrors the AY-level downstream check (usp_academic_year_has_downstream_data_by_id)
-- but scoped to a single group.
CREATE OR ALTER PROCEDURE [dbo].[usp_student_group_has_downstream_data_by_id]
    @studentGroupId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CAST(CASE WHEN
           EXISTS (SELECT 1 FROM dbo.StudentGroupMemberships
                    WHERE StudentGroupId = @studentGroupId)
        OR EXISTS (SELECT 1 FROM dbo.Marksheets
                    WHERE StudentGroupId = @studentGroupId)
        OR EXISTS (SELECT 1 FROM dbo.CurriculumGroups
                    WHERE StudentGroupId = @studentGroupId)
        OR EXISTS (SELECT 1 FROM dbo.Activities
                    WHERE StudentGroupId = @studentGroupId)
        OR EXISTS (SELECT 1 FROM dbo.ParentEveningGroups
                    WHERE StudentGroupId = @studentGroupId)
        OR EXISTS (SELECT 1 FROM dbo.ResultSetReleases
                    WHERE StudentGroupId = @studentGroupId)
        THEN 1 ELSE 0 END AS bit);
END;
