# Staff profile access — resolver & gating spec

Status: **agreed.** This is the gating model for the staff profile page. Each endpoint declares
a *permission area* (the data domain it serves) and the kinds of `StaffAccess` that would grant
it; the resolver answers a single question — "does this viewer hold any of those at a scope
that covers this subject?". The front end composes its sidebar from the viewer's permission
claim + the relationship the header returns. The server doesn't enumerate UI sections; it just
enforces at every endpoint.

## Decisions taken

- **Relationship-aware.** Access depends on the `(viewer, subject)` pair: a staff member sees
  their own record, a line manager sees more for their reports, HR/SLT see everyone.
- **Per-endpoint area + access-flags gating.** Each endpoint passes a `StaffArea` (server-internal
  permission domain) and a flags combo of `StaffAccess` values (`ViewOwn | ViewAll`, etc.). The
  resolver looks up the seeded permission constant for each `(area, access)` pair and grants when
  the viewer holds it AND their relationship satisfies that access's scope.
- **9 permission areas, not 13 UI sections.** Areas are server-internal *permission domains*, not
  UI sections — the FE may render a single area as several sidebar panels. The collapse rationale
  is below; the upshot is fewer constants (~37 vs the earlier 61) and fewer cross-cutting "which
  audience sees what" decisions.
- **Per-section endpoints, lazy.** Each profile panel is its own gated endpoint, fetched on
  demand. No section data is queried or serialized for a viewer who can't see it (GDPR data
  minimisation). The header endpoint returns identity + the viewer's relationship to the subject,
  not a capability map — the FE composes its own.
- **Scope encoded in the permission name** (`Staff.{Verb}{Scope}Staff{Area}`), matching the
  existing `ViewAll/Managed/OwnStaffDocuments` convention. `IAuthorizationService` stays a pure
  `HasPermission(string)` check; scope is just more strings.
- **`Managed` is transitive.** The viewer manages the subject if their StaffMember appears
  *anywhere* above the subject in the `LineManagerId` chain — any depth. Matches how schools
  delegate (a Head Teacher sees the whole tree beneath them); a `Managed` grant is effectively
  `All` for someone at the top of the tree, which is intended. Requires a recursive walk
  (`usp_staff_member_is_managed_by`, cycle-guarded).
- **Minimum to open the page.** The viewer needs `ViewStaffBasicDetails` at a scope that covers
  the subject (Own/Managed/All). With no applicable access the header endpoint returns **403**
  and the sidebar is never reached.
- **`All` covers self; `Managed` does not.** `All` includes everyone, yourself included.
  `Managed` does **not** cover yourself (you are not your own report) — self always resolves
  through `Own`. So a user holding only `Managed` for an area cannot see that area on their *own*
  record; grant `Own` broadly for the everyday areas.
- **Sensitive fields live in their own area** (e.g. NI / ethnicity / religion / disability in
  `EqualityDetails`, salary / bank in `EmploymentDetails`) rather than being masked field-by-
  field inside an otherwise-visible area.

## Relationship resolution

```
StaffRelationship of (currentUser -> subjectStaffMember):
  currentPersonId := User(currentUserId).PersonId        // User.PersonId is nullable
  if currentPersonId is null            -> Unrelated      // user has no person identity (service account etc.)
  if subject is null                    -> Unrelated      // staff member doesn't exist
  if subject.PersonId == currentPersonId -> Self
  currentStaff := StaffMember where PersonId == currentPersonId
  if currentStaff != null and currentStaff is an ancestor of subject
        in the LineManagerId chain        -> LineManaged   // transitive: any depth above subject
  otherwise                               -> Unrelated
```

Backed by:
- `IAuthorizationService.GetCurrentUserPersonId()` — reads the `pid` claim added at sign-in.
- `IAuthorizationService.GetPermissionsAsync()` — viewer's effective permission set as a
  case-insensitive `IReadOnlySet<string>`, loaded once per check.
- `IStaffMemberRepository.GetStaffMemberIdByPersonIdAsync(personId)` — viewer's own staff id.
- `IStaffMemberRepository.IsManagedByAsync(subjectId, managerId)` — wraps the recursive CTE
  `usp_staff_member_is_managed_by`, cycle-guarded with `Depth < 100` + `OPTION (MAXRECURSION 100)`.

## Resolver contract

