using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.Providers;
using MyPortal.Services.Interfaces.Security;

namespace MyPortal.Services.School.Bulletins;

public class BulletinAccessPolicy : IAccessPolicy<Bulletin, BulletinVisibilityScope>
{
    private readonly IDateTimeProvider _dateTimeProvider;
    
    public BulletinAccessPolicy(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public bool IsNotExpired(DateTime? expiresAt, DateTime nowUtc) => expiresAt is null || expiresAt > nowUtc;

    public bool CanView(Bulletin bulletin, BulletinVisibilityScope scope)
    {
        if (bulletin.IsPrivate && !scope.IsStaff)
        {
            return false;
        }

        if (scope.CanApprove)
        {
            return true;
        }

        var approvedAndNotExpired = bulletin.IsApproved && IsNotExpired(bulletin.ExpiresAt, _dateTimeProvider.UtcNow);

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

    public bool CanEdit(Bulletin bulletin, BulletinVisibilityScope scope)
    {
        if (!scope.IsStaff)
        {
            return false;
        }
        
        if (scope.CanApprove)
        {
            return true;
        }
        
        return scope.CanEdit && bulletin.CreatedById == scope.CurrentUserId;
    }
}