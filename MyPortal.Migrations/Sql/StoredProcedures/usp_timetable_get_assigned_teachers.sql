SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[usp_timetable_get_assigned_teachers]
    @timetableId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DISTINCT SM.*
      FROM dbo.StaffMembers SM
      JOIN dbo.TimetableAssignments TA ON TA.TeacherId = SM.Id
     WHERE TA.TimetableId = @timetableId
       AND SM.IsDeleted = 0;
END;
