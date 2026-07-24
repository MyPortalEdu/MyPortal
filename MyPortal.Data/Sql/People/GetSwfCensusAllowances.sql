
SELECT
    al.[StaffContractId]          AS [ContractId],
    apt.[Code]                    AS [PaymentTypeCode],
    al.[Amount]                   AS [Amount],
    CAST(al.[StartDate] AS date)  AS [PayStartDate],
    CAST(al.[EndDate] AS date)    AS [PayEndDate]
FROM [dbo].[StaffContractAllowances] al
    INNER JOIN [dbo].[AdditionalPaymentTypes] apt ON apt.[Id] = al.[AdditionalPaymentTypeId]
WHERE al.[IsDeleted] = 0
  AND al.[StartDate] <= @referenceDate
  AND (al.[EndDate] IS NULL OR CAST(al.[EndDate] AS date) >= @referenceDate)
ORDER BY al.[StaffContractId];
