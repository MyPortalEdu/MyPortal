SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Allowances for a set of contracts. Full column list (incl. audit + version) so reconcile updates
-- round-trip without zeroing the created/audit columns; soft-deleted rows are excluded.
CREATE OR ALTER PROCEDURE [dbo].[usp_staff_contract_allowance_get_by_contract_ids]
    @contractIds [GuidList] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SCA.[Id], SCA.[StaffContractId], SCA.[AdditionalPaymentTypeId], SCA.[Amount], SCA.[PayFactor],
        SCA.[StartDate], SCA.[EndDate], SCA.[IsSuperannuable], SCA.[IsSubjectToNi], SCA.[IsBenefitInKind],
        SCA.[Reason], SCA.[IsDeleted], SCA.[CreatedById], SCA.[CreatedByIpAddress], SCA.[CreatedAt],
        SCA.[LastModifiedById], SCA.[LastModifiedByIpAddress], SCA.[LastModifiedAt], SCA.[Version]
    FROM [dbo].[StaffContractAllowances] AS SCA
        INNER JOIN @contractIds AS CI ON CI.[Value] = SCA.[StaffContractId]
    WHERE SCA.[IsDeleted] = 0;
END;
