SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Contracts for a set of employments. Full column list (incl. audit + version) so reconcile
-- updates round-trip without zeroing the created/audit columns; soft-deleted rows are excluded.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_contract_get_by_employment_ids]
    @employmentIds [GuidList] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SC.[Id], SC.[StaffEmploymentId], SC.[ContractTypeId], SC.[StaffRoleId], SC.[ServiceTermId],
        SC.[DepartmentId], SC.[PayScaleId], SC.[PayScalePointId], SC.[PostId], SC.[SuperannuationSchemeId],
        SC.[NiContractedOut], SC.[PostTitle], SC.[StartDate],
        SC.[EndDate], SC.[Fte], SC.[HoursPerWeek], SC.[WeeksPerYear], SC.[AnnualSalary], SC.[IsAgencySupply],
        SC.[SafeguardedSalary], SC.[DailyRate], SC.[IsDeleted], SC.[CreatedById], SC.[CreatedByIpAddress],
        SC.[CreatedAt], SC.[LastModifiedById], SC.[LastModifiedByIpAddress], SC.[LastModifiedAt], SC.[Version]
    FROM [dbo].[StaffContracts] AS SC
        INNER JOIN @employmentIds AS EI ON EI.[Value] = SC.[StaffEmploymentId]
    WHERE SC.[IsDeleted] = 0;
END;
