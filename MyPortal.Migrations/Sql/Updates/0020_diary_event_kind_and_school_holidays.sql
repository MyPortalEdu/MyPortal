-- ============================================================================
-- Replace the magic-GUID coupling between SQL and seeded DiaryEventTypes with a
-- typed Kind column (mapped to MyPortal.Common.Enums.DiaryEventKind) and add a
-- SchoolHolidays table that mirrors the Detention pattern (1:1 with DiaryEvent).
-- ============================================================================

-- Drop the view so the function it depends on can be altered without dependency churn.
IF OBJECT_ID(N'dbo.vw_attendance_period_instances', N'V') IS NOT NULL
    DROP VIEW dbo.vw_attendance_period_instances;
GO

-- ─── DiaryEventTypes: + Kind ────────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.DiaryEventTypes') AND name = N'Kind'
)
BEGIN
    ALTER TABLE dbo.DiaryEventTypes
        ADD Kind tinyint NOT NULL
            CONSTRAINT DF_DiaryEventTypes_Kind DEFAULT (0);
END
GO

-- Backfill Kind for the seeded system rows + insert PublicHoliday if missing.
-- Kept in lockstep with 0004_seed_uk_data.sql.
MERGE INTO dbo.DiaryEventTypes AS Target
    USING (VALUES
    ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F22', 'EC Activity',      '#c2fdff', 1, 1),
    ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F23', 'Lesson',           '#2677d4', 1, 2),
    ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F24', 'Cover',            '#f0f029', 1, 3),
    ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F25', 'Detention',        '#d62a24', 1, 4),
    ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F26', 'NCC',              '#24d6ac', 1, 5),
    ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F27', 'PPA',              '#24d653', 1, 6),
    ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F28', 'School Holiday',   '#c300ff', 1, 7),
    ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F29', 'Teacher Training', '#ff9500', 1, 9),
    ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F2B', 'Parent Evening',   '#d10486', 1, 10),
    ('84E9DDA4-1BCB-4A2F-8082-FCE51DD04F2C', 'Public Holiday',   '#9b00ff', 1, 8)
    )
    AS Source (Id, Description, ColourCode, IsSystem, Kind)
    ON Target.Id = Source.Id

    WHEN NOT MATCHED THEN
    INSERT (Id, Description, Active, ColourCode, IsSystem, Kind)
    VALUES (Source.Id, Source.Description, 1, Source.ColourCode, Source.IsSystem, Source.Kind)

    WHEN MATCHED THEN
    UPDATE SET Kind = Source.Kind;
GO

-- ─── SchoolHolidays ─────────────────────────────────────────────────────────

IF OBJECT_ID(N'dbo.SchoolHolidays', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SchoolHolidays (
        Id             uniqueidentifier NOT NULL,
        EventId        uniqueidentifier NOT NULL,
        AcademicYearId uniqueidentifier NOT NULL,
        CONSTRAINT PK_SchoolHolidays PRIMARY KEY CLUSTERED (Id)
    );
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_SchoolHolidays_EventId_DiaryEvents'
      AND parent_object_id = OBJECT_ID(N'dbo.SchoolHolidays')
)
BEGIN
    ALTER TABLE dbo.SchoolHolidays
        ADD CONSTRAINT FK_SchoolHolidays_EventId_DiaryEvents
            FOREIGN KEY (EventId) REFERENCES dbo.DiaryEvents(Id);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_SchoolHolidays_EventId'
      AND object_id = OBJECT_ID(N'dbo.SchoolHolidays')
)
BEGIN
    CREATE INDEX IX_SchoolHolidays_EventId ON dbo.SchoolHolidays(EventId);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_SchoolHolidays_AcademicYearId_AcademicYears'
      AND parent_object_id = OBJECT_ID(N'dbo.SchoolHolidays')
)
BEGIN
    ALTER TABLE dbo.SchoolHolidays
        ADD CONSTRAINT FK_SchoolHolidays_AcademicYearId_AcademicYears
            FOREIGN KEY (AcademicYearId) REFERENCES dbo.AcademicYears(Id);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_SchoolHolidays_AcademicYearId'
      AND object_id = OBJECT_ID(N'dbo.SchoolHolidays')
)
BEGIN
    CREATE INDEX IX_SchoolHolidays_AcademicYearId ON dbo.SchoolHolidays(AcademicYearId);
END
GO
