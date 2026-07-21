SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Highest UPN serial (the final 3 digits) already allocated for a given 9-character prefix
-- (LA code + establishment number + 2-digit allocation year), or -1 if none. Used to suggest the
-- next serial when generating a UPN. Only permanent UPNs contribute — TRY_CAST yields NULL for a
-- temporary UPN's alphabetic serial, so those are ignored.
CREATE OR ALTER PROCEDURE [dbo].[usp_student_get_max_upn_serial]
    @prefix9 CHAR(9)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT ISNULL(MAX(TRY_CAST(SUBSTRING([Upn], 11, 3) AS int)), -1)
    FROM [dbo].[Students]
    WHERE [IsDeleted] = 0
      AND LEN([Upn]) = 13
      AND SUBSTRING([Upn], 2, 9) = @prefix9;
END;
GO
