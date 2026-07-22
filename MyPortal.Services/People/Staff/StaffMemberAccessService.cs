using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Staff;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.People;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People.Staff;

/// <summary>
/// Resolves the viewer's relationship to a staff member and answers, for an (area, access)
/// pair, whether they're allowed. The (area × access) → permission-constant table below is the
/// single audit-able source of truth: every endpoint's gating ultimately resolves to one of
/// these constants, with no derivation / no drift. See docs/staff-profile-access.md.
/// </summary>
public class StaffMemberAccessService(
    IAuthorizationService authorizationService,
    ILogger<StaffMemberAccessService> logger,
    IStaffMemberRepository staffMemberRepository)
    : BaseService(authorizationService, logger), IStaffMemberAccessService
{
    // Per-request caches (the service is scoped). A header request typically resolves a guard
    // plus an explicit relationship read; without these, that's 2× the DB lookups.
    private (Guid SubjectId, StaffRelationship Value)? _cachedRelationship;
    private IReadOnlySet<string>? _cachedPermissions;

    // (Area, single access flag) → the seeded permission constant that grants it. Missing
    // combinations are not grantable — the lookup misses and the branch is skipped.
    private static readonly IReadOnlyDictionary<(StaffArea Area, StaffAccess Access), string>
        AccessPermission = new Dictionary<(StaffArea, StaffAccess), string>
        {
            // Basic details (incl. contact methods, addresses, emergency contacts)
            [(StaffArea.BasicDetails, StaffAccess.ViewOwn)]            = Permissions.Staff.ViewOwnStaffBasicDetails,
            [(StaffArea.BasicDetails, StaffAccess.ViewManaged)]        = Permissions.Staff.ViewManagedStaffBasicDetails,
            [(StaffArea.BasicDetails, StaffAccess.ViewAll)]            = Permissions.Staff.ViewAllStaffBasicDetails,
            [(StaffArea.BasicDetails, StaffAccess.EditManaged)]        = Permissions.Staff.EditManagedStaffBasicDetails,
            [(StaffArea.BasicDetails, StaffAccess.EditAll)]            = Permissions.Staff.EditAllStaffBasicDetails,

            // Equality & identity (special-category)
            [(StaffArea.EqualityDetails, StaffAccess.ViewOwn)]         = Permissions.Staff.ViewOwnStaffEqualityDetails,
            [(StaffArea.EqualityDetails, StaffAccess.ViewAll)]         = Permissions.Staff.ViewAllStaffEqualityDetails,
            [(StaffArea.EqualityDetails, StaffAccess.EditAll)]         = Permissions.Staff.EditAllStaffEqualityDetails,

            // Professional (incl. qualifications & CPD)
            [(StaffArea.ProfessionalDetails, StaffAccess.ViewOwn)]     = Permissions.Staff.ViewOwnStaffProfessionalDetails,
            [(StaffArea.ProfessionalDetails, StaffAccess.ViewManaged)] = Permissions.Staff.ViewManagedStaffProfessionalDetails,
            [(StaffArea.ProfessionalDetails, StaffAccess.ViewAll)]     = Permissions.Staff.ViewAllStaffProfessionalDetails,
            [(StaffArea.ProfessionalDetails, StaffAccess.EditManaged)] = Permissions.Staff.EditManagedStaffProfessionalDetails,
            [(StaffArea.ProfessionalDetails, StaffAccess.EditAll)]     = Permissions.Staff.EditAllStaffProfessionalDetails,

            // Employment & contract (incl. pay, bank)
            [(StaffArea.EmploymentDetails, StaffAccess.ViewOwn)]       = Permissions.Staff.ViewOwnStaffEmploymentDetails,
            [(StaffArea.EmploymentDetails, StaffAccess.ViewAll)]       = Permissions.Staff.ViewAllStaffEmploymentDetails,
            [(StaffArea.EmploymentDetails, StaffAccess.EditAll)]       = Permissions.Staff.EditAllStaffEmploymentDetails,

            // Pre-employment checks (DBS, RTW)
            [(StaffArea.PreEmploymentChecks, StaffAccess.ViewAll)]     = Permissions.Staff.ViewAllStaffPreEmploymentChecks,
            [(StaffArea.PreEmploymentChecks, StaffAccess.EditAll)]     = Permissions.Staff.EditAllStaffPreEmploymentChecks,

            // Absences (health data)
            [(StaffArea.Absences, StaffAccess.ViewOwn)]                = Permissions.Staff.ViewOwnStaffAbsences,
            [(StaffArea.Absences, StaffAccess.ViewManaged)]            = Permissions.Staff.ViewManagedStaffAbsences,
            [(StaffArea.Absences, StaffAccess.ViewAll)]                = Permissions.Staff.ViewAllStaffAbsences,
            [(StaffArea.Absences, StaffAccess.EditManaged)]            = Permissions.Staff.EditManagedStaffAbsences,
            [(StaffArea.Absences, StaffAccess.EditAll)]                = Permissions.Staff.EditAllStaffAbsences,

            [(StaffArea.Timetable, StaffAccess.ViewOwn)]               = Permissions.Staff.ViewOwnStaffTimetable,
            [(StaffArea.Timetable, StaffAccess.ViewManaged)]           = Permissions.Staff.ViewManagedStaffTimetable,
            [(StaffArea.Timetable, StaffAccess.ViewAll)]               = Permissions.Staff.ViewAllStaffTimetable,

            [(StaffArea.Documents, StaffAccess.ViewOwn)]               = Permissions.Staff.ViewOwnStaffDocuments,
            [(StaffArea.Documents, StaffAccess.ViewManaged)]           = Permissions.Staff.ViewManagedStaffDocuments,
            [(StaffArea.Documents, StaffAccess.ViewAll)]               = Permissions.Staff.ViewAllStaffDocuments,
            [(StaffArea.Documents, StaffAccess.EditOwn)]               = Permissions.Staff.EditOwnStaffDocuments,
            [(StaffArea.Documents, StaffAccess.EditManaged)]           = Permissions.Staff.EditManagedStaffDocuments,
            [(StaffArea.Documents, StaffAccess.EditAll)]               = Permissions.Staff.EditAllStaffDocuments,

            // Performance (no Own scope)
            [(StaffArea.PerformanceDetails, StaffAccess.ViewManaged)]  = Permissions.Staff.ViewManagedStaffPerformanceDetails,
            [(StaffArea.PerformanceDetails, StaffAccess.ViewAll)]      = Permissions.Staff.ViewAllStaffPerformanceDetails,
            [(StaffArea.PerformanceDetails, StaffAccess.EditManaged)]  = Permissions.Staff.EditManagedStaffPerformanceDetails,
            [(StaffArea.PerformanceDetails, StaffAccess.EditAll)]      = Permissions.Staff.EditAllStaffPerformanceDetails,
        };

    public async Task<StaffRelationship> GetRelationshipAsync(Guid staffMemberId, CancellationToken cancellationToken)
    {
        if (_cachedRelationship is { } cached && cached.SubjectId == staffMemberId)
        {
            return cached.Value;
        }

        var relationship = await ComputeRelationshipAsync(staffMemberId, cancellationToken);
        _cachedRelationship = (staffMemberId, relationship);
        return relationship;
    }

    private async Task<StaffRelationship> ComputeRelationshipAsync(Guid staffMemberId,
        CancellationToken cancellationToken)
    {
        var currentPersonId = AuthorizationService.GetCurrentUserPersonId();

        // No person identity (e.g. a service account) can only ever be Unrelated — All-scope only.
        if (currentPersonId is null)
        {
            return StaffRelationship.Unrelated;
        }

        var subject = await staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (subject is null)
        {
            return StaffRelationship.Unrelated;
        }

        if (subject.PersonId == currentPersonId.Value)
        {
            return StaffRelationship.Self;
        }

        var currentStaffMemberId =
            await staffMemberRepository.GetStaffMemberIdByPersonIdAsync(currentPersonId.Value, cancellationToken);

        // Viewer isn't (active) staff, so can't be anyone's line manager.
        if (currentStaffMemberId is null)
        {
            return StaffRelationship.Unrelated;
        }

        var managed = await staffMemberRepository.IsManagedByAsync(staffMemberId, currentStaffMemberId.Value,
            cancellationToken);

        return managed ? StaffRelationship.LineManaged : StaffRelationship.Unrelated;
    }

    private async Task<IReadOnlySet<string>> GetPermissionsAsync(CancellationToken cancellationToken)
    {
        return _cachedPermissions ??= await AuthorizationService.GetPermissionsAsync(cancellationToken);
    }

    public async Task<bool> CanAsync(Guid staffMemberId, StaffArea area, StaffAccess acceptable,
        CancellationToken cancellationToken)
    {
        if (acceptable == StaffAccess.None)
        {
            // Surface the bug — silently always denying would be invisible.
            throw new InvalidOperationException(
                "CanAsync requires at least one acceptable StaffAccess flag.");
        }

        var relationship = await GetRelationshipAsync(staffMemberId, cancellationToken);
        var permissions = await GetPermissionsAsync(cancellationToken);

        // All first (covers any relationship, incl. Self); then Managed (LineManaged only);
        // then Own (Self only). Short-circuit on first match.
        if (acceptable.HasFlag(StaffAccess.ViewAll) && Held(area, StaffAccess.ViewAll, permissions)) return true;
        if (acceptable.HasFlag(StaffAccess.EditAll) && Held(area, StaffAccess.EditAll, permissions)) return true;

        if (relationship == StaffRelationship.LineManaged)
        {
            if (acceptable.HasFlag(StaffAccess.ViewManaged) && Held(area, StaffAccess.ViewManaged, permissions)) return true;
            if (acceptable.HasFlag(StaffAccess.EditManaged) && Held(area, StaffAccess.EditManaged, permissions)) return true;
        }

        if (relationship == StaffRelationship.Self)
        {
            if (acceptable.HasFlag(StaffAccess.ViewOwn) && Held(area, StaffAccess.ViewOwn, permissions)) return true;
            if (acceptable.HasFlag(StaffAccess.EditOwn) && Held(area, StaffAccess.EditOwn, permissions)) return true;
        }

        return false;
    }

    public async Task RequireAsync(Guid staffMemberId, StaffArea area, StaffAccess acceptable,
        CancellationToken cancellationToken)
    {
        if (!await CanAsync(staffMemberId, area, acceptable, cancellationToken))
        {
            throw new ForbiddenException("You do not have permission to access this staff information.");
        }
    }

    private static bool Held(StaffArea area, StaffAccess access, IReadOnlySet<string> permissions)
        => AccessPermission.TryGetValue((area, access), out var name) && permissions.Contains(name);
}
