-- ============================================================================
-- DfE CBDS data-item fields added to existing entities (gap analysis vs the
-- DataItems catalogue). Creates 8 new code-set lookups and adds the scalar /
-- FK fields they (and the pure-scalar items) hang off. Lookups are seeded in
-- 20260626000510. All added columns are additive (nullable, or NOT NULL with a
-- default) so existing rows are unaffected. Idempotent.
-- ============================================================================

-- New lookup tables ----------------------------------------------------------
DECLARE @lookups TABLE (name sysname);
INSERT INTO @lookups (name) VALUES
    ('StaffOrigins'),
    ('StaffDestinations'),
    ('ServiceChildIndicators'),
    ('YoungCarerIndicators'),
    ('PostLookedAfterArrangements'),
    ('UpnUnknownReasons'),
    ('EnglishProficiencies'),
    ('KinshipCareIndicators');

DECLARE @lt sysname, @lsql nvarchar(max);
DECLARE lcur CURSOR LOCAL FAST_FORWARD FOR SELECT name FROM @lookups;
OPEN lcur;
FETCH NEXT FROM lcur INTO @lt;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(N'dbo.' + @lt, N'U') IS NULL
    BEGIN
        SET @lsql = N'CREATE TABLE dbo.' + QUOTENAME(@lt) + N' ('
            + N'[Id] uniqueidentifier NOT NULL, '
            + N'[Description] nvarchar(256) NOT NULL, '
            + N'[Active] bit NOT NULL CONSTRAINT DF_' + @lt + N'_Active DEFAULT (1), '
            + N'[Code] nvarchar(10) NULL, '
            + N'[DisplayOrder] int NOT NULL CONSTRAINT DF_' + @lt + N'_DisplayOrder DEFAULT (0), '
            + N'CONSTRAINT PK_' + @lt + N' PRIMARY KEY CLUSTERED ([Id]));';
        EXEC sp_executesql @lsql;
    END
    FETCH NEXT FROM lcur INTO @lt;
END
CLOSE lcur; DEALLOCATE lcur;
GO

-- New columns on existing tables ---------------------------------------------
DECLARE @cols TABLE (tbl sysname, col sysname, def nvarchar(200));
INSERT INTO @cols (tbl, col, def) VALUES
    -- People
    ('People', 'FormerSurname', 'nvarchar(256) NULL'),
    -- StaffMembers
    ('StaffMembers', 'HasHlta', 'bit NOT NULL CONSTRAINT DF_StaffMembers_HasHlta DEFAULT (0)'),
    ('StaffMembers', 'HasQtls', 'bit NOT NULL CONSTRAINT DF_StaffMembers_HasQtls DEFAULT (0)'),
    ('StaffMembers', 'HasEyts', 'bit NOT NULL CONSTRAINT DF_StaffMembers_HasEyts DEFAULT (0)'),
    ('StaffMembers', 'IsSeniorLeadership', 'bit NOT NULL CONSTRAINT DF_StaffMembers_IsSeniorLeadership DEFAULT (0)'),
    -- StaffContracts
    ('StaffContracts', 'IsAgencySupply', 'bit NOT NULL CONSTRAINT DF_StaffContracts_IsAgencySupply DEFAULT (0)'),
    ('StaffContracts', 'SafeguardedSalary', 'bit NOT NULL CONSTRAINT DF_StaffContracts_SafeguardedSalary DEFAULT (0)'),
    ('StaffContracts', 'DailyRate', 'bit NOT NULL CONSTRAINT DF_StaffContracts_DailyRate DEFAULT (0)'),
    -- StaffEmployments: Origin/Destination are the arrival/departure pair of an employment spell
    ('StaffEmployments', 'OriginId', 'uniqueidentifier NULL'),
    ('StaffEmployments', 'DestinationId', 'uniqueidentifier NULL'),
    -- Students
    ('Students', 'EnglishProficiencyId', 'uniqueidentifier NULL'),
    ('Students', 'EnglishProficiencyDate', 'datetime2 NULL'),
    ('Students', 'FormerUpn', 'nvarchar(13) NULL'),
    ('Students', 'UpnUnknownReasonId', 'uniqueidentifier NULL'),
    ('Students', 'Uln', 'nvarchar(10) NULL'),
    ('Students', 'LaChildId', 'nvarchar(20) NULL'),
    ('Students', 'InCare', 'bit NOT NULL CONSTRAINT DF_Students_InCare DEFAULT (0)'),
    ('Students', 'CaringAuthorityId', 'uniqueidentifier NULL'),
    ('Students', 'PostLookedAfterArrangementId', 'uniqueidentifier NULL'),
    ('Students', 'TopUpFunding', 'bit NOT NULL CONSTRAINT DF_Students_TopUpFunding DEFAULT (0)'),
    ('Students', 'FsmEligibilityStartDate', 'datetime2 NULL'),
    ('Students', 'FsmEligibilityEndDate', 'datetime2 NULL'),
    ('Students', 'FsmReviewDate', 'datetime2 NULL'),
    ('Students', 'IsPartTime', 'bit NOT NULL CONSTRAINT DF_Students_IsPartTime DEFAULT (0)'),
    ('Students', 'SenUnitMember', 'bit NOT NULL CONSTRAINT DF_Students_SenUnitMember DEFAULT (0)'),
    ('Students', 'ResourcedProvisionMember', 'bit NOT NULL CONSTRAINT DF_Students_ResourcedProvisionMember DEFAULT (0)'),
    ('Students', 'ServiceChildIndicatorId', 'uniqueidentifier NULL'),
    ('Students', 'YoungCarerIndicatorId', 'uniqueidentifier NULL'),
    ('Students', 'KinshipCareIndicatorId', 'uniqueidentifier NULL'),
    -- StudentContactRelationships
    ('StudentContactRelationships', 'ContactOrder', 'int NOT NULL CONSTRAINT DF_StudentContactRelationships_ContactOrder DEFAULT (0)');

