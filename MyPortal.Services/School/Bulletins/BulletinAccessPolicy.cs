using MyPortal.Core.Entities;
using MyPortal.Data.VisibilityScopes;
using MyPortal.Services.Interfaces.Security;

namespace MyPortal.Services.School.Bulletins;

/// <summary>
/// Application-layer view/edit gate for bulletins. Audience-membership filtering for
/// non-staff readers is enforced by the bulletin SP / summaries SQL (which can resolve
/// pupil/parent → student group via the relationships tables in a single query). This
/// policy only enforces the role-level rules that don't need a DB lookup — so it stays
/// callable from the service without an extra round-trip.
/// </summary>
public class BulletinAccessPolicy : IAccessPolicy<Bulletin, BulletinVisibilityScope>
{
    public bool CanView(Bulletin bulletin, BulletinVisibilityScope scope)
    {
        // Staff with the admin (pin) permission can always see every bulletin.
        if (scope.IsStaff && scope.CanPin)
        {
            return true;
        }

        // Staff who created the bulletin always see their own, regardless of expiry.
        if (scope.IsStaff && scope.CanEdit && bulletin.CreatedById == scope.CurrentUserId)
        {
            return true;
        }

        // For everyone else, audience membership is the gate. The application layer
        // can't resolve audience here without another query, so we authorise in
        // principle (the SP-side filter is the real check). Returning false would
        // produce false negatives for legitimately-targeted readers.
        return true;
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
