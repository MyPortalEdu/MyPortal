# Student details — feature plan & entity validation

Status: **draft.** The staff-facing student profile inside the staff portal — the student
equivalent of the staff-details feature (`StaffMembersController` + `StaffMemberDetailsPage`).
This is *not* a student-facing portal. It reuses the shared `Person*` layer (services,
contracts, and the `person-emails` / `person-phones` / `person-addresses` / `directory-browser`
components) that staff-details already established.

Reference sources: Capita SIMS *Managing Pupil/Student Records* (2016) for the panel/field
surface, and the reverse-engineered SIMS entity set for the data model. We borrow SIMS's
statutory rigor where it earns its keep and simplify where SIMS is dated legacy.

## Decisions taken

- **Statutory-history only.** Census / Ever-6 / safeguarding-sensitive characteristics move to
  **dated child tables** (FSM periods, pupil premium per year, in-care episodes, SEN needs,
  address occupancy, disability). Ordinary demographics (ethnicity, language, religion,
  nationality, gender identity, etc.) stay as **flat current-only FK columns** on `Person` — we
  accept losing their dated history, which a modern SIS rarely needs.
- **England-only, mainstream.** Welsh/NI/Eire statutory variants are out of scope. Post-16
  (Connexions assent / YSSA) and international (Tier-4 CAS / visa / passport) fields are **P3 /
  deferred**, modelled only if independent/sixth-form students become a priority.
- **Mirror staff-details architecture.** One aggregate root (`Student → Person`), one
  `StudentsController` fanning out to per-tab services, each tab a `Response`+`UpsertRequest`
  pair, one tabbed `StudentDetailsPage` container with a shared data service and a
  `StudentAreaPanel` base class per tab.
- **Reuse the Person spine.** A student *is* a `Person` with a `Student` role row (same shape as
  `StaffMember`). Basic bio, photo, emails, phones, addresses, and documents all reuse the
  existing `Person*` services / contracts / components — no student-specific reimplementation.

## Entity validation — what exists today

The `Student` and `Person` entities are a **strong, census-aware foundation**. Verdict below;
prioritised the way the existing validation-hardening work is (P1 structural → P3 niche).

### Keep as-is (already correct, some ahead of SIMS-2016)
- `Student`: UPN / former UPN / UPN-unknown-reason / ULN / LA child ID; enrolment & boarder
  status; pupil-premium-indicator + FSM-category; service-child / young-carer / kinship-care
  indicators; English proficiency + date; top-up funding, part-time, SEN-unit / resourced-
  provision flags.
- `StudentContactRelationship`: parental-responsibility / correspondence / pupil-report /
  court-order / contact-order — correctly carried on the **join** (matches SIMS `StudRelation`).
- `Person`: title / preferred names / former surname / NHS number / deceased / the full
  demographic FK set (ethnicity, nationality, first language, religion, marital status, sexual
  orientation, gender identity).

### P1 — structural, decide before building the affected tab
Because `Student` currently has **no service layer** (entities only), these refactors are cheap now.
- **Address occupancy.** `AddressPerson` has `IsMain`/`AddressTypeId` but **no `StartDate`/`EndDate`**.
  Add them (SIMS `SimsResidence`) to support address history + a "move house" workflow.
- **FSM → dated periods.** Replace the flat `FsmEligibilityStart/End/ReviewDate` +
  `FreeSchoolMeals` bool on `Student` with a **`StudentFsmEligibility`** child table
  (`StudentId, StartDate, EndDate, Country, Notes`). Keep `FsmReviewDate`. Ever-6 reads the periods.
- **Pupil premium → per-year rows.** Replace the flat `PupilPremium` bool with
  **`StudentPupilPremium`** (`StudentId, AcademicYearId, Eligible, Notes`). Keep
  `PupilPremiumIndicatorId` as the current-year lookup.
- **In-care → episodes.** Replace `InCare` bool with **`StudentCareEpisode`**
  (`StudentId, CaringAuthorityId, StartDate, EndDate`). Keep `PostLookedAfterArrangementId`.
- **SEN → dated needs.** Add **`StudentSenNeed`** (`StudentId, SenTypeId, StartDate, EndDate,
  Description`) alongside the current `SenStatusId`/`SenStartDate` snapshot. (Full SEN
  provision/EHCP/review is its own later sub-module.)

### P2 — missing records to borrow
- **`PersonDisability`** (`PersonId, DisabilityId, StartDate, EndDate, Comments`). The `Disability`
  entity exists and `StaffMemberDisability` exists, but there is **no person/student disability
  link** — `Person.HasMedicalNeeds` is a bare bool. Dated (welfare/statutory).
- **`PersonPreviousName`** (`PersonId, Surname, Forename, MiddleName, ChangeReasonId, DateOfChange`).
  Today only `Person.FormerSurname` exists; SIMS keeps full name-change history for CTF.
- **Medical completeness:** `MedicalEvent` + `PersonMedicalEvent` (immunisation/accident/illness,
  dates), a **medical-practice** link (an `Agency`), and an **emergency-consent** flag + medical
  notes. Conditions (`PersonCondition`) and dietary (`PersonDietaryRequirement`) already exist.
- **`StudentSchoolHistory`** (previous schools + `ReasonForLeaving` + destination institution).
  Today only `Student.DateLeaving` exists.
