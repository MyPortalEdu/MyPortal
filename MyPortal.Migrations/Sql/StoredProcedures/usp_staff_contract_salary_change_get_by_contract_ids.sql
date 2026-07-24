SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Salary / pay-point change history for a set of contracts, newest first. The audit stamp is the
-- changed-by / changed-on; the changing user's person name is resolved here so the service doesn't
-- round-trip per row.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_contract_salary_change_get_by_contract_ids]
    @contractIds [GuidList] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SCC.[Id],
        SCC.[StaffContractId],
        SCC.[OldPayScalePointId],
        SCC.[NewPayScalePointId],
        SCC.[OldAnnualSalary],
        SCC.[NewAnnualSalary],
        SCC.[CreatedAt] AS [ChangedAt],
        NM.[Name]       AS [ChangedBy]
    FROM [dbo].[StaffContractSalaryChanges] AS SCC
        INNER JOIN @contractIds AS CI ON CI.[Value] = SCC.[StaffContractId]
        LEFT JOIN [dbo].[Users] AS U ON U.[Id] = SCC.[CreatedById]
        OUTER APPLY [dbo].[fn_person_get_name](U.[PersonId], 3, 1, 0) AS NM
    ORDER BY SCC.[CreatedAt] DESC;
END;