```csharp
public enum StaffArea
{
    BasicDetails,         // incl. contact methods, addresses, emergency contacts
    EqualityDetails,      // ethnicity, religion, disability, NI
    ProfessionalDetails,  // QTS, TRN, subjects, qualifications, CPD
    EmploymentDetails,    // tenures, contract, pay, bank
    PreEmploymentChecks,  // DBS, RTW
    Absences,             // sickness, leave (health data)
    Timetable,
    Documents,
    PerformanceDetails
}

[Flags]
public enum StaffAccess
{
    None        = 0,
    ViewOwn     = 1,
    ViewManaged = 1 << 1,
    ViewAll     = 1 << 2,
    EditOwn     = 1 << 3,
    EditManaged = 1 << 4,
    EditAll     = 1 << 5,
}

public enum StaffRelationship { Unrelated, LineManaged, Self }  // MyPortal.Contracts

public interface IStaffMemberAccessService
{
    Task<StaffRelationship> GetRelationshipAsync(Guid staffMemberId, CancellationToken ct);

    Task<bool> CanAsync(Guid staffMemberId, StaffArea area, StaffAccess acceptable,
        CancellationToken ct);

    // Throws ForbiddenException (-> 403). Throws InvalidOperationException if `acceptable`
    // is StaffAccess.None — that would silently always deny.
    Task RequireAsync(Guid staffMemberId, StaffArea area, StaffAccess acceptable,
        CancellationToken ct);
}
```

Decision algorithm:

```
CanAsync(subject, area, acceptable, ct):
    if acceptable == None: throw
    rel := GetRelationshipAsync(subject)
    perms := AuthorizationService.GetPermissionsAsync()

    if acceptable.HasFlag(ViewAll) and Held(area, ViewAll, perms): return true
    if acceptable.HasFlag(EditAll) and Held(area, EditAll, perms): return true

    if rel == LineManaged:
        if acceptable.HasFlag(ViewManaged) and Held(area, ViewManaged, perms): return true
        if acceptable.HasFlag(EditManaged) and Held(area, EditManaged, perms): return true

    if rel == Self:
        if acceptable.HasFlag(ViewOwn) and Held(area, ViewOwn, perms): return true
        if acceptable.HasFlag(EditOwn) and Held(area, EditOwn, perms): return true

    return false

Held(area, access, perms):
    return AccessPermission[(area, access)] in perms   // miss → false (combo not grantable)
```

The `(area, access) → permission-constant` lookup table in `StaffMemberAccessService` is the
single audit-able source of truth — 37 entries, each a direct reference to a `Permissions.Staff.*`
constant. No string derivation, no drift.

## API contract

### Header

`GET /api/staffmembers/{id}` → `StaffMemberHeaderResponse`

```csharp
public class StaffMemberHeaderResponse
{
    public Guid Id { get; set; }              // StaffMember id (the key for section endpoints)
    public Guid PersonId { get; set; }
    public string Code { get; set; }          // GFA-2041
    public string DisplayName { get; set; }   // "Title First [Middle] Last", legal name
    public string? PreferredName { get; set; }
    public Guid? PhotoId { get; set; }
    public StaffStatus Status { get; set; }
    public StaffRelationship Relationship { get; set; }   // Self / LineManaged / Unrelated
}
```