- **`Contact` depth:** add translator-required + contact language + address-disclosure /
  address-transfer flags (SIMS `StudContact`). **`Sibling`** family links.
- **`StudentParentalConsent`** (`StudentId, ConsentTypeId, Granted, Comments`).
- **Demographic separation:** `Person.HomeLanguage` (distinct from `FirstLanguage`),
  `NationalIdentity` (distinct from `Nationality`), `CountryOfBirth`. Flat FKs (no history).

### P3 — deferred (out of England-mainstream v1)
Connexions assent + YSSA (post-16); passport / visa / CAS (Tier-4, international); traveller
status; asylum/refugee status; mode-of-travel & transport; post-16 employment; uniform
allowance; blood group; birth-certificate-seen; free-milk eligibility; Welsh/NI variants.

## Architecture (mirror of staff-details)

- **Backend:** `StudentsController` at `/api/v1/students`, inheriting
  `BaseDirectoryEntityController<Person>` for the Documents tab, fanning out to per-tab services
  (`IStudentService`, `IStudentContactService`, `IStudentCulturalService`, …). Each writable tab:
  `Student{Area}Response` (GET) + `Student{Area}UpsertRequest` (PUT), child rows as
  `*UpsertItem` (nullable id = insert-or-update). One FluentValidation validator per request.
  Dapper `usp_student_*` getters + a `StudentHeaderRow` projection.
- **Access model (Phase-1 design task).** Staff-details gates on a `(viewer, subject)`
  *relationship* (Self / LineManaged / All). Students need a **distinct resolver** — the viewer is
  always staff, and the relationship is teaches / form-tutor / pastoral-lead / SENCO / all-students.
  Reuse the *shape* of `docs/staff-profile-access.md` (per-endpoint area + access-flags, lazy
  per-section endpoints, sensitive fields in their own area) but define a `StudentAccess` /
  `StudentRelationship` of its own. This is its own short spec before Phase 1 UI.
- **Frontend:** wire `path: 'student'` in `app.routes.ts` (the `features/student/routes.ts` file is
  currently empty), a `StudentDetailsPage` tabbed container (tabs are in-component `AreaKey`s, not
  routes, gated by `computed()` permission signals), a `StudentListPage`, a create dialog, a
  root `StudentsDataService`, `shared/types/student-*.ts` mirroring the contracts, and a
  `StudentAreaPanel` base class per tab. Reuse `person-emails` / `person-phones` /
  `person-addresses` / `person-picker` / `gender-select` / `directory-browser`.

## Tab map (SIMS panel → MyPortal tab)

| Tab | SIMS panel(s) | Notes |
|---|---|---|
| Header | (identity strip) | Name, admission no, year/reg group, enrolment status, photo. |
| Basic Details | Basic Details | Reuse Person bio + photo; add admission no, date starting/leaving, previous names. |
| Registration | Registration | Enrolment/boarder status, UPN/former/local UPN, ULN, admission date, NC year, part-time, house/reg group. |
| Contact Details | Addresses, Telephone/Email | **Fully reused** from Person layer. |
| Family / Contacts | Family/Home | The contacts model — priority-ranked, parental responsibility, court order; sibling links. |
| Cultural | Ethnic/Cultural | Ethnicity, home/first language, religion, nationality, national identity, country of birth, EAL/proficiency. |
| Medical | Medical, Dietary | Conditions, dietary, disabilities, emergency consent, medical events, practice. |
| Welfare | Welfare | In-care episodes, PEP, young carer, child protection, pupil premium, service child. |
| SEN | (read-only SEN in SIMS) | Needs + status; provision/EHCP later. |
| Funding & Additional | Additional Information | FSM periods, service child, top-up. (Transport/Connexions = P3.) |
| School History | School History | Previous schools, leaving reason, destination. |
| Documents | Links/Documents | **Fully reused** via `BaseDirectoryEntityController`. |
| (later) Behaviour / Achievement / Attendance / Timetable | cross-links | Read-only; leverage existing `StudentIncident` / `StudentAchievement` / `StudentDetention`. |

## Phasing

Each phase is a shippable vertical slice. The P1/P2 entity work folds into the phase that first
needs each table.

- **Phase 0 — Access spec + scaffold.** Write the `StudentAccess`/`StudentRelationship` resolver
  spec (sibling to `staff-profile-access.md`); wire the empty `student` route; stub
  `StudentsController` + `StudentsDataService`.
- **Phase 1 — Foundation + identity.** `StudentService`, `StudentAccessService`,
  `StudentRepository`, header row, permissions migration, validators. Header + Basic Details tabs,
  list, create / create-for-person, delete. `PersonPreviousName` (P2).
- **Phase 2 — Registration + Contact.** Registration tab; Contact Details tab (reused). Address
  occupancy dates (P1).
- **Phase 3 — Family / Contacts.** `StudentContactService` + UI; `Contact` depth + `Sibling` (P2).
- **Phase 4 — Cultural + Medical.** Both tabs; `PersonDisability`, medical events/practice,
  demographic separation (P2).
- **Phase 5 — Welfare + SEN.** Dated welfare child tables (`StudentCareEpisode`, PEP, young carer,
  child protection, `StudentPupilPremium`) and `StudentSenNeed` (P1/P2).
- **Phase 6 — Funding + School History + cross-links.** `StudentFsmEligibility` (P1),
  `StudentSchoolHistory`, `StudentParentalConsent` (P2), read-only behaviour/attendance panels.
