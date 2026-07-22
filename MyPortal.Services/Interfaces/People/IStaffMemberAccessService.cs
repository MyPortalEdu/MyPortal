using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Staff;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// Permission domain over staff data. Server-internal grouping, NOT a UI section — a single
/// area can back several sidebar panels in the FE.
/// </summary>
public enum StaffArea
{
    BasicDetails,
    EqualityDetails,
    ProfessionalDetails,
    EmploymentDetails,
    PreEmploymentChecks,
    Absences,
    Timetable,
    Documents,
    PerformanceDetails
}

/// <summary>
/// Access types an endpoint will accept, composed via OR. Scope dictates the viewer→subject
/// relationship needed: Own⇢Self, Managed⇢LineManaged (transitive), All⇢any.
/// </summary>
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

/// <summary>
/// Single authority for staff-data gating. The endpoint declares (area, acceptable access);
/// the resolver answers whether the viewer holds a matching permission AND their relationship
/// satisfies its scope. See docs/staff-profile-access.md.
/// </summary>
public interface IStaffMemberAccessService
{
    Task<StaffRelationship> GetRelationshipAsync(Guid staffMemberId, CancellationToken cancellationToken);

    /// <summary>Throws <see cref="InvalidOperationException"/> if <paramref name="acceptable"/>
    /// is <see cref="StaffAccess.None"/> (would silently deny).</summary>
    Task<bool> CanAsync(Guid staffMemberId, StaffArea area, StaffAccess acceptable,
        CancellationToken cancellationToken);

    /// <summary>Throwing guard for endpoints (<c>ForbiddenException</c> ⇢ 403).</summary>
    Task RequireAsync(Guid staffMemberId, StaffArea area, StaffAccess acceptable,
        CancellationToken cancellationToken);
}
