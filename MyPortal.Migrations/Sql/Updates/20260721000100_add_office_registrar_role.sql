-- ============================================================================
-- Add an "Office / Registrar" default staff role. SIMS puts bulk pupil-record
-- *edit* on the School Administrator / office profile; MyPortal's seeded roles
-- (20260717000100) only cover Admissions/HR/Finance Officers, leaving no natural
-- home for general student-record maintenance. This role carries the Edit* grants
-- across the student-profile areas (seeded in 20260721000200).
--
-- IsDefault = 1 (identity protected, grants editable), UserType = 1 (Staff).
-- Insert-only MERGE by the well-known Id — a re-run never overwrites a school's
-- edited Description/grants. Nothing references the GUID in code.
-- ============================================================================

MERGE INTO dbo.Roles AS Target
USING (VALUES
    (N'5EED0001-0000-4000-8000-00000000001E', N'Office / Registrar',
     N'School office / registrar. Maintains student records (admissions, registration, contacts).', 1)
) AS Source (Id, Name, Description, UserType)
ON Target.Id = Source.Id
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, Name, NormalizedName, Description, IsSystem, IsDefault, UserType, ConcurrencyStamp)
    VALUES (Source.Id, Source.Name, UPPER(Source.Name), Source.Description, 0, 1, Source.UserType,
            CONVERT(NVARCHAR(36), Source.Id));
GO
