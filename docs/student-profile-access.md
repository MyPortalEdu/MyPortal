# Student profile access — gating spec

Status: **draft (v1).** The gating model for the staff-facing student profile page. Unlike the staff
profile (`docs/staff-profile-access.md`), this is **flat and role-based**: access is decided by
*which data-sensitivity area* an endpoint serves and *which role* the viewer holds — **not** by any
viewer→student relationship. A staff member who holds `Student.ViewStudentBasicDetails` can open
*any* pupil, exactly as SIMS works (a teacher profile sees the whole pupil population).

This means there is **no `StudentAccessService`, no `StudentRelationship`, and no `Own/Managed/All`
scoping** — endpoints gate with the standard `[Permission(PermissionMode.RequireAny, …)]` attribute
used across the rest of the API.

## Why flat (and not relationship-scoped like staff)

The staff profile scopes by line-management because staff data (salary, appraisal, absence) is
personal to the individual. The pupil record is different: it's operational data that teaching staff
legitimately need for *any* child they might teach, cover, or supervise on duty. Confirmed against
the SIMS 7 permissions matrix (`~/Development/sims/perms.xls`):

- Every pupil-record permission is a plain **View / Edit** — SIMS has **no** `View (Own)` /
  `(Managed)` variant anywhere on the `Student > Student Details` tree.
- SIMS *does* have relationship scoping, but reserves it for **authored content and line-management**
  (your marksheets, homework you set, staff you manage) — never for record visibility.
- Differentiation is entirely by **data-sensitivity panel** (separate permissions per panel) and by
  **role** (which roles are granted each panel).

If a school later wants "form tutors only see their tutees," that's an enhancement *beyond* SIMS and
would be added as an optional scope on top of this flat base — not assumed now.

## Permission areas (10)

`Student.{View|Edit}Student{Area}`, `Area ∈`:

`BasicDetails` (identity, contact methods, addresses) · `Registration` · `Family` (contacts,
parental responsibility, siblings) · `Cultural` (ethnicity/language/religion/nationality —
special-category) · `Medical` (conditions/dietary/disabilities — health) · `Welfare`
(safeguarding/LAC/child-protection/young-carer/pupil-premium — **most sensitive**) · `Sen` ·
`Funding` (FSM/top-up) · `SchoolHistory` · `Documents`.

Areas are server-internal permission *domains*, not UI sections — the FE may render one area as
several sidebar panels, and composes its sidebar from the viewer's permission claim (no relationship
is returned on the header). No section is queried or serialised for a viewer lacking its View
permission (GDPR data minimisation). Minimum to open the page: `Student.ViewStudentBasicDetails`.

## Enforcement

- **GET** `{area}` endpoint → `[Permission(PermissionMode.RequireAny, Permissions.Student.ViewStudent{Area})]`.
- **PUT/POST/DELETE** `{area}` → `[Permission(PermissionMode.RequireAny, Permissions.Student.EditStudent{Area})]`.
- The Documents tab inherits `BaseDirectoryEntityController<Person>`; gate its actions with
  `ViewStudentDocuments` / `EditStudentDocuments`.
- No `studentId`-dependent authorisation — access does not vary by subject, so a per-request access
  service is unnecessary. (Existence/404 is a data concern, handled in the service.)

## Default role grants (SIMS-derived guidance)

Catalogue and grants are separate migrations (as with staff). The permission catalogue is seeded by
`20260721000000_add_student_profile_permissions.sql`; the **grants below are a follow-up seed**
(sibling to `20260717000300_seed_default_role_permissions.sql`). Mapped from the SIMS matrix onto
MyPortal's seeded roles. `V` = View, `E` = Edit.

| Area | Teacher | Teaching Asst | Form Tutor | Head of Year | SENCo | SLT |
|---|:--:|:--:|:--:|:--:|:--:|:--:|
| BasicDetails  | V | V | V | V | V·E | V |
| Registration  | V | V | V | V·E | V·E | V |
| Family        | V | V | V | V | V·E | V |
| Cultural      | V | V | V | V | V·E | V |
| Medical       | V | V | V | V·E | V·E | V |
| Sen           | V·E | V | V | V | V·E | V |
| Funding       | V | – | V | V | V·E | V |
| SchoolHistory | V | V | V | V | V·E | V |
| **Welfare**   | – | – | – | – | **V·E** | **V** |
| Documents     | V | V | V | V | V·E | V |

Notes: **Welfare** is the sharply gated row — SENCo/SLT only, no teaching or tutor access (matches
SIMS, where Class Teacher / Form Tutor / Pastoral Manager get neither view nor edit). **Edit** on the
core record is otherwise office/admin + SENCo in SIMS — **MyPortal has no generic "Office /
Registrar" role** (only Admissions/HR/Finance Officers), so record-maintenance edit currently lands
on SENCo/SLT above. *Open item:* decide whether to add an **Office/Registrar** default role to carry
the bulk record-edit grants (recommended — it's the natural home for `Edit*` on most areas).

## Open items for implementation

- Follow-up grant seed migration implementing the table above (once the Office/Registrar role
  question is settled).
- If/when a pastoral "own tutees only" scope is wanted, add it as an optional relationship filter on
  top of the flat permission — not a replacement for it.
