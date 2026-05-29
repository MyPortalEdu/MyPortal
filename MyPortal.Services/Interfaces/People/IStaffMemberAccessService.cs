using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

/// <summary>What the current viewer may do with a section of a staff record.</summary>
public enum StaffSectionVerb
{
    View,
    Edit
}

/// <summary>Relationship of the current viewer to the subject staff member.</summary>
public enum StaffRelationship
{
    /// <summary>No self or line-management link (or the viewer has no person identity).</summary>
    Unrelated,

    /// <summary>The viewer is somewhere above the subject in the line-management chain (transitive).</summary>
    LineManaged,

    /// <summary>The subject is the viewer's own record.</summary>
    Self
}

/// <summary>
/// The single authority for "can this viewer see/edit this section of this staff member?".
/// Both the capability map (what the sidebar shows) and every section endpoint guard go through
/// this, so they can never disagree. Resolves the viewer→subject relationship and tests it
/// against the scoped permissions the viewer holds. See docs/staff-profile-access.md.
/// </summary>
public interface IStaffMemberAccessService
{
    /// <summary>Relationship of the current viewer to the given staff member.</summary>
    Task<StaffRelationship> GetRelationshipAsync(Guid staffMemberId, CancellationToken cancellationToken);

    /// <summary>Whether the current viewer may perform <paramref name="verb"/> on
    /// <paramref name="section"/> of the given staff member.</summary>
    Task<bool> CanAsync(Guid staffMemberId, StaffProfileSection section, StaffSectionVerb verb,
        CancellationToken cancellationToken);

    /// <summary>Throwing guard for section endpoints. Throws <c>ForbiddenException</c> when denied.</summary>
    Task RequireAsync(Guid staffMemberId, StaffProfileSection section, StaffSectionVerb verb,
        CancellationToken cancellationToken);

    /// <summary>Resolves view/edit access for every section at once — for the header load.</summary>
    Task<IReadOnlyDictionary<StaffProfileSection, SectionAccess>> GetCapabilitiesAsync(Guid staffMemberId,
        CancellationToken cancellationToken);
}
