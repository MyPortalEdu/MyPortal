SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A person's linked dietary requirements.
CREATE OR ALTER PROCEDURE [dbo].[usp_person_dietary_requirement_get_by_person_id]
    @personId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [PersonId], [DietaryRequirementId] FROM [dbo].[PersonDietaryRequirements]
    WHERE [PersonId] = @personId;
END;
