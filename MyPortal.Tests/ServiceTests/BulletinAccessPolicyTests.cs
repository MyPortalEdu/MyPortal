using Moq;
using MyPortal.Common.Enums;
using MyPortal.Core.Entities;
using MyPortal.Data.VisibilityScopes;
using MyPortal.Services.Interfaces.Providers;
using MyPortal.Services.School.Bulletins;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class BulletinAccessPolicyTests
{
    private static readonly DateTime NowUtc = new(2026, 4, 29, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid AuthorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private BulletinAccessPolicy _policy = null!;

    [SetUp]
    public void Setup()
    {
        var clock = new Mock<IDateTimeProvider>();
        clock.SetupGet(c => c.UtcNow).Returns(NowUtc);
        _policy = new BulletinAccessPolicy(clock.Object);
    }

    // ─── CanView ─────────────────────────────────────────────────────────────

    [TestCase(UserType.Student, false, false, false)]
    [TestCase(UserType.Student, true, true, true)]
    [TestCase(UserType.Parent, false, false, false)]
    [TestCase(UserType.Parent, true, true, true)]
    public void CanView_PrivateBulletin_HiddenFromNonStaff_RegardlessOfPermissions(
        UserType userType, bool canView, bool canEdit, bool canApprove)
    {
        var bulletin = MakeBulletin(isPrivate: true, isApproved: true, expiresInDays: 7, createdById: AuthorId);
        var scope = MakeScope(userType, OtherUserId, canView, canEdit, canApprove);
        Assert.That(_policy.CanView(bulletin, scope), Is.False);
    }

    [TestCase(false, false, null)]
    [TestCase(false, false, -1)]
    [TestCase(false, true, null)]
    [TestCase(true, false, -1)]
    [TestCase(true, true, 7)]
    public void CanView_StaffApprover_SeesEverything(bool isPrivate, bool isApproved, int? expiresInDays)
    {
        var bulletin = MakeBulletin(isPrivate, isApproved, expiresInDays, AuthorId);
        var scope = MakeScope(UserType.Staff, OtherUserId, canView: false, canEdit: false, canApprove: true);
        Assert.That(_policy.CanView(bulletin, scope), Is.True);
    }

    [TestCase(true, 7, true)]
    [TestCase(true, null, true)]
    [TestCase(false, 7, false)]
    [TestCase(true, -1, false)]
    public void CanView_NonStaffViewer_RequiresApprovedAndNotExpired(
        bool isApproved, int? expiresInDays, bool expected)
    {
        var bulletin = MakeBulletin(isPrivate: false, isApproved, expiresInDays, AuthorId);
        var scope = MakeScope(UserType.Student, OtherUserId, canView: true, canEdit: false, canApprove: false);
        Assert.That(_policy.CanView(bulletin, scope), Is.EqualTo(expected));
    }

    [TestCase(false, false, -1, true)]
    [TestCase(false, false, null, true)]
    [TestCase(true, false, null, false)]
    public void CanView_NonStaffApprover_BypassesApprovalAndExpiry_ButNotPrivacy(
        bool isPrivate, bool isApproved, int? expiresInDays, bool expected)
    {
        var bulletin = MakeBulletin(isPrivate, isApproved, expiresInDays, AuthorId);
        var scope = MakeScope(UserType.Student, OtherUserId, canView: false, canEdit: false, canApprove: true);
        Assert.That(_policy.CanView(bulletin, scope), Is.EqualTo(expected));
    }

    [Test]
    public void CanView_StaffWithNoPermissions_HiddenEvenForApprovedBulletin()
    {
        var bulletin = MakeBulletin(isPrivate: false, isApproved: true, expiresInDays: 7, createdById: AuthorId);
        var scope = MakeScope(UserType.Staff, OtherUserId, canView: false, canEdit: false, canApprove: false);
        Assert.That(_policy.CanView(bulletin, scope), Is.False);
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    public void CanView_StaffEditor_SeesOwnBulletinInAnyState(bool isApproved, bool isPrivate)
    {
        var bulletin = MakeBulletin(isPrivate, isApproved, expiresInDays: -10, createdById: AuthorId);
        var scope = MakeScope(UserType.Staff, AuthorId, canView: false, canEdit: true, canApprove: false);
        Assert.That(_policy.CanView(bulletin, scope), Is.True);
    }

    [TestCase(true, 7, true)]
    [TestCase(false, 7, false)]
    [TestCase(true, -1, false)]
    public void CanView_StaffEditor_OnOthersBulletin_RequiresApprovedAndNotExpired(
        bool isApproved, int expiresInDays, bool expected)
    {
        var bulletin = MakeBulletin(isPrivate: false, isApproved, expiresInDays, AuthorId);
        var scope = MakeScope(UserType.Staff, OtherUserId, canView: false, canEdit: true, canApprove: false);
        Assert.That(_policy.CanView(bulletin, scope), Is.EqualTo(expected));
    }

    [TestCase(true, 7, true)]
    [TestCase(false, 7, false)]
    [TestCase(true, -1, false)]
    public void CanView_StaffViewer_RequiresApprovedAndNotExpired(
        bool isApproved, int expiresInDays, bool expected)
    {
        var bulletin = MakeBulletin(isPrivate: false, isApproved, expiresInDays, AuthorId);
        var scope = MakeScope(UserType.Staff, OtherUserId, canView: true, canEdit: false, canApprove: false);
        Assert.That(_policy.CanView(bulletin, scope), Is.EqualTo(expected));
    }

    // ─── CanEdit ─────────────────────────────────────────────────────────────

    [TestCase(UserType.Student)]
    [TestCase(UserType.Parent)]
    public void CanEdit_NonStaff_AlwaysDenied_EvenWithAllPermissions(UserType userType)
    {
        var bulletin = MakeBulletin(isPrivate: false, isApproved: true, expiresInDays: 7, createdById: AuthorId);
        var scope = MakeScope(userType, AuthorId, canView: true, canEdit: true, canApprove: true);
        Assert.That(_policy.CanEdit(bulletin, scope), Is.False);
    }

    [Test]
    public void CanEdit_StaffApprover_CanEditAnyBulletin()
    {
        var bulletin = MakeBulletin(isPrivate: false, isApproved: false, expiresInDays: -10, createdById: AuthorId);
        var scope = MakeScope(UserType.Staff, OtherUserId, canView: false, canEdit: false, canApprove: true);
        Assert.That(_policy.CanEdit(bulletin, scope), Is.True);
    }

    [Test]
    public void CanEdit_StaffEditor_CanEditOwnBulletin()
    {
        var bulletin = MakeBulletin(isPrivate: false, isApproved: false, expiresInDays: null, createdById: AuthorId);
        var scope = MakeScope(UserType.Staff, AuthorId, canView: false, canEdit: true, canApprove: false);
        Assert.That(_policy.CanEdit(bulletin, scope), Is.True);
    }

    [Test]
    public void CanEdit_StaffEditor_CannotEditOthersBulletin()
    {
        var bulletin = MakeBulletin(isPrivate: false, isApproved: true, expiresInDays: 7, createdById: AuthorId);
        var scope = MakeScope(UserType.Staff, OtherUserId, canView: false, canEdit: true, canApprove: false);
        Assert.That(_policy.CanEdit(bulletin, scope), Is.False);
    }

    [Test]
    public void CanEdit_StaffViewer_CannotEdit()
    {
        var bulletin = MakeBulletin(isPrivate: false, isApproved: true, expiresInDays: 7, createdById: AuthorId);
        var scope = MakeScope(UserType.Staff, AuthorId, canView: true, canEdit: false, canApprove: false);
        Assert.That(_policy.CanEdit(bulletin, scope), Is.False);
    }

    // ─── helpers ─────────────────────────────────────────────────────────────

    private static Bulletin MakeBulletin(bool isPrivate, bool isApproved, int? expiresInDays, Guid createdById) =>
        new()
        {
            Id = Guid.NewGuid(),
            DirectoryId = Guid.NewGuid(),
            Title = "Test",
            Detail = "Test",
            IsPrivate = isPrivate,
            IsApproved = isApproved,
            ExpiresAt = expiresInDays.HasValue ? NowUtc.AddDays(expiresInDays.Value) : null,
            CreatedById = createdById,
            CreatedByIpAddress = "::1",
            LastModifiedById = createdById,
            LastModifiedByIpAddress = "::1"
        };

    private static BulletinVisibilityScope MakeScope(UserType userType, Guid? userId, bool canView, bool canEdit, bool canApprove) =>
        new(userId, userType, canView, canEdit, canApprove);
}
