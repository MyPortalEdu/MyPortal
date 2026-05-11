using MyPortal.Common.Enums;
using MyPortal.Core.Entities;
using MyPortal.Data.VisibilityScopes;
using MyPortal.Services.School.Bulletins;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class BulletinAccessPolicyTests
{
    private static readonly Guid AuthorId    = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private BulletinAccessPolicy _policy = null!;

    [SetUp]
    public void Setup()
    {
        _policy = new BulletinAccessPolicy();
    }

    // ─── CanView ─────────────────────────────────────────────────────────────
    // Audience-membership filtering for non-staff is handled by the SP/summaries
    // SQL (which can resolve pupil/parent → student group in a single query).
    // The application-layer policy only enforces what doesn't need a DB lookup,
    // so non-staff calls return true and defer to SQL.

    [TestCase(UserType.Student)]
    [TestCase(UserType.Parent)]
    public void CanView_NonStaff_ReturnsTrue_DefersToSqlAudienceFilter(UserType userType)
    {
        var bulletin = MakeBulletin(createdById: AuthorId);
        var scope = MakeScope(userType, OtherUserId, canView: false, canEdit: false, canPin: false);
        Assert.That(_policy.CanView(bulletin, scope), Is.True);
    }

    [Test]
    public void CanView_StaffPinner_SeesEverything()
    {
        var bulletin = MakeBulletin(createdById: AuthorId);
        var scope = MakeScope(UserType.Staff, OtherUserId, canView: false, canEdit: false, canPin: true);
        Assert.That(_policy.CanView(bulletin, scope), Is.True);
    }

    [Test]
    public void CanView_StaffCreator_SeesOwnBulletin()
    {
        var bulletin = MakeBulletin(createdById: AuthorId);
        var scope = MakeScope(UserType.Staff, AuthorId, canView: false, canEdit: true, canPin: false);
        Assert.That(_policy.CanView(bulletin, scope), Is.True);
    }

    [Test]
    public void CanView_StaffNonCreator_DefersToSqlAudienceFilter()
    {
        // Non-pinner, non-creator staff still get CanView=true from the policy; the
        // audience filter in the SP / summaries SQL is what limits the actual rows.
        var bulletin = MakeBulletin(createdById: AuthorId);
        var scope = MakeScope(UserType.Staff, OtherUserId, canView: true, canEdit: false, canPin: false);
        Assert.That(_policy.CanView(bulletin, scope), Is.True);
    }

    // ─── CanEdit ─────────────────────────────────────────────────────────────

    [TestCase(UserType.Student)]
    [TestCase(UserType.Parent)]
    public void CanEdit_NonStaff_AlwaysDenied_EvenWithAllPermissions(UserType userType)
    {
        var bulletin = MakeBulletin(createdById: AuthorId);
        var scope = MakeScope(userType, AuthorId, canView: true, canEdit: true, canPin: true);
        Assert.That(_policy.CanEdit(bulletin, scope), Is.False);
    }

    [Test]
    public void CanEdit_StaffPinner_CanEditAnyBulletin()
    {
        var bulletin = MakeBulletin(createdById: AuthorId);
        var scope = MakeScope(UserType.Staff, OtherUserId, canView: false, canEdit: false, canPin: true);
        Assert.That(_policy.CanEdit(bulletin, scope), Is.True);
    }

    [Test]
    public void CanEdit_StaffEditor_CanEditOwnBulletin()
    {
        var bulletin = MakeBulletin(createdById: AuthorId);
        var scope = MakeScope(UserType.Staff, AuthorId, canView: false, canEdit: true, canPin: false);
        Assert.That(_policy.CanEdit(bulletin, scope), Is.True);
    }

    [Test]
    public void CanEdit_StaffEditor_CannotEditOthersBulletin()
    {
        var bulletin = MakeBulletin(createdById: AuthorId);
        var scope = MakeScope(UserType.Staff, OtherUserId, canView: false, canEdit: true, canPin: false);
        Assert.That(_policy.CanEdit(bulletin, scope), Is.False);
    }

    [Test]
    public void CanEdit_StaffViewer_CannotEdit()
    {
        var bulletin = MakeBulletin(createdById: AuthorId);
        var scope = MakeScope(UserType.Staff, AuthorId, canView: true, canEdit: false, canPin: false);
        Assert.That(_policy.CanEdit(bulletin, scope), Is.False);
    }

    // ─── helpers ─────────────────────────────────────────────────────────────

    private static Bulletin MakeBulletin(Guid createdById) =>
        new()
        {
            Id = Guid.NewGuid(),
            DirectoryId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Title = "Test",
            Detail = "Test",
            CreatedById = createdById,
            CreatedByIpAddress = "::1",
            LastModifiedById = createdById,
            LastModifiedByIpAddress = "::1"
        };

    private static BulletinVisibilityScope MakeScope(UserType userType, Guid? userId, bool canView, bool canEdit, bool canPin) =>
        new(userId, userType, canView, canEdit, canPin);
}
