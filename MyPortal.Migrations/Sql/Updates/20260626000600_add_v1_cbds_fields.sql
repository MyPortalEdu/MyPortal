-- ============================================================================
-- v1 CBDS completeness sweep across all subsystems (exams, SEN, behaviour/
-- exclusion, school/staff). Creates 8 new code-set lookups, brings
-- ExclusionAppealResults into line (Code + DisplayOrder), and adds the scalar /
-- FK fields surfaced by the DataItems gap analysis. Lookups seeded in
-- 20260626000610. All additive (nullable, or NOT NULL with a default). Idempotent.
-- ============================================================================

-- New lookup tables ----------------------------------------------------------
DECLARE @lookups TABLE (name sysname);
INSERT INTO @lookups (name) VALUES
    ('ExamComponentResultTypes'),
    ('AssessmentStages'),
    ('ExclusionReviewCategories'),
    ('SpecialSchoolOrganisations'),
    ('SpecialSchoolTypes'),
    ('PupilPremiumIndicators'),
    ('FsmCategories'),
    ('ClassOfDegrees');

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
    -- ExclusionAppealResults: bring into line with the other ordered lookups
    ('ExclusionAppealResults', 'Code', 'nvarchar(10) NULL'),
    ('ExclusionAppealResults', 'DisplayOrder', 'int NOT NULL CONSTRAINT DF_ExclusionAppealResults_DisplayOrder DEFAULT (0)'),
    -- Exams & assessment
    ('ExamQualifications', 'QualificationNumber', 'nvarchar(8) NULL'),
    ('ExamBaseComponents', 'ResultTypeId', 'uniqueidentifier NULL'),
    ('ExamBaseElements', 'DiscountCode', 'nvarchar(10) NULL'),
    ('ExamAssessments', 'StageId', 'uniqueidentifier NULL'),
    ('ExamAssessments', 'Locale', 'nvarchar(3) NULL'),
    -- Exclusions
    ('Exclusions', 'SessionCount', 'int NULL'),
    ('Exclusions', 'ReinstatementDate', 'datetime2 NULL'),
    ('Exclusions', 'ReviewCategoryId', 'uniqueidentifier NULL'),
    -- Schools
    ('Schools', 'Ukprn', 'nvarchar(8) NULL'),
    ('Schools', 'LowestAge', 'int NULL'),
    ('Schools', 'HighestAge', 'int NULL'),
    ('Schools', 'NetCapacity', 'int NULL'),
    ('Schools', 'NetCapacityAssessmentDate', 'datetime2 NULL'),
    ('Schools', 'IsSpecialSchool', 'bit NOT NULL CONSTRAINT DF_Schools_IsSpecialSchool DEFAULT (0)'),
    ('Schools', 'SpecialSchoolOrganisationId', 'uniqueidentifier NULL'),
    ('Schools', 'SpecialSchoolTypeId', 'uniqueidentifier NULL'),
    ('Schools', 'MaxBoarders', 'int NULL'),
    ('Schools', 'Telephone', 'nvarchar(30) NULL'),
    ('Schools', 'Email', 'nvarchar(256) NULL'),
    -- Students
    ('Students', 'PupilPremiumIndicatorId', 'uniqueidentifier NULL'),
    ('Students', 'FsmCategoryId', 'uniqueidentifier NULL'),
    ('Students', 'SenStartDate', 'datetime2 NULL'),
    -- People
    ('People', 'HasMedicalNeeds', 'bit NOT NULL CONSTRAINT DF_People_HasMedicalNeeds DEFAULT (0)'),
    -- StaffQualifications
    ('StaffQualifications', 'ClassOfDegreeId', 'uniqueidentifier NULL');

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

-- CS105 exclusion-review-result texts are long official sentences; widen the
-- ExclusionAppealResults description column (256 -> 512). Idempotent.
IF EXISTS (SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.ExclusionAppealResults') AND name = N'Description' AND max_length < 1024)
BEGIN
    ALTER TABLE dbo.ExclusionAppealResults ALTER COLUMN [Description] nvarchar(512) NOT NULL;
END
GO

-- Foreign keys ---------------------------------------------------------------
DECLARE @fks TABLE (fk sysname, tbl sysname, col sysname, ref sysname);
INSERT INTO @fks (fk, tbl, col, ref) VALUES
    ('FK_ExamBaseComponents_ResultTypeId_ExamComponentResultTypes', 'ExamBaseComponents', 'ResultTypeId', 'ExamComponentResultTypes'),
    ('FK_ExamAssessments_StageId_AssessmentStages', 'ExamAssessments', 'StageId', 'AssessmentStages'),
    ('FK_Exclusions_ReviewCategoryId_ExclusionReviewCategories', 'Exclusions', 'ReviewCategoryId', 'ExclusionReviewCategories'),
    ('FK_Schools_SpecialSchoolOrganisationId_SpecialSchoolOrganisations', 'Schools', 'SpecialSchoolOrganisationId', 'SpecialSchoolOrganisations'),
    ('FK_Schools_SpecialSchoolTypeId_SpecialSchoolTypes', 'Schools', 'SpecialSchoolTypeId', 'SpecialSchoolTypes'),
    ('FK_Students_PupilPremiumIndicatorId_PupilPremiumIndicators', 'Students', 'PupilPremiumIndicatorId', 'PupilPremiumIndicators'),
    ('FK_Students_FsmCategoryId_FsmCategories', 'Students', 'FsmCategoryId', 'FsmCategories'),
    ('FK_StaffQualifications_ClassOfDegreeId_ClassOfDegrees', 'StaffQualifications', 'ClassOfDegreeId', 'ClassOfDegrees');

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
