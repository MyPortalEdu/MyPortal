using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.VisibilityScopes;
using MyPortal.Services.Interfaces.Security;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.School.Bulletins;

/// <summary>
/// Application-layer view/edit gate for bulletins. The audience-membership branch
/// delegates to <see cref="IBulletinRepository.IsVisibleToUserAsync"/> so the policy
/// stays honest at any call site — including paths that don't go through the
/// summaries/details SPs. In-memory short-circuits handle the cases the SP would
/// resolve the same way (staff pinners, staff creators) without a DB round-trip.
/// </summary>
public class BulletinAccessPolicy(IBulletinRepository bulletinRepository)
    : IAccessPolicy<Bulletin, BulletinVisibilityScope>
{
    public Task<bool> CanViewAsync(Bulletin bulletin, BulletinVisibilityScope scope,
        CancellationToken cancellationToken)
    {
        // Staff with the admin (pin) permission can always see every bulletin.
        if (scope.IsStaff && scope.CanPin)
        {
            return Task.FromResult(true);
        }

        // Staff who created the bulletin always see their own, regardless of expiry.
        if (scope.IsStaff && scope.CanEdit && bulletin.CreatedById == scope.CurrentUserId)
        {
            return Task.FromResult(true);
        }

        return bulletinRepository.IsVisibleToUserAsync(bulletin.Id, scope, cancellationToken);
    }

    public bool CanEdit(Bulletin bulletin, BulletinVisibilityScope scope)
    {
        if (!scope.IsStaff)
        {
            return false;
        }

        // Pinners (admins) can edit anything.
        if (scope.CanPin)
        {
            return true;
        }

        // Otherwise only the creator with edit permission can edit.
        return scope.CanEdit && bulletin.CreatedById == scope.CurrentUserId;
    }
}
