using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces;
using MyPortal.Data.VisibilityScopes;
using MyPortal.Services.Interfaces.Security;
using MyPortal.Services.School.Bulletins;
using QueryKit.Repositories.Exceptions;
using Task = System.Threading.Tasks.Task;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.Documents;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class BulletinServiceTests
{
    private static readonly Guid CurrentUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private Mock<IAuthorizationService> _authorizationService;
    private Mock<ILogger<BulletinService>> _logger;
    private Mock<IDirectoryService> _directoryService;
    private Mock<IDocumentService> _documentService;
    private Mock<IValidationService> _validationService;
    private Mock<IBulletinRepository> _bulletinRepository;
    private Mock<IAccessPolicy<Bulletin, BulletinVisibilityScope>> _accessPolicy;
    private Mock<IUnitOfWorkFactory> _unitOfWorkFactory;
    private Mock<IUnitOfWork> _unitOfWork;
    private Mock<IDbTransaction> _transaction;

    private BulletinService _service;

    [SetUp]
    public void Setup()
    {
        _authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        _logger = new Mock<ILogger<BulletinService>>(MockBehavior.Loose);
        _directoryService = new Mock<IDirectoryService>(MockBehavior.Strict);
        _documentService = new Mock<IDocumentService>(MockBehavior.Strict);
        _validationService = new Mock<IValidationService>(MockBehavior.Strict);
        _bulletinRepository = new Mock<IBulletinRepository>(MockBehavior.Strict);
        _accessPolicy = new Mock<IAccessPolicy<Bulletin, BulletinVisibilityScope>>(MockBehavior.Strict);
        _unitOfWorkFactory = new Mock<IUnitOfWorkFactory>(MockBehavior.Strict);
        _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Loose);
        _transaction = new Mock<IDbTransaction>(MockBehavior.Loose);

        _unitOfWork.SetupGet(u => u.Transaction).Returns(_transaction.Object);
        _unitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.DisposeAsync()).Returns(ValueTask.CompletedTask);
        _unitOfWorkFactory.Setup(f => f.BeginAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWork.Object);

        // Default scope: staff editor with edit permission. Tests can re-stub these.
        SetupScope(UserType.Staff, CurrentUserId, canView: true, canEdit: true, canApprove: false);

        _service = new BulletinService(
            _authorizationService.Object,
            _logger.Object,
            _directoryService.Object,
            _documentService.Object,
            _validationService.Object,
            _bulletinRepository.Object,
            _accessPolicy.Object,
            _unitOfWorkFactory.Object
        );
    }

    private void SetupScope(UserType userType, Guid? userId, bool canView, bool canEdit, bool canApprove)
    {
        _authorizationService.Setup(a => a.GetCurrentUserId()).Returns(userId);
        _authorizationService.Setup(a => a.GetCurrentUserType()).Returns(userType);
        _authorizationService.Setup(a =>
                a.HasPermissionAsync(Permissions.School.ViewSchoolBulletins, It.IsAny<CancellationToken>()))
            .ReturnsAsync(canView);
        _authorizationService.Setup(a =>
                a.HasPermissionAsync(Permissions.School.EditSchoolBulletins, It.IsAny<CancellationToken>()))
            .ReturnsAsync(canEdit);
        _authorizationService.Setup(a =>
                a.HasPermissionAsync(Permissions.School.ApproveSchoolBulletins, It.IsAny<CancellationToken>()))
            .ReturnsAsync(canApprove);
    }

    private void RequirePermission(string permission, bool succeeds = true)
    {
        var setup = _authorizationService.Setup(a =>
            a.RequirePermissionAsync(permission, It.IsAny<CancellationToken>()));
        if (succeeds)
            setup.Returns(Task.CompletedTask);
        else
            setup.ThrowsAsync(new ForbiddenException($"missing permission: {permission}"));
    }

    private static Bulletin MakeBulletin(Guid? id = null, Guid? directoryId = null, long version = 1,
        bool isApproved = true, Guid? createdById = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            DirectoryId = directoryId ?? Guid.NewGuid(),
            Title = "Test",
            Detail = "Test detail",
            IsPrivate = false,
            IsApproved = isApproved,
            CreatedById = createdById ?? CurrentUserId,
            CreatedByIpAddress = "::1",
            LastModifiedById = createdById ?? CurrentUserId,
            LastModifiedByIpAddress = "::1",
            Version = version
        };

    // ─── GetDetailsByIdAsync ─────────────────────────────────────────────────

    [Test]
    public async Task GetDetailsByIdAsync_ReturnsDetails_WhenAccessPolicyAllowsView()
    {
        var bulletinId = Guid.NewGuid();
        var bulletin = MakeBulletin(bulletinId);
        var dto = new BulletinDetailsResponse { Id = bulletinId, Title = "Test" };

        _bulletinRepository.Setup(r => r.GetByIdAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);
        _accessPolicy.Setup(p => p.CanView(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _bulletinRepository.Setup(r => r.GetDetailsByIdAsync(bulletinId,
                It.IsAny<BulletinVisibilityScope>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _service.GetDetailsByIdAsync(bulletinId, CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
    }

    [Test]
    public void GetDetailsByIdAsync_Throws_NotFound_WhenAccessPolicyDeniesView()
    {
        var bulletinId = Guid.NewGuid();
        var bulletin = MakeBulletin(bulletinId);

        _bulletinRepository.Setup(r => r.GetByIdAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);
        // Defense in depth: even if the SP would have returned a row, the policy denies.
        _accessPolicy.Setup(p => p.CanView(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(false);

        Assert.That(async () => await _service.GetDetailsByIdAsync(bulletinId, CancellationToken.None),
            Throws.TypeOf<NotFoundException>());

        // Critical: the detail SP must NOT be called when the policy denies view, otherwise
        // we leak whatever the SP-side filter happened to allow through.
        _bulletinRepository.Verify(r => r.GetDetailsByIdAsync(It.IsAny<Guid>(),
            It.IsAny<BulletinVisibilityScope>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void GetDetailsByIdAsync_Throws_NotFound_WhenBulletinMissing()
    {
        var bulletinId = Guid.NewGuid();
        _bulletinRepository.Setup(r => r.GetByIdAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((Bulletin?)null);

        Assert.That(async () => await _service.GetDetailsByIdAsync(bulletinId, CancellationToken.None),
            Throws.TypeOf<NotFoundException>());
    }

    // ─── CreateAsync ─────────────────────────────────────────────────

    [Test]
    public async Task CreateAsync_RequiresEdit_ThenCreatesDirectoryAndBulletin()
    {
        RequirePermission(Permissions.School.EditSchoolBulletins);

        var directoryId = Guid.NewGuid();
        _directoryService.Setup(d => d.CreateAsync(It.IsAny<DirectoryUpsertRequest>(),
                It.IsAny<CancellationToken>(), It.IsAny<IUnitOfWork?>()))
            .ReturnsAsync(new DirectoryDetailsResponse { Id = directoryId, Name = "bulletin-x" });
        _bulletinRepository.Setup(r => r.InsertAsync(It.IsAny<Bulletin>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((Bulletin b, CancellationToken _, IDbTransaction? _) => b);

        var model = new BulletinUpsertRequest
        {
            Title = "Hello",
            Detail = "World",
            IsPrivate = true,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var resultId = await _service.CreateAsync(model, CancellationToken.None);

        Assert.That(resultId, Is.Not.EqualTo(Guid.Empty));
        _authorizationService.Verify(a =>
            a.RequirePermissionAsync(Permissions.School.EditSchoolBulletins, It.IsAny<CancellationToken>()),
            Times.Once);
        _directoryService.Verify(d => d.CreateAsync(
            It.Is<DirectoryUpsertRequest>(r => r.IsPrivate == model.IsPrivate),
            It.IsAny<CancellationToken>(), It.IsAny<IUnitOfWork?>()), Times.Once);
        _bulletinRepository.Verify(r => r.InsertAsync(
            It.Is<Bulletin>(b =>
                b.Id == resultId &&
                b.DirectoryId == directoryId &&
                b.Title == "Hello" &&
                b.Detail == "World" &&
                b.IsPrivate),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Once);
    }

    [Test]
    public void CreateAsync_PropagatesPermissionDenial()
    {
        RequirePermission(Permissions.School.EditSchoolBulletins, succeeds: false);

        var model = new BulletinUpsertRequest { Title = "X", Detail = "Y" };

        Assert.That(async () => await _service.CreateAsync(model, CancellationToken.None),
            Throws.TypeOf<ForbiddenException>());

        _directoryService.Verify(d => d.CreateAsync(It.IsAny<DirectoryUpsertRequest>(),
            It.IsAny<CancellationToken>(), It.IsAny<IUnitOfWork?>()), Times.Never);
        _bulletinRepository.Verify(r => r.InsertAsync(It.IsAny<Bulletin>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    // ─── UpdateAsync ─────────────────────────────────────────────────

    [Test]
    public async Task UpdateAsync_AppliesFields_AndHandsExpectedVersionToRepoForOptimisticConcurrency()
    {
        RequirePermission(Permissions.School.EditSchoolBulletins);

        var bulletinId = Guid.NewGuid();
        var bulletin = MakeBulletin(bulletinId, version: 5);

        _bulletinRepository.Setup(r => r.GetByIdAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);
        _accessPolicy.Setup(p => p.CanView(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _accessPolicy.Setup(p => p.CanEdit(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _bulletinRepository.Setup(r => r.UpdateAsync(bulletin, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);

        var model = new BulletinUpsertRequest
        {
            Title = "Updated",
            Detail = "Updated detail",
            IsPrivate = true,
            ExpiresAt = DateTime.UtcNow.AddDays(14),
            ExpectedVersion = 5
        };

        await _service.UpdateAsync(bulletinId, model, CancellationToken.None);

        Assert.That(bulletin.Title, Is.EqualTo("Updated"));
        Assert.That(bulletin.Detail, Is.EqualTo("Updated detail"));
        Assert.That(bulletin.IsPrivate, Is.True);
        // Version is set to the client's expected value so QueryKit's UpdateWithVersionAsync
        // turns it into a WHERE Version=@expected guard.
        Assert.That(bulletin.Version, Is.EqualTo(5));
    }

    [Test]
    public async Task UpdateAsync_NonApprover_FlipsIsApprovedFalse()
    {
        // Default scope from SetUp has canApprove = false.
        RequirePermission(Permissions.School.EditSchoolBulletins);

        var bulletinId = Guid.NewGuid();
        var bulletin = MakeBulletin(bulletinId, isApproved: true);

        _bulletinRepository.Setup(r => r.GetByIdAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);
        _accessPolicy.Setup(p => p.CanView(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _accessPolicy.Setup(p => p.CanEdit(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _bulletinRepository.Setup(r => r.UpdateAsync(bulletin, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);

        await _service.UpdateAsync(bulletinId, new BulletinUpsertRequest
        {
            Title = "T", Detail = "D", ExpectedVersion = 1
        }, CancellationToken.None);

        Assert.That(bulletin.IsApproved, Is.False, "Non-approver edits must require re-approval.");
    }

    [Test]
    public async Task UpdateAsync_Approver_PreservesIsApproved()
    {
        SetupScope(UserType.Staff, CurrentUserId, canView: true, canEdit: true, canApprove: true);
        RequirePermission(Permissions.School.EditSchoolBulletins);

        var bulletinId = Guid.NewGuid();
        var bulletin = MakeBulletin(bulletinId, isApproved: true);

        _bulletinRepository.Setup(r => r.GetByIdAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);
        _accessPolicy.Setup(p => p.CanView(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _accessPolicy.Setup(p => p.CanEdit(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _bulletinRepository.Setup(r => r.UpdateAsync(bulletin, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);

        await _service.UpdateAsync(bulletinId, new BulletinUpsertRequest
        {
            Title = "T", Detail = "D", ExpectedVersion = 1
        }, CancellationToken.None);

        Assert.That(bulletin.IsApproved, Is.True);
    }

    [Test]
    public void UpdateAsync_Throws_Forbidden_WhenAccessPolicyDeniesEdit()
    {
        RequirePermission(Permissions.School.EditSchoolBulletins);

        var bulletinId = Guid.NewGuid();
        var bulletin = MakeBulletin(bulletinId);

        _bulletinRepository.Setup(r => r.GetByIdAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);
        _accessPolicy.Setup(p => p.CanView(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _accessPolicy.Setup(p => p.CanEdit(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(false);

        Assert.That(async () => await _service.UpdateAsync(bulletinId,
                new BulletinUpsertRequest { Title = "X", Detail = "Y", ExpectedVersion = 1 },
                CancellationToken.None),
            Throws.TypeOf<ForbiddenException>());

        _bulletinRepository.Verify(r => r.UpdateAsync(It.IsAny<Bulletin>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    [Test]
    public void UpdateAsync_PropagatesConcurrencyExceptionFromRepo()
    {
        RequirePermission(Permissions.School.EditSchoolBulletins);

        var bulletinId = Guid.NewGuid();
        var bulletin = MakeBulletin(bulletinId, version: 5);

        _bulletinRepository.Setup(r => r.GetByIdAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);
        _accessPolicy.Setup(p => p.CanView(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _accessPolicy.Setup(p => p.CanEdit(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _bulletinRepository.Setup(r => r.UpdateAsync(bulletin, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ThrowsAsync(new ConcurrencyException("version mismatch"));

        var staleClient = new BulletinUpsertRequest { Title = "X", Detail = "Y", ExpectedVersion = 4 };

        Assert.That(async () => await _service.UpdateAsync(bulletinId, staleClient, CancellationToken.None),
            Throws.TypeOf<ConcurrencyException>());
    }

    // ─── UpdateBulletinApprovalAsync ─────────────────────────────────────────

    [Test]
    public async Task UpdateBulletinApprovalAsync_SetsIsApproved_AndHandsExpectedVersionToRepo()
    {
        SetupScope(UserType.Staff, CurrentUserId, canView: true, canEdit: false, canApprove: true);
        RequirePermission(Permissions.School.ApproveSchoolBulletins);

        var bulletinId = Guid.NewGuid();
        var bulletin = MakeBulletin(bulletinId, version: 7, isApproved: false);

        _bulletinRepository.Setup(r => r.GetByIdAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);
        _accessPolicy.Setup(p => p.CanView(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _accessPolicy.Setup(p => p.CanEdit(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _bulletinRepository.Setup(r => r.UpdateAsync(bulletin, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);

        await _service.UpdateBulletinApprovalAsync(bulletinId, isApproved: true, expectedVersion: 7,
            CancellationToken.None);

        Assert.That(bulletin.IsApproved, Is.True);
        Assert.That(bulletin.Version, Is.EqualTo(7));
    }

    [Test]
    public void UpdateBulletinApprovalAsync_Throws_Forbidden_WhenAccessPolicyDeniesEdit()
    {
        SetupScope(UserType.Staff, CurrentUserId, canView: true, canEdit: false, canApprove: true);
        RequirePermission(Permissions.School.ApproveSchoolBulletins);

        var bulletinId = Guid.NewGuid();
        var bulletin = MakeBulletin(bulletinId);

        _bulletinRepository.Setup(r => r.GetByIdAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);
        _accessPolicy.Setup(p => p.CanView(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _accessPolicy.Setup(p => p.CanEdit(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(false);

        Assert.That(async () => await _service.UpdateBulletinApprovalAsync(bulletinId, true, 1,
                CancellationToken.None),
            Throws.TypeOf<ForbiddenException>());

        _bulletinRepository.Verify(r => r.UpdateAsync(It.IsAny<Bulletin>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    // ─── DeleteAsync ─────────────────────────────────────────────────

    [Test]
    public async Task DeleteAsync_DeletesBulletinAndItsDirectory()
    {
        RequirePermission(Permissions.School.EditSchoolBulletins);

        var bulletinId = Guid.NewGuid();
        var directoryId = Guid.NewGuid();
        var bulletin = MakeBulletin(bulletinId, directoryId);

        _bulletinRepository.Setup(r => r.GetByIdAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);
        _accessPolicy.Setup(p => p.CanView(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _accessPolicy.Setup(p => p.CanEdit(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _bulletinRepository.Setup(r => r.DeleteAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<bool>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(true);
        _directoryService.Setup(d => d.DeleteAsync(directoryId, It.IsAny<CancellationToken>(), It.IsAny<IUnitOfWork?>()))
            .Returns(Task.CompletedTask);

        await _service.DeleteAsync(bulletinId, CancellationToken.None);

        _bulletinRepository.Verify(r => r.DeleteAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<bool>(), It.IsAny<IDbTransaction?>()),
            Times.Once);
        _directoryService.Verify(d => d.DeleteAsync(directoryId, It.IsAny<CancellationToken>(), It.IsAny<IUnitOfWork?>()),
            Times.Once);
    }

    [Test]
    public void DeleteAsync_Throws_Forbidden_WhenAccessPolicyDeniesEdit()
    {
        RequirePermission(Permissions.School.EditSchoolBulletins);

        var bulletinId = Guid.NewGuid();
        var bulletin = MakeBulletin(bulletinId);

        _bulletinRepository.Setup(r => r.GetByIdAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);
        _accessPolicy.Setup(p => p.CanView(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _accessPolicy.Setup(p => p.CanEdit(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(false);

        Assert.That(async () => await _service.DeleteAsync(bulletinId, CancellationToken.None),
            Throws.TypeOf<ForbiddenException>());

        _bulletinRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>(),
            It.IsAny<CancellationToken>(), It.IsAny<bool>(), It.IsAny<IDbTransaction?>()), Times.Never);
        _directoryService.Verify(d => d.DeleteAsync(It.IsAny<Guid>(),
            It.IsAny<CancellationToken>(), It.IsAny<IUnitOfWork?>()), Times.Never);
    }
}
