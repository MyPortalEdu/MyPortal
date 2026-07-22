SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Suspension periods for a set of contracts. Full column list (incl. audit + version) so reconcile
-- updates round-trip without zeroing the created/audit columns; soft-deleted rows are excluded.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_contract_suspension_get_by_contract_ids]
    @contractIds [GuidList] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SCS.[Id], SCS.[StaffContractId], SCS.[StartDate], SCS.[EndDate], SCS.[Reason],
        SCS.[IsDeleted], SCS.[CreatedById], SCS.[CreatedByIpAddress], SCS.[CreatedAt],
        SCS.[LastModifiedById], SCS.[LastModifiedByIpAddress], SCS.[LastModifiedAt], SCS.[Version]
    FROM [dbo].[StaffContractSuspensions] AS SCS
        INNER JOIN @contractIds AS CI ON CI.[Value] = SCS.[StaffContractId]
    WHERE SCS.[IsDeleted] = 0;
END;
