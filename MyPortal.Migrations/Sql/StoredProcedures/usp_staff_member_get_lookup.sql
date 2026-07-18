SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- {id, name} for every active staff member. Name composed via the shared person-name
-- function (format 3 = First [Middle] Last, preferred names on).
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_member_get_lookup]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT sm.[Id] AS Id, nm.[Name] AS Description
    FROM [dbo].[StaffMembers] sm
    OUTER APPLY [dbo].[fn_person_get_name](sm.[PersonId], 3, 1, 0) AS nm
    WHERE sm.[IsDeleted] = 0 ORDER BY nm.[Name];
END;
GO
