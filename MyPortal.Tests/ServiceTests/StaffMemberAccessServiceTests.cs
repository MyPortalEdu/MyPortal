using System.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.People;
using MyPortal.Services.People;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class StaffMemberAccessServiceTests
{
    private Mock<IAuthorizationService> _auth;
    private Mock<IStaffMemberRepository> _repo;
    private StaffMemberAccessService _service;

    private Guid _subjectId;

    [SetUp]
    public void Setup()
    {
        _auth = new Mock<IAuthorizationService>();
        _repo = new Mock<IStaffMemberRepository>();
        _service = new StaffMemberAccessService(_auth.Object,
            NullLogger<StaffMemberAccessService>.Instance, _repo.Object);

        _subjectId = Guid.NewGuid();

        SetPermissions();
    }

    private void AsSelf()
    {
        var personId = Guid.NewGuid();
        _auth.Setup(a => a.GetCurrentUserPersonId()).Returns(personId);
        _repo.Setup(r => r.GetByIdAsync(_subjectId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction>()))
            .ReturnsAsync(new StaffMember { Id = _subjectId, PersonId = personId, Code = "S1" });
    }

    private void AsLineManaged()
    {
        var viewerPersonId = Guid.NewGuid();
        var viewerStaffId = Guid.NewGuid();
        _auth.Setup(a => a.GetCurrentUserPersonId()).Returns(viewerPersonId);
        _repo.Setup(r => r.GetByIdAsync(_subjectId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction>()))
            .ReturnsAsync(new StaffMember { Id = _subjectId, PersonId = Guid.NewGuid(), Code = "S1" });
        _repo.Setup(r => r.GetStaffMemberIdByPersonIdAsync(viewerPersonId, It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction>()))
            .ReturnsAsync(viewerStaffId);
        _repo.Setup(r => r.IsManagedByAsync(_subjectId, viewerStaffId, It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction>()))
            .ReturnsAsync(true);
    }

    private void AsUnrelated()
    {
        var viewerPersonId = Guid.NewGuid();
        _auth.Setup(a => a.GetCurrentUserPersonId()).Returns(viewerPersonId);
        _repo.Setup(r => r.GetByIdAsync(_subjectId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction>()))
            .ReturnsAsync(new StaffMember { Id = _subjectId, PersonId = Guid.NewGuid(), Code = "S1" });
        _repo.Setup(r => r.GetStaffMemberIdByPersonIdAsync(viewerPersonId, It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction>()))
            .ReturnsAsync(Guid.NewGuid());
        _repo.Setup(r => r.IsManagedByAsync(_subjectId, It.IsAny<Guid>(), It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction>()))
            .ReturnsAsync(false);
    }

    private void AsNoPersonIdentity()
    {
        _auth.Setup(a => a.GetCurrentUserPersonId()).Returns((Guid?)null);
    }

    private void AsViewerNotStaff()
    {
        var viewerPersonId = Guid.NewGuid();
        _auth.Setup(a => a.GetCurrentUserPersonId()).Returns(viewerPersonId);
        _repo.Setup(r => r.GetByIdAsync(_subjectId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction>()))
            .ReturnsAsync(new StaffMember { Id = _subjectId, PersonId = Guid.NewGuid(), Code = "S1" });
        _repo.Setup(r => r.GetStaffMemberIdByPersonIdAsync(viewerPersonId, It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction>()))
            .ReturnsAsync((Guid?)null);
    }

    private void SetPermissions(params string[] permissions)
    {
        _auth.Setup(a => a.GetPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase));
    }

    private Task<bool> Can(StaffArea area, StaffAccess acceptable)
        => _service.CanAsync(_subjectId, area, acceptable, CancellationToken.None);

    [Test]
    public async Task GetRelationship_PersonLessViewer_IsUnrelated()
    {
        AsNoPersonIdentity();

        Assert.That(await _service.GetRelationshipAsync(_subjectId, CancellationToken.None),
            Is.EqualTo(StaffRelationship.Unrelated));

        _repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction>()),
            Times.Never, "Should short-circuit before any subject lookup.");
    }

    [Test]
    public async Task GetRelationship_SubjectSharesPersonWithViewer_IsSelf()
    {
        AsSelf();

        Assert.That(await _service.GetRelationshipAsync(_subjectId, CancellationToken.None),
            Is.EqualTo(StaffRelationship.Self));
    }

    [Test]
    public async Task GetRelationship_ViewerInChainAboveSubject_IsLineManaged()
    {
        AsLineManaged();

        Assert.That(await _service.GetRelationshipAsync(_subjectId, CancellationToken.None),
            Is.EqualTo(StaffRelationship.LineManaged));
    }

    [Test]
    public async Task GetRelationship_ViewerIsNotStaff_IsUnrelated()
    {
        AsViewerNotStaff();

        Assert.That(await _service.GetRelationshipAsync(_subjectId, CancellationToken.None),
            Is.EqualTo(StaffRelationship.Unrelated));
    }

    [Test]
    public async Task GetRelationship_MissingSubject_IsUnrelated()
    {
        _auth.Setup(a => a.GetCurrentUserPersonId()).Returns(Guid.NewGuid());
        _repo.Setup(r => r.GetByIdAsync(_subjectId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction>()))
            .ReturnsAsync((StaffMember?)null);

        Assert.That(await _service.GetRelationshipAsync(_subjectId, CancellationToken.None),
            Is.EqualTo(StaffRelationship.Unrelated));
    }

    // All scope: grants regardless of relationship.

    [Test]
    public async Task AllScope_GrantsToUnrelatedViewer()
    {
        AsUnrelated();
        SetPermissions(Permissions.Staff.ViewAllStaffEmploymentDetails);

        Assert.That(await Can(StaffArea.EmploymentDetails, StaffAccess.ViewAll), Is.True);
    }

    [Test]
    public async Task AllScope_GrantsToSelf()
    {
        AsSelf();
        SetPermissions(Permissions.Staff.ViewAllStaffEmploymentDetails);

        Assert.That(await Can(StaffArea.EmploymentDetails, StaffAccess.ViewAll), Is.True);
    }

    // Own scope: self only.

    [Test]
    public async Task OwnScope_GrantsToSelf()
    {
        AsSelf();
        SetPermissions(Permissions.Staff.ViewOwnStaffEmploymentDetails);

        Assert.That(await Can(StaffArea.EmploymentDetails, StaffAccess.ViewOwn), Is.True);
    }

    [Test]
    public async Task OwnScope_DeniesLineManagedViewer()
    {
        AsLineManaged();
        SetPermissions(Permissions.Staff.ViewOwnStaffEmploymentDetails);

        Assert.That(await Can(StaffArea.EmploymentDetails, StaffAccess.ViewOwn), Is.False);
    }

    [Test]
    public async Task OwnScope_DeniesUnrelatedViewer()
    {
        AsUnrelated();
        SetPermissions(Permissions.Staff.ViewOwnStaffEmploymentDetails);

        Assert.That(await Can(StaffArea.EmploymentDetails, StaffAccess.ViewOwn), Is.False);
    }

    // Managed scope: line-managed only; never self.

    [Test]
    public async Task ManagedScope_GrantsToLineManagedViewer()
    {
        AsLineManaged();
        SetPermissions(Permissions.Staff.ViewManagedStaffBasicDetails);

        Assert.That(await Can(StaffArea.BasicDetails, StaffAccess.ViewManaged), Is.True);
    }

    [Test]
    public async Task ManagedScope_DoesNotCoverSelf()
    {
        // A viewer is never their own report: holding only Managed must NOT grant on own record.
        AsSelf();
        SetPermissions(Permissions.Staff.ViewManagedStaffBasicDetails);

        Assert.That(await Can(StaffArea.BasicDetails, StaffAccess.ViewManaged), Is.False);
    }

    [Test]
    public async Task ManagedScope_DeniesUnrelatedViewer()
    {
        AsUnrelated();
        SetPermissions(Permissions.Staff.ViewManagedStaffBasicDetails);

        Assert.That(await Can(StaffArea.BasicDetails, StaffAccess.ViewManaged), Is.False);
    }

    [Test]
    public async Task GrantsViaManagedBranch_WhenAcceptableIncludesAll()
    {
        // Endpoint declares Own | Managed | All; viewer holds only Managed, is LineManaged → grant.
        AsLineManaged();
        SetPermissions(Permissions.Staff.ViewManagedStaffBasicDetails);

        Assert.That(await Can(StaffArea.BasicDetails,
            StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll), Is.True);
    }

    [Test]
    public async Task NoMatchingPermission_Denies()
    {
        // Endpoint accepts ViewOwn + ViewAll on Employment; viewer is Self but holds neither.
        AsSelf();
        SetPermissions("Staff.SomeOtherPermission");

        Assert.That(await Can(StaffArea.EmploymentDetails, StaffAccess.ViewOwn | StaffAccess.ViewAll),
            Is.False);
    }

    [Test]
    public async Task AccessFlagWithNoSeededPermissionForArea_IsNeverGranted()
    {
        // Employment has no Managed scope in the catalogue. A LineManaged viewer holding a
        // (non-existent) managed-employment string still gets nothing — the lookup misses.
        AsLineManaged();
        SetPermissions("Staff.ViewManagedStaffEmploymentDetails");  // not seeded

        Assert.That(await Can(StaffArea.EmploymentDetails,
            StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll), Is.False);
    }

    [Test]
    public async Task NoPersonIdentity_AllScopeStillApplies()
    {
        AsNoPersonIdentity();
        SetPermissions(Permissions.Staff.ViewAllStaffBasicDetails);

        Assert.That(await Can(StaffArea.BasicDetails, StaffAccess.ViewOwn | StaffAccess.ViewAll),
            Is.True);
    }

    [Test]
    public async Task NoPersonIdentity_OwnScopeDeniedAndNoSubjectLookup()
    {
        AsNoPersonIdentity();
        SetPermissions(Permissions.Staff.ViewOwnStaffBasicDetails);

        Assert.That(await Can(StaffArea.BasicDetails, StaffAccess.ViewOwn), Is.False);
        _repo.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction>()),
            Times.Never, "Person-less viewer should short-circuit before any subject lookup.");
    }

    [Test]
    public void RequireAsync_Throws_WhenDenied()
    {
        AsUnrelated();
        SetPermissions();

        Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.RequireAsync(_subjectId, StaffArea.EmploymentDetails, StaffAccess.ViewAll,
                CancellationToken.None));
    }

    [Test]
    public void RequireAsync_DoesNotThrow_WhenAllowed()
    {
        AsUnrelated();
        SetPermissions(Permissions.Staff.ViewAllStaffEmploymentDetails);

        Assert.DoesNotThrowAsync(() =>
            _service.RequireAsync(_subjectId, StaffArea.EmploymentDetails, StaffAccess.ViewAll,
                CancellationToken.None));
    }

    [Test]
    public void CanAsync_WithNoneFlag_Throws()
    {
        // Passing StaffAccess.None would silently always deny; surface the bug instead.
        AsSelf();

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CanAsync(_subjectId, StaffArea.BasicDetails, StaffAccess.None,
                CancellationToken.None));
    }
}
