using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.People;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.People;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

/// <summary>
/// Resolves staff-profile section access for the current viewer. Single source of truth for both
/// the capability map and the per-endpoint guards. See docs/staff-profile-access.md.
/// </summary>
public class StaffMemberAccessService : BaseService, IStaffMemberAccessService
{
    private readonly IStaffMemberRepository _staffMemberRepository;

    public StaffMemberAccessService(IAuthorizationService authorizationService,
        ILogger<StaffMemberAccessService> logger, IStaffMemberRepository staffMemberRepository)
        : base(authorizationService, logger)
    {
        _staffMemberRepository = staffMemberRepository;
    }

    // Permission scopes, encoded into the permission name (Staff.{Verb}{Scope}Staff{Section}).
    private enum Scope
    {
        Own,
        Managed,
        All
    }

    private sealed record SectionScopes(Scope[] View, Scope[] Edit);

    // The section -> grantable-scopes matrix. A scope absent here is not grantable for that
    // section/verb, so the resolver can never grant it even if a (non-existent) permission were
    // held. Mirrors the table in docs/staff-profile-access.md and the seeded catalogue exactly
    // (asserted by StaffMemberAccessServiceTests against Permissions.Staff).
    private static readonly IReadOnlyDictionary<StaffProfileSection, SectionScopes> Matrix =
        new Dictionary<StaffProfileSection, SectionScopes>
        {
            [StaffProfileSection.BasicDetails]         = new([Scope.Own, Scope.Managed, Scope.All], [Scope.Managed, Scope.All]),
            [StaffProfileSection.EqualityAndIdentity]  = new([Scope.Own, Scope.All],                [Scope.All]),
            [StaffProfileSection.ContactMethods]       = new([Scope.Own, Scope.Managed, Scope.All], [Scope.Own, Scope.Managed, Scope.All]),
            [StaffProfileSection.Addresses]            = new([Scope.Own, Scope.Managed, Scope.All], [Scope.Own, Scope.Managed, Scope.All]),
            [StaffProfileSection.EmergencyContacts]    = new([Scope.Own, Scope.Managed, Scope.All], [Scope.Own, Scope.Managed, Scope.All]),
            [StaffProfileSection.Professional]         = new([Scope.Own, Scope.Managed, Scope.All], [Scope.Managed, Scope.All]),
            [StaffProfileSection.QualificationsAndCpd] = new([Scope.Own, Scope.Managed, Scope.All], [Scope.Own, Scope.Managed, Scope.All]),
            [StaffProfileSection.Employment]           = new([Scope.Own, Scope.All],                [Scope.All]),
            [StaffProfileSection.PreEmploymentChecks]  = new([Scope.All],                           [Scope.All]),
            [StaffProfileSection.AbsencesAndLeave]     = new([Scope.Own, Scope.Managed, Scope.All], [Scope.Managed, Scope.All]),
            [StaffProfileSection.Timetable]            = new([Scope.Own, Scope.Managed, Scope.All], [Scope.All]),
            [StaffProfileSection.Documents]            = new([Scope.Own, Scope.Managed, Scope.All], [Scope.Own, Scope.Managed, Scope.All]),
            [StaffProfileSection.Performance]          = new([Scope.Managed, Scope.All],            [Scope.Managed, Scope.All]),
        };

    // Section -> the fragment used in the permission name. Must match the seeded catalogue.
    private static readonly IReadOnlyDictionary<StaffProfileSection, string> NameFragment =
        new Dictionary<StaffProfileSection, string>
        {
            [StaffProfileSection.BasicDetails]         = "BasicDetails",
            [StaffProfileSection.EqualityAndIdentity]  = "EqualityDetails",
            [StaffProfileSection.ContactMethods]       = "ContactMethods",
            [StaffProfileSection.Addresses]            = "Addresses",
            [StaffProfileSection.EmergencyContacts]    = "EmergencyContacts",
            [StaffProfileSection.Professional]         = "ProfessionalDetails",
            [StaffProfileSection.QualificationsAndCpd] = "Qualifications",
            [StaffProfileSection.Employment]           = "EmploymentDetails",
            [StaffProfileSection.PreEmploymentChecks]  = "PreEmploymentChecks",
            [StaffProfileSection.AbsencesAndLeave]     = "Absences",
            [StaffProfileSection.Timetable]            = "Timetable",
            [StaffProfileSection.Documents]            = "Documents",
            [StaffProfileSection.Performance]          = "PerformanceDetails",
        };

