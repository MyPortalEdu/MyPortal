SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_attendance_code_get_by_ids]
    @attendanceCodeIds [GuidList] READONLY
AS
BEGIN
    SET NOCOUNT ON;

SELECT
    AC.Id,
    AC.Code,
    AC.Description,
    AC.AttendanceCodeTypeId,
    AC.IsActive,
    AC.IsRestricted,
    AC.IsSystem
FROM [dbo].[AttendanceCodes] AC
    INNER JOIN @attendanceCodeIds CI ON CI.[Value] = AC.[Id];
END;
GO
