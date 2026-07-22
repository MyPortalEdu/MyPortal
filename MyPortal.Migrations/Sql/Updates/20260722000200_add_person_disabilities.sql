-- The person↔disability multi-select link for the student Medical area (mirrors PersonDietaryRequirements,
-- a plain join table with no FKs beyond the join columns). Idempotent guarded create.

IF OBJECT_ID(N'dbo.PersonDisabilities', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[PersonDisabilities] (
    [Id] uniqueidentifier NOT NULL,
    [PersonId] uniqueidentifier NOT NULL,
    [DisabilityId] uniqueidentifier NOT NULL,
    CONSTRAINT PK_PersonDisabilities PRIMARY KEY CLUSTERED ([Id])
    );
END
GO
