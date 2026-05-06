-- Widen Bulletins.Detail from nvarchar(256) to nvarchar(2000) to fit
-- realistic bulletin body content. Idempotent.

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Bulletins')
      AND name = N'Detail'
      AND max_length = 512  -- nvarchar(256) → 256 chars × 2 bytes
)
BEGIN
    ALTER TABLE dbo.Bulletins
        ALTER COLUMN [Detail] NVARCHAR(2000) NOT NULL;
END
GO
