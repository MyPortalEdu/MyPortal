-- ============================================================================
-- Align AttendanceCodes with the DfE 2024 School Attendance Codes guidance
-- (https://www.gov.uk/government/publications/school-attendance).
--
-- Changes vs the original 0003 seed:
--   * Add C1, C2, J1, K, Q, Y1–Y7 (new in 2024).
--   * Refresh descriptions on existing codes to match the 2024 wording.
--   * Move X from "Attendance not Required" to "Authorised Absence" (its
--     statistical category in the 2024 model).
--   * Deactivate codes withdrawn in 2024:
--       H — family holiday (agreed); folded into C with headteacher discretion.
--       J — interview; replaced by the granular J1.
--       Y — generic "unable to attend"; replaced by Y1–Y7.
--     Deactivated rather than deleted so any legacy AttendanceMark rows that
--     reference them stay referentially valid.
--
-- "/" and "\" remain separate codes (Present AM / Present PM).
-- ============================================================================

MERGE INTO [dbo].[AttendanceCodes] AS Target
    USING (VALUES
        -- ── Present (in registration) ────────────────────────────────────────
        ('EBACEBAB-153B-452E-B2F4-9CFF11D1B083', '/',  'Present (AM)',
            '59036717-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-163B-452E-B2F4-9CFF11D1B083', '\',  'Present (PM)',
            '59036717-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-203B-452E-B2F4-9CFF11D1B083', 'L',  'Arrived after the register has started but before it has closed',
            '59036717-D349-46D3-B8A6-60FFA9263DB3'),

        -- ── Approved Educational Activity (off-site, counts as attending) ───
        ('EBACEBAB-173B-452E-B2F4-9CFF11D1B083', 'B',  'Educated off site (not K, V, P or W)',
            '59036719-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-323B-452E-B2F4-9CFF11D1B083', 'K',  'Attending provision arranged by the local authority under Section 19 of the EA 1996',
            '59036719-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-243B-452E-B2F4-9CFF11D1B083', 'P',  'Sporting activity with prior agreement from school',
            '59036719-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-293B-452E-B2F4-9CFF11D1B083', 'V',  'Educational visit or trip supervised by school staff',
            '59036719-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-2A3B-452E-B2F4-9CFF11D1B083', 'W',  'Work experience under arrangements by the school or local authority',
            '59036719-D349-46D3-B8A6-60FFA9263DB3'),

        -- ── Authorised Absence ──────────────────────────────────────────────
        ('EBACEBAB-183B-452E-B2F4-9CFF11D1B083', 'C',  'Exceptional circumstance, agreed by the headteacher',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-2F3B-452E-B2F4-9CFF11D1B083', 'C1', 'Regulated performance / undertaking regulated employment abroad',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-303B-452E-B2F4-9CFF11D1B083', 'C2', 'Part-time timetable, agreed by the headteacher and parent(s)/carer(s)',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-193B-452E-B2F4-9CFF11D1B083', 'D',  'Dual registered',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-1A3B-452E-B2F4-9CFF11D1B083', 'E',  'Suspended or permanently excluded; no alternative provision made',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-1E3B-452E-B2F4-9CFF11D1B083', 'I',  'Illness (physical or mental health; not medical/dental appointments)',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-313B-452E-B2F4-9CFF11D1B083', 'J1', 'Job, school or college interview',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-213B-452E-B2F4-9CFF11D1B083', 'M',  'Medical or dental appointment',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-333B-452E-B2F4-9CFF11D1B083', 'Q',  'Unable to attend because of a lack of access arrangements',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-253B-452E-B2F4-9CFF11D1B083', 'R',  'Religious observance (1 day; further days coded C if agreed)',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-263B-452E-B2F4-9CFF11D1B083', 'S',  'Study leave',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-273B-452E-B2F4-9CFF11D1B083', 'T',  'Parent travelling for occupational purposes',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-2B3B-452E-B2F4-9CFF11D1B083', 'X',  'Non-compulsory school age pupil not required to attend',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-343B-452E-B2F4-9CFF11D1B083', 'Y1', 'Unable to attend due to transport provided not being available',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-353B-452E-B2F4-9CFF11D1B083', 'Y2', 'Unable to attend due to widespread transport disruption',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-363B-452E-B2F4-9CFF11D1B083', 'Y3', 'Unable to attend due to part of the school premises being unexpectedly closed',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-373B-452E-B2F4-9CFF11D1B083', 'Y4', 'Unable to attend due to unexpected whole school closure',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-383B-452E-B2F4-9CFF11D1B083', 'Y5', 'Unable to attend; pupil is in criminal justice detention',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-393B-452E-B2F4-9CFF11D1B083', 'Y6', 'Unable to attend in accordance with public health guidance or law',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-3A3B-452E-B2F4-9CFF11D1B083', 'Y7', 'Unable to attend due to other avoidable cause (must affect the pupil)',
            '59036718-D349-46D3-B8A6-60FFA9263DB3'),

        -- ── Unauthorised Absence ────────────────────────────────────────────
        ('EBACEBAB-1C3B-452E-B2F4-9CFF11D1B083', 'G',  'Holiday or absence for leisure-related purposes (not agreed by the headteacher)',
            '59036720-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-223B-452E-B2F4-9CFF11D1B083', 'N',  'Reason for absence not yet established (must be corrected within 5 days)',
            '59036720-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-233B-452E-B2F4-9CFF11D1B083', 'O',  'Absent in other or unknown circumstances',
            '59036720-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-283B-452E-B2F4-9CFF11D1B083', 'U',  'Late after the register has closed',
            '59036720-D349-46D3-B8A6-60FFA9263DB3'),

        -- ── Administrative ──────────────────────────────────────────────────
        ('EBACEBAB-2D3B-452E-B2F4-9CFF11D1B083', 'Z',  'Prospective pupil not yet on register',
            '59036721-D349-46D3-B8A6-60FFA9263DB3'),
        ('EBACEBAB-2E3B-452E-B2F4-9CFF11D1B083', '#',  'Planned whole school closure (e.g. holidays, INSETs, polling station days)',
            '59036721-D349-46D3-B8A6-60FFA9263DB3')
    )
    AS Source (Id, Code, Description, AttendanceCodeTypeId)
    ON Target.Id = Source.Id

    WHEN MATCHED THEN
        UPDATE SET
            Code                 = Source.Code,
            Description          = Source.Description,
            AttendanceCodeTypeId = Source.AttendanceCodeTypeId,
            IsActive             = 1

    WHEN NOT MATCHED BY TARGET THEN
        INSERT (Id, Code, Description, AttendanceCodeTypeId, IsActive, IsRestricted, IsSystem)
        VALUES (Source.Id, Source.Code, Source.Description, Source.AttendanceCodeTypeId, 1, 0, 1);
GO

-- Deactivate codes withdrawn in 2024. Kept in the table so legacy AttendanceMark
-- rows (if any are imported from a previous SIMS-style export) remain valid.
UPDATE [dbo].[AttendanceCodes]
SET IsActive = 0
WHERE Id IN (
    'EBACEBAB-1D3B-452E-B2F4-9CFF11D1B083', -- H (family holiday agreed)
    'EBACEBAB-1F3B-452E-B2F4-9CFF11D1B083', -- J (interview, replaced by J1)
    'EBACEBAB-2C3B-452E-B2F4-9CFF11D1B083'  -- Y (generic, replaced by Y1–Y7)
);
GO

-- Default-restrict the codes with policy / safeguarding / SLT-only implications.
-- Schools can override these via the attendance settings UI — this is just the
-- safer default to ship with.
--   B, K     — off-site / Section 19 provision (dual-provision and legal weight).
--   E        — formal suspension or permanent exclusion (SLT-managed event).
--   C, C1, C2 — require headteacher / parent agreement.
--   Z        — admissions territory; not for class teachers.
--   #        — whole-school closure; bulk-applied by admin staff only.
UPDATE [dbo].[AttendanceCodes]
SET IsRestricted = 1
WHERE Id IN (
    'EBACEBAB-173B-452E-B2F4-9CFF11D1B083', -- B
    'EBACEBAB-323B-452E-B2F4-9CFF11D1B083', -- K
    'EBACEBAB-1A3B-452E-B2F4-9CFF11D1B083', -- E
    'EBACEBAB-183B-452E-B2F4-9CFF11D1B083', -- C
    'EBACEBAB-2F3B-452E-B2F4-9CFF11D1B083', -- C1
    'EBACEBAB-303B-452E-B2F4-9CFF11D1B083', -- C2
    'EBACEBAB-2D3B-452E-B2F4-9CFF11D1B083', -- Z
    'EBACEBAB-2E3B-452E-B2F4-9CFF11D1B083'  -- #
);
GO
