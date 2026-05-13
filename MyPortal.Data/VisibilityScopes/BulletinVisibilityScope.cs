using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;

namespace MyPortal.Data.VisibilityScopes;

public record BulletinVisibilityScope(
    Guid? CurrentUserId,
    UserType CurrentUserType,
    bool CanView,
    bool CanEdit,
    bool CanPin)
{
    public bool IsStaff  => CurrentUserType == UserType.Staff;
    public bool IsPupil  => CurrentUserType == UserType.Student;
    public bool IsParent => CurrentUserType == UserType.Parent;

    public static async Task<BulletinVisibilityScope> FromAsync(IAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var canView =
            await authorizationService.HasPermissionAsync(Permissions.School.ViewSchoolBulletins, cancellationToken);
        var canEdit =
            await authorizationService.HasPermissionAsync(Permissions.School.EditSchoolBulletins, cancellationToken);
        var canPin =
            await authorizationService.HasPermissionAsync(Permissions.School.PinSchoolBulletins, cancellationToken);

        return new BulletinVisibilityScope(
            authorizationService.GetCurrentUserId(),
            authorizationService.GetCurrentUserType(),
            canView, canEdit, canPin);
    }

    /// <summary>
    /// Flat parameter object passed to bulletin SPs / SQL templates. Audience-based
    /// filtering needs to know who the caller is and what role they're in; permission
    /// flags decide whether staff bypass the audience filter.
    /// </summary>
    public object ToSqlParams() => new
    {
        currentUserId = CurrentUserId,
        isStaff       = IsStaff,
        isPupil       = IsPupil,
        isParent      = IsParent,
        canView       = CanView,
        canEdit       = CanEdit,
        canPin        = CanPin
    };
}
