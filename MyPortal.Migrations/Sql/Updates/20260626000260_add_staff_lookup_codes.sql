-- ============================================================================
-- Add a Code column to the staff/contact lookups that are about to be reseeded
-- from the DfE CBDS (PhoneNumberTypes, RelationshipTypes, StaffAbsenceTypes), so
-- statutory returns can emit the official code rather than reverse-mapping from
-- the (editable) description. Codes are populated from the CBDS in
-- 20260626000300 (runs after this). Idempotent.
-- ============================================================================

DECLARE @tables TABLE (name sysname);
INSERT INTO @tables (name) VALUES
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
        WHERE object_id = OBJECT_ID(N'dbo.' + @t) AND name = N'Code'
    )
    BEGIN
        SET @sql = N'ALTER TABLE dbo.' + QUOTENAME(@t) + N' ADD Code nvarchar(10) NULL;';
        EXEC sp_executesql @sql;
    END
    FETCH NEXT FROM cur INTO @t;
END
CLOSE cur;
DEALLOCATE cur;
GO
