SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

CREATE OR ALTER PROCEDURE [dbo].[usp_timetable_update_status]
    @timetableId UNIQUEIDENTIFIER,
    @status INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Timetables
       SET Status = @status,
           LastModifiedAt = SYSUTCDATETIME()
     WHERE Id = @timetableId;
END;
