 # Staff profile access — resolver & capability-map spec

Status: **draft for review.** This is the keystone for the staff details page. Endpoints
should not be written until the resolver contract and permission catalogue below are agreed,
because both the sidebar (what you see) and every section endpoint (what you can fetch) hang
off the same authorization decision and *must not be allowed to drift*.

## Decisions already taken

- **Section-level gating.** The unit of access is a profile *section*, not a field. Sensitive
  fields are relocated into their own sections (see [Sections](#sections)) rather than masked
  individually, so there is exactly one gating mechanism.
- **Relationship-aware.** Visibility depends on the `(viewer, subject)` pair, not just the
  viewer's global permissions: a staff member sees their own record, a line manager sees more
  for their reports, HR/SLT see everyone.
- **Per-section endpoints, lazy.** The page load returns the header + a capability map; each
  section is its own gated endpoint fetched on demand. No section data is queried or serialized
  for a viewer who cannot see it (GDPR data minimisation).
- **Single resolver.** One `IStaffMemberAccessService` answers every access question. The
  capability-map builder and each endpoint guard both call it, so they can never disagree.
- **Scope encoded in the permission name** (`View{Scope}Staff{Section}`), matching the existing
  `ViewAllStaffDocuments` / `ViewManagedStaffDocuments` / `ViewOwnStaffDocuments` convention.
  `IAuthorizationService` stays a pure `HasPermission(string)` check; scope is just more strings.
- **`Managed` is transitive.** The viewer manages the subject if their StaffMember appears
  *anywhere* above the subject in the `LineManagerId` chain — direct reports, reports of reports,
  any depth. Matches how schools delegate (a Head Teacher sees the whole tree beneath them).
  Consequence: a `Managed` grant is effectively `All` for someone at the top of the tree, which
  is intended. Requires a recursive walk (`usp_staff_member_is_managed_by`, cycle-guarded).
- **Minimum to open the page.** The viewer needs `ViewStaffBasicDetails` at a scope that covers
  the subject (Own/Managed/All). With no applicable access the header endpoint returns **403** and
  the sidebar is never reached.
- **`All` covers self; `Managed` does not.** `All` includes everyone, yourself included.
  `Managed` does **not** cover yourself (you are not your own report) — self always resolves
  through `Own`. So a user holding only `Managed` for a section cannot see that section on their
  *own* record; grant `Own` broadly for the everyday sections.

## Relationship resolution

```
StaffRelationship of (currentUser -> subjectStaffMember):
  currentPersonId := User(currentUserId).PersonId        // User.PersonId is nullable
  if currentPersonId is null            -> Unrelated      // user has no person identity (service account etc.)
  if subject.PersonId == currentPersonId -> Self
  currentStaff := StaffMember where PersonId == currentPersonId
  if currentStaff != null and currentStaff is an ancestor of subject
        in the LineManagerId chain        -> LineManaged   // transitive: any depth above subject
  otherwise                               -> Unrelated
```

Dependencies this exposes:
- `IAuthorizationService` currently has no "current user's person/staff" accessor. Add
  `Guid? GetCurrentUserPersonId()` (or resolve via the user store) — the resolver needs it.
- Transitive management needs a tree query. Proposed:
  `usp_staff_member_is_managed_by(@subjectStaffMemberId, @managerStaffMemberId) -> bit`, a
  recursive CTE up `StaffMembers.LineManagerId`. Guard against cycles (`MAXRECURSION`, or a
  visited check) — bad data could otherwise loop.

## Sections

`StaffProfileSection` (serialized camelCase in JSON):

| Section | Contains | View scopes | Edit scopes |
|---|---|---|---|
| `basicDetails` | name, photo, DOB, gender, pronouns, marital status, nationality | Own, Managed, All | Own*, Managed, All |
| `equalityAndIdentity` | ethnicity, religion, disability, NI number | Own (view), All | All |
| `contactMethods` | phone, email | Own, Managed, All | Own, Managed, All |
| `addresses` | home/postal addresses | Own, Managed, All | Own, Managed, All |
| `emergencyContacts` | next of kin | Own, Managed, All | Own, Managed, All |
| `professional` | QTS, TRN, teaching status, subjects | Own, Managed, All | Managed, All |
| `qualificationsAndCpd` | qualifications, CPD | Own, Managed, All | Own, Managed, All |
| `employment` | tenures, contract, post, FTE, pay scale, **salary**, **bank** | Own (view), All | All |
| `preEmploymentChecks` | DBS, right-to-work, identity | All | All |
| `absencesAndLeave` | sickness, leave (**health data**) | Own, Managed, All | Managed, All |
| `timetable` | teaching load / timetable | Own, Managed, All | All |
| `documents` | staff documents | Own, Managed, All | Own, Managed, All |
| `performance` | appraisal (not in current mockup) | Managed, All | Managed, All |

\* `basicDetails` edit-Own is debatable — letting staff change their own legal name/DOB usually
routes through HR. Start without `EditOwnStaffBasicDetails` and add if self-service is wanted.

Empty cells = that scope is not grantable for that section (no permission string exists, so the
resolver can never grant it). The matrix above is the starting point to prune in review — the
crown jewels (`employment`, `preEmploymentChecks`, `equalityAndIdentity`) are deliberately
`All`-only for write and near-`All`-only for read.

## Permissions

Naming: `Staff.{Verb}{Scope}Staff{Section}` — e.g. `Staff.ViewAllStaffEmploymentDetails`,
`Staff.ViewManagedStaffEmploymentDetails`, `Staff.ViewOwnStaffEmploymentDetails`,
`Staff.EditAllStaffEmploymentDetails`. Generated from the matrix (verb × scope × section);
only cells present in the matrix get a string. Seed via a MERGE migration like
`20260514000000_add_agency_permissions.sql`. None are auto-granted to a role — role assignment
is a separate admin step.

> **Supersedes the current placeholder.** `Staff.ViewStaffBasicDetails` /
> `Staff.EditStaffBasicDetails` (migration `20260518000400`) are unscoped and pre-date this
> model. They get replaced by the scoped `…OwnStaffBasicDetails` / `…ManagedStaffBasicDetails` /
> `…AllStaffBasicDetails` set, and the staff picker + `StaffMembersController` + `StaffMemberService`
> gates move to the scoped names. Nothing has shipped, so this is a rename, not a data migration.

## Resolver contract

```csharp
public enum StaffProfileSection { BasicDetails, EqualityAndIdentity, ContactMethods, Addresses,
    EmergencyContacts, Professional, QualificationsAndCpd, Employment, PreEmploymentChecks,
    AbsencesAndLeave, Timetable, Documents, Performance }

public enum StaffSectionVerb { View, Edit }

public enum StaffRelationship { Unrelated, LineManaged, Self }

public enum StaffAccessScope { Own, Managed, All }   // permission scopes

public sealed class SectionAccess
{
    public bool CanView { get; init; }
    public bool CanEdit { get; init; }
}

public interface IStaffMemberAccessService
{
    Task<StaffRelationship> GetRelationshipAsync(Guid staffMemberId, CancellationToken ct);

    // Single source of truth. Every guard and the capability map go through this.
    Task<bool> CanAsync(Guid staffMemberId, StaffProfileSection section, StaffSectionVerb verb,
        CancellationToken ct);

    // Throwing guard for section endpoints (ForbiddenException -> 403).
    Task RequireAsync(Guid staffMemberId, StaffProfileSection section, StaffSectionVerb verb,
        CancellationToken ct);

    // Whole-page resolution for the header load: every section at once.
    Task<IReadOnlyDictionary<StaffProfileSection, SectionAccess>> GetCapabilitiesAsync(
        Guid staffMemberId, CancellationToken ct);
}
```

### Algorithm

```
CanAsync(subject, section, verb):
    scopes := ScopeMatrix[section][verb]          // subset of {Own, Managed, All}; empty => false
    if scopes is empty: return false
    rel := GetRelationshipAsync(subject)
    if All     in scopes and HasPerm(name(verb, All,     section)): return true
    if Managed in scopes and rel == LineManaged and HasPerm(name(verb, Managed, section)): return true
    if Own     in scopes and rel == Self        and HasPerm(name(verb, Own,     section)): return true
    return false
```

`GetCapabilitiesAsync` resolves the relationship **once**, then evaluates every section/verb cell
against the viewer's held permissions in memory.

> **Perf note.** Evaluating the map is ~`sections × scopes × verbs` permission checks. If
> `HasPermissionAsync` hits the DB per call that is too many round-trips. Add
> `Task<ISet<string>> GetPermissionsAsync(CancellationToken)` to `IAuthorizationService` (the
> permission set is already claim-backed for the token), load it once, and have the resolver
> test membership in memory.

## API contract

### Header + capability map

`GET /api/staffmembers/{id}` → `StaffMemberHeaderResponse`

```csharp
public class StaffMemberHeaderResponse
{
    public Guid Id { get; set; }              // StaffMember id (the key for section endpoints)
    public Guid PersonId { get; set; }
    public string Code { get; set; }          // GFA-2041
    public string DisplayName { get; set; }   // "Mr Daniel James Roberts"
    public string? PreferredName { get; set; }
    public Guid? PhotoId { get; set; }
    public StaffStatus Status { get; set; }   // Active / Inactive / Leaver
    public string[] Roles { get; set; }       // ["SLT", "Head of Mathematics", "DSL deputy"]
    public DateTime? StartDate { get; set; }
    public decimal Fte { get; set; }
    public string? Site { get; set; }

    // Resolved for (viewer, this subject). The FE renders the sidebar from this and
    // never hardcodes section visibility.
    public IReadOnlyDictionary<StaffProfileSection, SectionAccess> Sections { get; set; }
}
```

JSON shape:

```json
{
  "id": "…", "code": "GFA-2041", "displayName": "Mr Daniel James Roberts",
  "status": "active", "roles": ["SLT", "Head of Mathematics"],
  "sections": {
    "basicDetails":   { "canView": true,  "canEdit": true  },
    "employment":     { "canView": true,  "canEdit": false },
    "absencesAndLeave": { "canView": false, "canEdit": false }
  }
}
```

The header itself is gated: 403 if the viewer has no applicable access to the subject
(see "minimum to open the page" above). Sections the viewer cannot see are still keyed in the map with
`canView: false` so the FE can choose to grey-out vs omit — or omit them server-side; pick one
and be consistent.

### Section endpoints

`GET /api/staffmembers/{id}/{section}` and `PUT /api/staffmembers/{id}/{section}`, one per section
(`/basic-details`, `/employment`, `/absences`, …). Each, first line:

```csharp
await _access.RequireAsync(id, StaffProfileSection.Employment, StaffSectionVerb.View, ct);
```

`RequireAsync` throws `ForbiddenException` → 403. Existence of the staff member is not secret
(the header already revealed it), so 403 (not 404) for a forbidden section is fine.

## Consequences for what already exists

- **Decompose the details endpoint.** `usp_staff_member_get_details_by_id` + the two-result-set
  `StaffMemberDetailsResponse` become the *Basic details section* source, not "the details
  endpoint." The "append more result sets later" plan is dropped in favour of per-section
  endpoints. `GetDetailsAsync` is replaced by `GetHeaderAsync` (header + capabilities) + the
  section gets.
- **Rename the placeholder permissions** as noted above and repoint the picker + controller +
  service gates to the scoped names.
- **New auth primitives:** `GetCurrentUserPersonId()` and `GetPermissionsAsync()` on
  `IAuthorizationService`; the `is-managed-by` CTE.

## Suggested build order

1. Permission catalogue + seed migration (rename placeholders → scoped set).
2. `IAuthorizationService` additions (`GetCurrentUserPersonId`, `GetPermissionsAsync`) + the
   `is-managed-by` query.
3. `IStaffMemberAccessService` + the scope matrix + unit tests for the algorithm (the truth table
   of relationship × held-scope → allowed is exactly what to test hardest).
4. Header endpoint (`GetHeaderAsync` + capability map).
5. Section endpoints one at a time, each calling `RequireAsync`, starting with `basicDetails`
   (reusing the existing proc) then `employment`.
