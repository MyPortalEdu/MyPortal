ALTER TABLE dbo.StudentGroups
    ADD AcademicYearId UNIQUEIDENTIFIER NOT NULL;

ALTER TABLE dbo.StudentGroups
    ADD CONSTRAINT FK_StudentGroups_AcademicYears
        FOREIGN KEY (AcademicYearId)
            REFERENCES dbo.AcademicYears(Id);

ALTER TABLE dbo.ReportCards
    ADD AcademicYearId UNIQUEIDENTIFIER NOT NULL;

ALTER TABLE dbo.ReportCards
    ADD CONSTRAINT FK_ReportCards_AcademicYears
        FOREIGN KEY (AcademicYearId)
            REFERENCES dbo.AcademicYears(Id);