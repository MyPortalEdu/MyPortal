SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

GO

-- Pay scale point rates in effect for a pay zone as at a given date.
CREATE OR ALTER PROCEDURE [dbo].[usp_pay_scale_point_rate_get_current_by_zone]
    @payZoneId UNIQUEIDENTIFIER,
    @asOf DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [Id], [PayScalePointId], [PayZoneId], [EffectiveFrom], [EffectiveTo], [AnnualSalary]
    FROM [dbo].[PayScalePointRates]
    WHERE [PayZoneId] = @payZoneId AND [EffectiveFrom] <= @asOf
    AND ([EffectiveTo] IS NULL OR [EffectiveTo] >= @asOf);
END;
