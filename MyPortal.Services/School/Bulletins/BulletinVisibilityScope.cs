using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;

namespace MyPortal.Services.School.Bulletins;

public record BulletinVisibilityScope(
    Guid? CurrentUserId,
    UserType CurrentUserType,
    bool CanView,
    bool CanEdit,
    bool CanApprove)
{
    public bool IsStaff =>  CurrentUserType == UserType.Staff;

    public static async Task<BulletinVisibilityScope> FromAsync(IAuthorizationService authorizationService,
        CancellationToken cancellationToken)
    {
        var canView =
            await authorizationService.HasPermissionAsync(Permissions.School.ViewSchoolBulletins, cancellationToken);
        var canEdit =
            await authorizationService.HasPermissionAsync(Permissions.School.EditSchoolBulletins, cancellationToken);
        var canApprove =
            await authorizationService.HasPermissionAsync(Permissions.School.ApproveSchoolBulletins, cancellationToken);
        
        return new BulletinVisibilityScope(authorizationService.GetCurrentUserId(), authorizationService.GetCurrentUserType(), canView, canEdit, canApprove);
    }

    public object ToSqlParams(DateTime nowUtc) => new
    {
        currentUserId = CurrentUserId,
        isStaff = IsStaff,
        canView = CanView,
        canEdit = CanEdit,
        canApprove = CanApprove,
        nowUtc
    };
}