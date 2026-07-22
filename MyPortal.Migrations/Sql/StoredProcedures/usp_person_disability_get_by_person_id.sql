SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A person's linked disabilities.
CREATE OR ALTER PROCEDURE [dbo].[usp_person_disability_get_by_person_id]
    @personId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [PersonId], [DisabilityId] FROM [dbo].[PersonDisabilities]
    WHERE [PersonId] = @personId;
END;
