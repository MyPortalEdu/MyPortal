SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Returns every active AttendanceCode including restricted ones; the register
-- screen surfaces them all and the UI greys out restricted codes for users
-- without UseRestrictedCodes.
CREATE OR ALTER PROCEDURE [dbo].[sp_attendance_code_get_active]
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
    WHERE AC.IsActive = 1
    ORDER BY AC.Code;
END;
GO
