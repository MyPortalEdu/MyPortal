using Moq;
using MyPortal.Common.Enums;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.VisibilityScopes;
using MyPortal.Services.School.Bulletins;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class BulletinAccessPolicyTests
{
    private static readonly Guid AuthorId    = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private Mock<IBulletinRepository> _bulletinRepository = null!;
    private BulletinAccessPolicy _policy = null!;

    [SetUp]
    public void Setup()
    {
        _bulletinRepository = new Mock<IBulletinRepository>(MockBehavior.Strict);
        _policy = new BulletinAccessPolicy(_bulletinRepository.Object);
    }

    // ─── CanViewAsync ────────────────────────────────────────────────────────
    // Staff pinners and staff creators short-circuit in-memory. Everyone else
    // (including non-creator staff with view-only permission) hits the audience
    // SP via IBulletinRepository.IsVisibleToUserAsync.

    [Test]
    public async Task CanViewAsync_StaffPinner_ReturnsTrue_WithoutDbCall()
    {
        var bulletin = MakeBulletin(createdById: AuthorId);
        var scope = MakeScope(UserType.Staff, OtherUserId, canView: false, canEdit: false, canPin: true);

        Assert.That(await _policy.CanViewAsync(bulletin, scope, CancellationToken.None), Is.True);
        _bulletinRepository.Verify(r => r.IsVisibleToUserAsync(
            It.IsAny<Guid>(), It.IsAny<BulletinVisibilityScope>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task CanViewAsync_StaffCreator_ReturnsTrue_WithoutDbCall()
    {
        var bulletin = MakeBulletin(createdById: AuthorId);
        var scope = MakeScope(UserType.Staff, AuthorId, canView: false, canEdit: true, canPin: false);

        Assert.That(await _policy.CanViewAsync(bulletin, scope, CancellationToken.None), Is.True);
        _bulletinRepository.Verify(r => r.IsVisibleToUserAsync(
            It.IsAny<Guid>(), It.IsAny<BulletinVisibilityScope>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestCase(UserType.Student, true)]
    [TestCase(UserType.Student, false)]
    [TestCase(UserType.Parent, true)]
    [TestCase(UserType.Parent, false)]
    public async Task CanViewAsync_NonStaff_DelegatesToAudienceSp(UserType userType, bool spResult)
    {
        var bulletin = MakeBulletin(createdById: AuthorId);
        var scope = MakeScope(userType, OtherUserId, canView: false, canEdit: false, canPin: false);

        _bulletinRepository.Setup(r => r.IsVisibleToUserAsync(bulletin.Id, scope, It.IsAny<CancellationToken>()))
            .ReturnsAsync(spResult);

        Assert.That(await _policy.CanViewAsync(bulletin, scope, CancellationToken.None), Is.EqualTo(spResult));
        _bulletinRepository.Verify(r => r.IsVisibleToUserAsync(
            bulletin.Id, scope, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanViewAsync_StaffNonCreatorNonPinner_DelegatesToAudienceSp(bool spResult)
    {
        // Non-pinner, non-creator staff have no in-memory short-circuit — the SP is the
        // gate. (For non-creator staff with no pin permission, the SP allows them via
        // the AudienceKind = Staff branch when the bulletin targets staff.)
        var bulletin = MakeBulletin(createdById: AuthorId);
        var scope = MakeScope(UserType.Staff, OtherUserId, canView: true, canEdit: false, canPin: false);

        _bulletinRepository.Setup(r => r.IsVisibleToUserAsync(bulletin.Id, scope, It.IsAny<CancellationToken>()))
            .ReturnsAsync(spResult);

        Assert.That(await _policy.CanViewAsync(bulletin, scope, CancellationToken.None), Is.EqualTo(spResult));
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
