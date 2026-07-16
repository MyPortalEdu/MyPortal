-- ============================================================================
-- Correct IsSystem flags on three seeded lookups. Forward fix for databases that
-- already ran the original seeds; the seed sources are also corrected so fresh
-- installs and any seed re-run agree with this.
--
--   StaffAbsenceTypes  — DfE CBDS workforce-census categories (codes MAT, SIC,
--     SEC, …). 20260626000300 re-seeded these as IsSystem = 0 when the original
--     20251101000300 seed had them protected; a regression. Same class of data
--     as AttendanceCodes (37/37 system). Restore protection on all 9.
--
--   StaffIllnessTypes  — free-form pastoral categories with no external standard
--     or code references; schools should be able to rename/add/remove them.
--     20260628000200 seeded all 10 as system. Release the 9 real categories;
--     keep only 'Other' protected as the fallback the UI needs.
--
--   DiaryEventTypes 'General' — referenced by the DiaryEventTypes.General
--     constant; must not be user-deletable. Seeded IsSystem = 0; protect it.
--
-- Idempotent: plain UPDATEs converging on the target state.
-- ============================================================================

-- StaffAbsenceTypes: protect the 9 DfE census categories.
UPDATE dbo.StaffAbsenceTypes
SET IsSystem = 1
WHERE Code IN (N'MAT', N'OTH', N'PRG', N'PUB', N'SEC', N'SIC', N'TRN', N'UNA', N'UNP');
GO

-- StaffIllnessTypes: keep 'Other' protected, release the rest.
UPDATE dbo.StaffIllnessTypes
SET IsSystem = 1
WHERE Id = N'C5A1B200-0000-4000-8000-00000000000A';

UPDATE dbo.StaffIllnessTypes
SET IsSystem = 0
WHERE Id <> N'C5A1B200-0000-4000-8000-00000000000A';
GO

-- DiaryEventTypes 'General': protect the well-known row.
UPDATE dbo.DiaryEventTypes
SET IsSystem = 1
WHERE Id = N'84E9DDA4-1BCB-4A2F-8082-FCE51DD04F2A';
GO
