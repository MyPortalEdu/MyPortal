SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_staff_member_get_id_by_person_id]
    @personId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 [Id] FROM [dbo].[StaffMembers] WHERE [PersonId] = @personId AND [IsDeleted] = 0;
END;
GO
