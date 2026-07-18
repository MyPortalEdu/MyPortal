SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_school_get_local_pay_zone_id]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 [PayZoneId] FROM [dbo].[Schools] WHERE [IsLocal] = 1;
END;
GO