    private static string PermissionName(StaffSectionVerb verb, Scope scope, StaffProfileSection section)
        => $"Staff.{verb}{scope}Staff{NameFragment[section]}";

    /// <summary>
    /// Every permission string the matrix can require. Lets tests assert this exactly matches the
    /// seeded <c>Permissions.Staff</c> catalogue, closing the gap where a derived name could drift
    /// from a declared constant (a mismatch would silently deny access).
    /// </summary>
    public static IReadOnlySet<string> AllPermissions { get; } = BuildAllPermissions();

    private static IReadOnlySet<string> BuildAllPermissions()
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (section, scopes) in Matrix)
        {
            foreach (var scope in scopes.View)
            {
                set.Add(PermissionName(StaffSectionVerb.View, scope, section));
            }

            foreach (var scope in scopes.Edit)
            {
                set.Add(PermissionName(StaffSectionVerb.Edit, scope, section));
            }
        }

        return set;
    }

    public async Task<StaffRelationship> GetRelationshipAsync(Guid staffMemberId, CancellationToken cancellationToken)
    {
        var currentPersonId = AuthorizationService.GetCurrentUserPersonId();

        // No person identity (e.g. a service account) can only ever be Unrelated — All-scope only.
        if (currentPersonId is null)
        {
            return StaffRelationship.Unrelated;
        }

        var subject = await _staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (subject is null)
        {
            return StaffRelationship.Unrelated;
        }

        if (subject.PersonId == currentPersonId.Value)
        {
            return StaffRelationship.Self;
        }

        var currentStaffMemberId =
            await _staffMemberRepository.GetStaffMemberIdByPersonIdAsync(currentPersonId.Value, cancellationToken);

        // Viewer isn't (active) staff, so can't be anyone's line manager.
        if (currentStaffMemberId is null)
        {
            return StaffRelationship.Unrelated;
        }

        var managed = await _staffMemberRepository.IsManagedByAsync(staffMemberId, currentStaffMemberId.Value,
            cancellationToken);

        return managed ? StaffRelationship.LineManaged : StaffRelationship.Unrelated;
    }

    public async Task<bool> CanAsync(Guid staffMemberId, StaffProfileSection section, StaffSectionVerb verb,
        CancellationToken cancellationToken)
    {
        var relationship = await GetRelationshipAsync(staffMemberId, cancellationToken);
        var permissions = await AuthorizationService.GetPermissionsAsync(cancellationToken);

        return Evaluate(section, verb, relationship, permissions);
    }

    public async Task RequireAsync(Guid staffMemberId, StaffProfileSection section, StaffSectionVerb verb,
        CancellationToken cancellationToken)
    {
        if (!await CanAsync(staffMemberId, section, verb, cancellationToken))
        {
            throw new ForbiddenException("You do not have permission to access this staff information.");
        }
    }

    public async Task<IReadOnlyDictionary<StaffProfileSection, SectionAccess>> GetCapabilitiesAsync(
        Guid staffMemberId, CancellationToken cancellationToken)
    {
        // Resolve the relationship and permission set once, then evaluate every section in memory.
        var relationship = await GetRelationshipAsync(staffMemberId, cancellationToken);
        var permissions = await AuthorizationService.GetPermissionsAsync(cancellationToken);

        var map = new Dictionary<StaffProfileSection, SectionAccess>();

        foreach (var section in Enum.GetValues<StaffProfileSection>())
        {
            map[section] = new SectionAccess
            {
                CanView = Evaluate(section, StaffSectionVerb.View, relationship, permissions),
                CanEdit = Evaluate(section, StaffSectionVerb.Edit, relationship, permissions)
            };
        }

        return map;
    }

    // The single decision. All-scope covers any relationship (incl. self); Managed covers only
    // LineManaged (a viewer is never their own report); Own covers only Self.
    private static bool Evaluate(StaffProfileSection section, StaffSectionVerb verb, StaffRelationship relationship,
        IReadOnlySet<string> permissions)
    {
        var scopes = verb == StaffSectionVerb.View ? Matrix[section].View : Matrix[section].Edit;

        if (scopes.Contains(Scope.All) && permissions.Contains(PermissionName(verb, Scope.All, section)))
        {
            return true;
        }

        if (relationship == StaffRelationship.LineManaged && scopes.Contains(Scope.Managed) &&
            permissions.Contains(PermissionName(verb, Scope.Managed, section)))
        {
            return true;
        }

        if (relationship == StaffRelationship.Self && scopes.Contains(Scope.Own) &&
            permissions.Contains(PermissionName(verb, Scope.Own, section)))
        {
            return true;
        }

        return false;
    }
}
