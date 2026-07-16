-- ============================================================================
-- Person photo support.
--
-- Groundwork for uploading/serving a person's photo via the existing
-- People.PhotoId -> Photos -> Documents -> file-storage chain (see the
-- 20251101000800 restructure that moved photos onto the Documents pipeline).
--
--   (a) Complete the Photos audit block. The Photo entity implements
--       IAuditableEntity, but the 20251101000800 restructure only added
--       DocumentId + Version to dbo.Photos and never added the audit columns —
--       so EntityRepository.InsertAsync would write to columns that don't exist.
--       The table is empty (nothing has ever written a Photo row), so the
--       NOT NULL columns can be added without a backfill.
--
--   (b) Seed one *system* Photos directory. A photo's Document must reference a
--       Directory (Documents.DirectoryId is NOT NULL), but it must NOT live in
--       any person's attachments subtree — the mp-directory-browser is scoped to
--       each person's own root and doesn't filter by type, so a photo document
--       anywhere under a person's root would show in their Documents tab and be
--       deletable there. Parking it in this dedicated system directory (which no
--       browser is ever scoped to) keeps it invisible + undeletable by
--       construction.
--
--   (c) Seed a "Photograph" document type. IsSystem = 1 and every facet flag = 0
--       so it never appears in the staff/student/etc. upload classification picker.
--
-- Deterministic GUIDs mirrored in MyPortal.Common.Constants.SystemPhotos.
-- Idempotent: guarded column adds + insert-only MERGE.
-- ============================================================================

-- (a) Photos audit columns (NOT NULL — table is empty; EntityRepository sets them on insert).
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID(N'dbo.Photos') AND name = N'CreatedById')
BEGIN
    ALTER TABLE [dbo].[Photos]
        ADD [CreatedById] UNIQUEIDENTIFIER NOT NULL,
            [CreatedByIpAddress] NVARCHAR(45) NOT NULL,
            [CreatedAt] DATETIME2(7) NOT NULL,
            [LastModifiedById] UNIQUEIDENTIFIER NOT NULL,
            [LastModifiedByIpAddress] NVARCHAR(45) NOT NULL,
            [LastModifiedAt] DATETIME2(7) NOT NULL;
END
GO

-- (b) System Photos directory. UploadPolicy defaults to 0 (StaffOnly); CreatedById/
-- LastModifiedById are nullable on Directories and left NULL (mirrors the seeded root
-- directory in 20251101000300).
MERGE INTO [dbo].[Directories] AS Target
    USING (VALUES
        ('0F0705D1-0000-4000-8000-000000000001', NULL, 'system-photos', 1)
    )
    AS Source (Id, ParentId, Name, Private)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN
    INSERT (Id, ParentId, Name, IsPrivate, UploadPolicy,
            CreatedAt, CreatedByIpAddress, LastModifiedAt, LastModifiedByIpAddress)
    VALUES (Id, ParentId, Name, Private, 0,
            SYSUTCDATETIME(), '::1', SYSUTCDATETIME(), '::1');
GO

-- (c) "Photograph" document type — system, no facet flags (never offered in the picker).
MERGE INTO [dbo].[DocumentTypes] AS Target
    USING (VALUES
        ('5DD555DE-0C38-4FCC-BB54-C3C4A7E81201', 'Photograph')
    )
    AS Source (Id, Description)
    ON Target.Id = Source.Id
    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Student, Staff, Contact, General, IsSend, Active, IsSystem)
    VALUES (Id, Description, 0, 0, 0, 0, 0, 1, 1);
GO