DECLARE @tbl sysname, @col sysname, @def nvarchar(200), @csql nvarchar(max);
DECLARE ccur CURSOR LOCAL FAST_FORWARD FOR SELECT tbl, col, def FROM @cols;
OPEN ccur;
FETCH NEXT FROM ccur INTO @tbl, @col, @def;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'dbo.' + @tbl) AND name = @col)
    BEGIN
        SET @csql = N'ALTER TABLE dbo.' + QUOTENAME(@tbl) + N' ADD ' + QUOTENAME(@col) + N' ' + @def + N';';
        EXEC sp_executesql @csql;
    END
    FETCH NEXT FROM ccur INTO @tbl, @col, @def;
END
CLOSE ccur; DEALLOCATE ccur;
GO

-- Foreign keys ---------------------------------------------------------------
DECLARE @fks TABLE (fk sysname, tbl sysname, col sysname, ref sysname);
INSERT INTO @fks (fk, tbl, col, ref) VALUES
    ('FK_StaffEmployments_OriginId_StaffOrigins', 'StaffEmployments', 'OriginId', 'StaffOrigins'),
    ('FK_StaffEmployments_DestinationId_StaffDestinations', 'StaffEmployments', 'DestinationId', 'StaffDestinations'),
    ('FK_Students_EnglishProficiencyId_EnglishProficiencies', 'Students', 'EnglishProficiencyId', 'EnglishProficiencies'),
    ('FK_Students_UpnUnknownReasonId_UpnUnknownReasons', 'Students', 'UpnUnknownReasonId', 'UpnUnknownReasons'),
    ('FK_Students_CaringAuthorityId_LocalAuthorities', 'Students', 'CaringAuthorityId', 'LocalAuthorities'),
    ('FK_Students_PostLookedAfterArrangementId_PostLookedAfterArrangements', 'Students', 'PostLookedAfterArrangementId', 'PostLookedAfterArrangements'),
    ('FK_Students_ServiceChildIndicatorId_ServiceChildIndicators', 'Students', 'ServiceChildIndicatorId', 'ServiceChildIndicators'),
    ('FK_Students_YoungCarerIndicatorId_YoungCarerIndicators', 'Students', 'YoungCarerIndicatorId', 'YoungCarerIndicators'),
    ('FK_Students_KinshipCareIndicatorId_KinshipCareIndicators', 'Students', 'KinshipCareIndicatorId', 'KinshipCareIndicators');

DECLARE @fk sysname, @ftbl sysname, @fcol sysname, @fref sysname, @fsql nvarchar(max);
DECLARE fcur CURSOR LOCAL FAST_FORWARD FOR SELECT fk, tbl, col, ref FROM @fks;
OPEN fcur;
FETCH NEXT FROM fcur INTO @fk, @ftbl, @fcol, @fref;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = @fk
        AND parent_object_id = OBJECT_ID(N'dbo.' + @ftbl))
    BEGIN
        SET @fsql = N'ALTER TABLE dbo.' + QUOTENAME(@ftbl) + N' ADD CONSTRAINT ' + QUOTENAME(@fk)
            + N' FOREIGN KEY (' + QUOTENAME(@fcol) + N') REFERENCES dbo.' + QUOTENAME(@fref) + N'([Id]);';
        EXEC sp_executesql @fsql;
    END
    FETCH NEXT FROM fcur INTO @fk, @ftbl, @fcol, @fref;
END
CLOSE fcur; DEALLOCATE fcur;
GO
