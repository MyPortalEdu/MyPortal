SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- A person's linked medical conditions.
CREATE OR ALTER PROCEDURE [dbo].[usp_person_condition_get_by_person_id]
    @personId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [PersonId], [MedicalConditionId], [RequiresMedication], [Medication],
           [StartDate], [EndDate], [InfoReceivedDate], [Notes]
    FROM [dbo].[PersonConditions]
    WHERE [PersonId] = @personId;
END;
