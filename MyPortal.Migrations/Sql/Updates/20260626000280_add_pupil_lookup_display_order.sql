-- ============================================================================
-- Add DisplayOrder to the pupil-domain lookups reseeded from the CBDS in
-- 20260626000310 (their entities implement IOrderedLookupEntity). Same contract
-- as 20260626000270: NOT NULL DEFAULT 0 = plain alphabetical; the seed sets
-- higher values to sink catch-alls. These tables already carry a Code column, so
-- only DisplayOrder is added here. Idempotent.
-- ============================================================================

DECLARE @tables TABLE (name sysname);
INSERT INTO @tables (name) VALUES
    ('EnrolmentStatus'),
    ('BoarderStatus'),
    ('SenTypes'),
    ('ExclusionTypes'),
    ('ExclusionReasons');

DECLARE @t sysname, @sql nvarchar(max);
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT name FROM @tables;
OPEN cur;
FETCH NEXT FROM cur INTO @t;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.' + @t) AND name = N'DisplayOrder'
    )
    BEGIN
        SET @sql = N'ALTER TABLE dbo.' + QUOTENAME(@t)
            + N' ADD DisplayOrder int NOT NULL CONSTRAINT DF_' + @t
            + N'_DisplayOrder DEFAULT (0);';
        EXEC sp_executesql @sql;
    END
    FETCH NEXT FROM cur INTO @t;
END
CLOSE cur;
DEALLOCATE cur;
GO