Gated on minimum-to-open:
```csharp
await _access.RequireAsync(id, StaffArea.BasicDetails,
    StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll, ct);
```
With no applicable scope → **403**. With access but no such staff member → **404** (only
reachable by All-scope holders, so existence isn't leaked to non-All viewers).

`Roles`, `LineManager`, `StartDate`, `Fte`, `Site`, and a `Leaver` `Status` state will land with
their respective area slices (employment / professional) — omitted here rather than included
with placeholder data.

### Section endpoints

`GET /api/staffmembers/{id}/{section}` and `PUT /api/staffmembers/{id}/{section}`. First line:

```csharp
// Employment view — only Own + All grant (no managed view of pay/bank).
await _access.RequireAsync(id, StaffArea.EmploymentDetails,
    StaffAccess.ViewOwn | StaffAccess.ViewAll, ct);

// Employment edit — All only.
await _access.RequireAsync(id, StaffArea.EmploymentDetails, StaffAccess.EditAll, ct);
```

403 (not 404) for a forbidden area: existence isn't secret — the header already revealed it.

### Sidebar (front end)

The FE has the viewer's permission claim (used for global nav gating) and `Relationship` from
the header. It composes section visibility from those two inputs — typically:

```ts
// pseudo
const canViewEmployment =
    perms.has("Staff.ViewAllStaffEmploymentDetails")
 || (relationship === "Self" && perms.has("Staff.ViewOwnStaffEmploymentDetails"));
```

The mapping from UI section to permission strings is owned by the FE. Getting it wrong means a
wrong sidebar, **never a security hole** — the server enforces at every endpoint.

## Permission areas — the 9-way collapse

The collapse from 13 UI-shaped sections to 9 permission areas reflects who *actually* needs each
data domain in a school. The FE may still render multiple sidebar panels per area.

| Area | UI panels it gates | Why grouped |
|---|---|---|
| **BasicDetails** | Basic details + Contact methods + Addresses + Emergency contacts | All "personal HR basics" — same audience. Bundling means HR doesn't manage four sets of grants. Address-level safety concerns (DV, etc.) belong on the data record (a "withhold from directory" flag), not the permission grid. |
| **EqualityDetails** | Equality & identity (ethnicity, religion, disability, NI no.) | UK GDPR Article 9 special-category data — strictest separation, smallest audience, separate audit trail. |
| **ProfessionalDetails** | Professional + Qualifications & CPD | Credentials / academic record. One audience. |
| **EmploymentDetails** | Employment & contract (tenures, post, FTE, pay scale, **salary**, **bank**) | Crown jewels. HR / payroll. No `Managed` scope — a line manager doesn't see their report's salary. |
| **PreEmploymentChecks** | DBS + Right to work + identity verification | Safeguarding lead / HR. Statutory single central record. Distinct audience from payroll. |
| **Absences** | Absences & leave | Health data (special category). Line managers need it for their reports (absence patterns / return-to-work); Finance needs it for payroll (SSP/OSP calculations). Separate gate keeps line managers out of salary while letting them do their job. |
| **Timetable** | Timetable & teaching load | Broadly visible operationally — colleagues need to see who's where. Editing is a central scheduling activity, hence its own area. |
| **Documents** | Documents | File access already had Own/Managed/All trio convention. Keep separate. |
| **PerformanceDetails** | Performance / appraisal | Most restricted — managers + HR only. No `Own` scope (you don't view your own appraisal record by default). |

## Permission catalogue

Naming: `Staff.{Verb}{Scope}Staff{Area}` — e.g. `Staff.ViewAllStaffEmploymentDetails`. Seeded by
`20260518000400_add_staff_profile_permissions.sql` (original) +
`20260518000500_collapse_staff_profile_permissions.sql` (removes the ContactMethods / Addresses /
EmergencyContacts / Qualifications areas that were superseded by the collapse). Total: 37
constants. None are auto-granted to a role — assignment is a separate admin step.

| Area | View scopes | Edit scopes |
|---|---|---|
| BasicDetails | Own, Managed, All | Managed, All |
| EqualityDetails | Own, All | All |
| ProfessionalDetails | Own, Managed, All | Managed, All |
| EmploymentDetails | Own, All | All |
| PreEmploymentChecks | All | All |
| Absences | Own, Managed, All | Managed, All |
| Timetable | Own, Managed, All | All |
| Documents | Own, Managed, All | Own, Managed, All |
| PerformanceDetails | Managed, All | Managed, All |

Empty cells = no permission string seeded → `(area, that-flag)` lookup misses → resolver
naturally denies. Expanding (e.g. adding `EditOwn` to ProfessionalDetails for self-logged CPD) is
one constant + one MERGE row + one lookup entry.

## Build order

1. ✅ Permission catalogue + seed migration.
2. ✅ `IAuthorizationService` additions (`GetCurrentUserPersonId`, `GetPermissionsAsync`) +
   `usp_staff_member_is_managed_by`.
3. ✅ `IStaffMemberAccessService` (StaffArea / StaffAccess flags / 37-entry lookup) + truth-table
   tests.
4. ✅ Header endpoint (`GetHeaderAsync` returning identity + `Relationship`).
5. Section endpoints, one slice at a time. Start with **basic details** (reuses
   `usp_staff_member_get_details_by_id` + `StaffMemberDetailsResponse`); then employment,
   absences, etc. Each declares its `(StaffArea, StaffAccess)` at the `RequireAsync` call.
