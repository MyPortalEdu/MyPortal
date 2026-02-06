using MyPortal.Core.Entities;

namespace MyPortal.Services.School.Bulletins;

public static class BulletinAccessPolicy
{
    public static bool IsNotExpired(DateTime? expiresAt, DateTime nowUtc) => expiresAt is null || expiresAt > nowUtc;

    public static bool CanView(Bulletin bulletin, BulletinVisibilityScope scope, DateTime nowUtc)
    {
        if (bulletin.IsPrivate && !scope.IsStaff)
        {
            return false;
        }

        if (scope.CanApprove)
        {
            return true;
        }

        var approvedAndNotExpired = bulletin.IsApproved && IsNotExpired(bulletin.ExpiresAt, nowUtc);

        if (!scope.IsStaff)
        {
            return approvedAndNotExpired;
        }

        if (!(scope.CanView || scope.CanEdit))
        {
            return false;
        }

        if (scope.CanEdit && bulletin.CreatedById == scope.CurrentUserId)
        {
            return true;
        }

        return approvedAndNotExpired;
    }

    public static bool CanEdit(Bulletin bulletin, BulletinVisibilityScope scope)
    {
        if (scope.CanApprove)
        {
            return true;
        }

        if (!scope.IsStaff)
        {
            return false;
        }
        
        return scope.CanEdit && bulletin.CreatedById == scope.CurrentUserId;
    }
}