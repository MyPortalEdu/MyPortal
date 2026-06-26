-- ============================================================================
-- Add a DisplayOrder column to the lookups whose entity implements
-- IOrderedLookupEntity. The services sort by DisplayOrder then Description, so a
-- default of 0 gives plain alphabetical order; the CBDS seed (20260626000300)
-- and the equality seed (20260626000200) set higher values to sink catch-all /
-- "not stated" rows below the list.
--
-- Languages and Nationalities are deliberately excluded: they're large, purely
-- alphabetical lists with no curated order, so their entities stay unordered.
-- Idempotent.
-- ============================================================================

DECLARE @tables TABLE (name sysname);
INSERT INTO @tables (name) VALUES
    ('Ethnicities'),
    ('Religions'),
    ('Disabilities'),
    ('ContractTypes'),
    ('SchoolTypes'),
    ('SchoolPhases'),
    ('IntakeTypes'),
    ('GovernanceTypes'),
    ('SexualOrientations'),
    ('GenderIdentities'),
    ('PhoneNumberTypes'),
    ('RelationshipTypes'),
    ('StaffAbsenceTypes');

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

-- SexualOrientations / GenderIdentities are seeded in 20260626000200 (before this
-- column existed), so their curated order is applied here. Ids match that seed.
-- Most-common option first; self-describe / prefer-not-to-say sink to the bottom.
UPDATE [dbo].[SexualOrientations] SET [DisplayOrder] = CASE [Id]
    WHEN 'A2B3C4D5-0002-4000-8000-000000000001' THEN 1   -- Heterosexual or straight
    WHEN 'A2B3C4D5-0002-4000-8000-000000000002' THEN 2   -- Gay or lesbian
    WHEN 'A2B3C4D5-0002-4000-8000-000000000003' THEN 3   -- Bisexual
    WHEN 'A2B3C4D5-0002-4000-8000-000000000004' THEN 800 -- Other
    WHEN 'A2B3C4D5-0002-4000-8000-000000000005' THEN 900 -- Prefer not to say
    ELSE [DisplayOrder] END;
GO

UPDATE [dbo].[GenderIdentities] SET [DisplayOrder] = CASE [Id]
    WHEN 'A3B4C5D6-0002-4000-8000-000000000001' THEN 1   -- Same as sex registered at birth
    WHEN 'A3B4C5D6-0002-4000-8000-000000000002' THEN 2   -- Different from sex registered at birth
    WHEN 'A3B4C5D6-0002-4000-8000-000000000003' THEN 3   -- Non-binary
    WHEN 'A3B4C5D6-0002-4000-8000-000000000004' THEN 800 -- Prefer to self-describe
    WHEN 'A3B4C5D6-0002-4000-8000-000000000005' THEN 900 -- Prefer not to say
    ELSE [DisplayOrder] END;
GO
