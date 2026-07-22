SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- The employer contribution rate in effect on @asOf, one row per scheme. Rates are effective-dated
-- (close-and-insert), so an open EffectiveTo means still current.
CREATE OR ALTER PROCEDURE [dbo].[usp_superannuation_scheme_rate_get_current]
    @asOf DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [SuperannuationSchemeId], [EffectiveFrom], [EffectiveTo], [EmployerRate]
    FROM [dbo].[SuperannuationSchemeRates]
    WHERE [EffectiveFrom] <= @asOf
      AND ([EffectiveTo] IS NULL OR [EffectiveTo] >= @asOf);
END;
