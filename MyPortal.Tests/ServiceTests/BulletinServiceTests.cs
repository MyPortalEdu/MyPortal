using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Constants;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.VisibilityScopes;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Documents;
using MyPortal.Services.Interfaces.Security;
using MyPortal.Services.School.Bulletins;
using QueryKit.Repositories.Exceptions;
using IAuthorizationService = MyPortal.Auth.Interfaces.IAuthorizationService;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class BulletinServiceTests
{
    private static readonly Guid CurrentUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private Mock<IAuthorizationService> _authorizationService = null!;
    private Mock<ILogger<BulletinService>> _logger = null!;
    private Mock<IDirectoryService> _directoryService = null!;
    private Mock<IDocumentService> _documentService = null!;
    private Mock<IValidationService> _validationService = null!;
    private Mock<IBulletinRepository> _bulletinRepository = null!;
    private Mock<IBulletinAcknowledgementRepository> _ackRepository = null!;
    private Mock<IAccessPolicy<Bulletin, BulletinVisibilityScope>> _accessPolicy = null!;
    private Mock<IUnitOfWorkFactory> _unitOfWorkFactory = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private Mock<IDbTransaction> _transaction = null!;

    private BulletinService _service = null!;

    [SetUp]
    public void Setup()
    {
        _authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        _logger = new Mock<ILogger<BulletinService>>(MockBehavior.Loose);
        _directoryService = new Mock<IDirectoryService>(MockBehavior.Strict);
        _documentService = new Mock<IDocumentService>(MockBehavior.Strict);
        _validationService = new Mock<IValidationService>(MockBehavior.Strict);
        _bulletinRepository = new Mock<IBulletinRepository>(MockBehavior.Strict);
        _ackRepository = new Mock<IBulletinAcknowledgementRepository>(MockBehavior.Strict);
        _accessPolicy = new Mock<IAccessPolicy<Bulletin, BulletinVisibilityScope>>(MockBehavior.Strict);
        _unitOfWorkFactory = new Mock<IUnitOfWorkFactory>(MockBehavior.Strict);
        _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Loose);
        _transaction = new Mock<IDbTransaction>(MockBehavior.Loose);

        _unitOfWork.SetupGet(u => u.Transaction).Returns(_transaction.Object);
        _unitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.DisposeAsync()).Returns(ValueTask.CompletedTask);
        _unitOfWorkFactory.Setup(f => f.BeginAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWork.Object);

        // Default scope: staff editor with edit permission, no pin. Tests can re-stub these.
        SetupScope(UserType.Staff, CurrentUserId, canView: true, canEdit: true, canPin: false);

        _service = new BulletinService(
            _authorizationService.Object,
            _logger.Object,
            _directoryService.Object,
            _documentService.Object,
            _validationService.Object,
            _bulletinRepository.Object,
            _ackRepository.Object,
            _accessPolicy.Object,
            _unitOfWorkFactory.Object
        );
    }

    private void SetupScope(UserType userType, Guid? userId, bool canView, bool canEdit, bool canPin)
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
                a.HasPermissionAsync(Permissions.School.PinSchoolBulletins, It.IsAny<CancellationToken>()))
            .ReturnsAsync(canPin);
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
        DateTime? pinnedAt = null, Guid? createdById = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            DirectoryId = directoryId ?? Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Title = "Test",
            Detail = "Test detail",
            PinnedAt = pinnedAt,
            CreatedById = createdById ?? CurrentUserId,
            CreatedByIpAddress = "::1",
            LastModifiedById = createdById ?? CurrentUserId,
            LastModifiedByIpAddress = "::1",
            Version = version
        };

    private static BulletinUpsertRequest MakeUpsertRequest(string title = "Hello", string detail = "World",
        bool isPinned = false, bool requiresAck = false, long expectedVersion = 1) =>
        new()
        {
            Title = title,
            Detail = detail,
            CategoryId = Guid.NewGuid(),
            IsPinned = isPinned,
            RequiresAcknowledgement = requiresAck,
            ExpectedVersion = expectedVersion,
            Audiences = new List<BulletinAudienceRequest>
            {
                new() { AudienceKind = BulletinAudienceKind.AllStaff }
            }
        };

    // ─── GetDetailsByIdAsync ─────────────────────────────────────────────────

    [Test]
    public async Task GetDetailsByIdAsync_ReturnsDetails_WhenSpReturnsRow()
    {
        var bulletinId = Guid.NewGuid();
        var dto = new BulletinDetailsResponse { Id = bulletinId, Title = "Test" };

        _bulletinRepository.Setup(r => r.GetDetailsByIdAsync(bulletinId,
                It.IsAny<BulletinVisibilityScope>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _service.GetDetailsByIdAsync(bulletinId, CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
    }

    [Test]
    public void GetDetailsByIdAsync_Throws_NotFound_WhenSpReturnsNull()
    {
        // The SP returns no header for both genuinely-missing ids AND audience-mismatch
        // hits; both surface as NotFound (avoids leaking existence to non-audience users).
        var bulletinId = Guid.NewGuid();
        _bulletinRepository.Setup(r => r.GetDetailsByIdAsync(bulletinId,
                It.IsAny<BulletinVisibilityScope>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BulletinDetailsResponse?)null);

        Assert.That(async () => await _service.GetDetailsByIdAsync(bulletinId, CancellationToken.None),
            Throws.TypeOf<NotFoundException>());
    }

    // ─── CreateAsync ─────────────────────────────────────────────────────────

    [Test]
    public async Task CreateAsync_RequiresEdit_CreatesDirectoryBulletinAndAudiences()
    {
        RequirePermission(Permissions.School.EditSchoolBulletins);

        var directoryId = Guid.NewGuid();
        _directoryService.Setup(d => d.CreateAsync(It.IsAny<DirectoryUpsertRequest>(),
                It.IsAny<CancellationToken>(), It.IsAny<IUnitOfWork?>()))
            .ReturnsAsync(new DirectoryDetailsResponse { Id = directoryId, Name = "bulletin-x" });
        _bulletinRepository.Setup(r => r.InsertAsync(It.IsAny<Bulletin>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((Bulletin b, CancellationToken _, IDbTransaction? _) => b);
        _bulletinRepository.Setup(r => r.ReplaceAudiencesAsync(It.IsAny<Guid>(), It.IsAny<IList<BulletinAudience>>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .Returns(Task.CompletedTask);

        var resultId = await _service.CreateAsync(MakeUpsertRequest(), CancellationToken.None);

        Assert.That(resultId, Is.Not.EqualTo(Guid.Empty));
        _bulletinRepository.Verify(r => r.InsertAsync(
            It.Is<Bulletin>(b => b.Id == resultId && b.DirectoryId == directoryId && b.PinnedAt == null),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Once);
        _bulletinRepository.Verify(r => r.ReplaceAudiencesAsync(resultId,
            It.Is<IList<BulletinAudience>>(list => list.Count == 1),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Once);
    }

    [Test]
    public async Task CreateAsync_WithIsPinned_AlsoRequiresPinPermission_AndSetsPinnedAt()
    {
        RequirePermission(Permissions.School.EditSchoolBulletins);
        RequirePermission(Permissions.School.PinSchoolBulletins);

        var directoryId = Guid.NewGuid();
        _directoryService.Setup(d => d.CreateAsync(It.IsAny<DirectoryUpsertRequest>(),
                It.IsAny<CancellationToken>(), It.IsAny<IUnitOfWork?>()))
            .ReturnsAsync(new DirectoryDetailsResponse { Id = directoryId, Name = "bulletin-x" });
        _bulletinRepository.Setup(r => r.InsertAsync(It.IsAny<Bulletin>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((Bulletin b, CancellationToken _, IDbTransaction? _) => b);
        _bulletinRepository.Setup(r => r.ReplaceAudiencesAsync(It.IsAny<Guid>(), It.IsAny<IList<BulletinAudience>>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .Returns(Task.CompletedTask);

        await _service.CreateAsync(MakeUpsertRequest(isPinned: true), CancellationToken.None);

        _authorizationService.Verify(a =>
            a.RequirePermissionAsync(Permissions.School.PinSchoolBulletins, It.IsAny<CancellationToken>()),
            Times.Once);
        _bulletinRepository.Verify(r => r.InsertAsync(
            It.Is<Bulletin>(b => b.PinnedAt != null),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Once);
    }

    [Test]
    public void CreateAsync_RejectsEmptyAudiences()
    {
        RequirePermission(Permissions.School.EditSchoolBulletins);

        var model = MakeUpsertRequest();
        model.Audiences.Clear();

        Assert.That(async () => await _service.CreateAsync(model, CancellationToken.None),
            Throws.TypeOf<InvalidOperationException>());

        _directoryService.Verify(d => d.CreateAsync(It.IsAny<DirectoryUpsertRequest>(),
            It.IsAny<CancellationToken>(), It.IsAny<IUnitOfWork?>()), Times.Never);
    }

    [Test]
    public void CreateAsync_PropagatesPermissionDenial()
    {
        RequirePermission(Permissions.School.EditSchoolBulletins, succeeds: false);

        Assert.That(async () => await _service.CreateAsync(MakeUpsertRequest(), CancellationToken.None),
            Throws.TypeOf<ForbiddenException>());

        _bulletinRepository.Verify(r => r.InsertAsync(It.IsAny<Bulletin>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    // ─── UpdateAsync ─────────────────────────────────────────────────────────

    [Test]
    public async Task UpdateAsync_AppliesFields_AndPersistsAudiences_AndHandsExpectedVersionToRepo()
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
        _bulletinRepository.Setup(r => r.ReplaceAudiencesAsync(bulletinId, It.IsAny<IList<BulletinAudience>>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .Returns(Task.CompletedTask);

        var model = MakeUpsertRequest(title: "Updated", detail: "Updated detail", expectedVersion: 5);

        await _service.UpdateAsync(bulletinId, model, CancellationToken.None);

        Assert.That(bulletin.Title,   Is.EqualTo("Updated"));
        Assert.That(bulletin.Detail,  Is.EqualTo("Updated detail"));
        Assert.That(bulletin.Version, Is.EqualTo(5));
        _bulletinRepository.Verify(r => r.ReplaceAudiencesAsync(bulletinId, It.IsAny<IList<BulletinAudience>>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_ChangingPinState_AlsoRequiresPinPermission()
    {
        // Default scope has canPin=false; the editor is trying to pin a previously-unpinned
        // bulletin without the pin permission, which must fail-fast on RequirePermissionAsync.
        RequirePermission(Permissions.School.EditSchoolBulletins);
        RequirePermission(Permissions.School.PinSchoolBulletins, succeeds: false);

        var bulletinId = Guid.NewGuid();
        var bulletin = MakeBulletin(bulletinId, pinnedAt: null);

        _bulletinRepository.Setup(r => r.GetByIdAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);
        _accessPolicy.Setup(p => p.CanView(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _accessPolicy.Setup(p => p.CanEdit(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);

        var model = MakeUpsertRequest(isPinned: true);

        Assert.That(async () => await _service.UpdateAsync(bulletinId, model, CancellationToken.None),
            Throws.TypeOf<ForbiddenException>());

        _bulletinRepository.Verify(r => r.UpdateAsync(It.IsAny<Bulletin>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
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

        Assert.That(async () => await _service.UpdateAsync(bulletinId, MakeUpsertRequest(), CancellationToken.None),
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

        Assert.That(async () => await _service.UpdateAsync(bulletinId,
                MakeUpsertRequest(expectedVersion: 4), CancellationToken.None),
            Throws.TypeOf<ConcurrencyException>());
    }

    // ─── UpdatePinAsync ──────────────────────────────────────────────────────

    [Test]
    public async Task UpdatePinAsync_SetsPinnedAt_AndHandsExpectedVersionToRepo()
    {
        SetupScope(UserType.Staff, CurrentUserId, canView: true, canEdit: false, canPin: true);
        RequirePermission(Permissions.School.PinSchoolBulletins);

        var bulletinId = Guid.NewGuid();
        var bulletin = MakeBulletin(bulletinId, version: 7, pinnedAt: null);

        _bulletinRepository.Setup(r => r.GetByIdAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);
        _accessPolicy.Setup(p => p.CanView(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _bulletinRepository.Setup(r => r.UpdateAsync(bulletin, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);

        await _service.UpdatePinAsync(bulletinId, isPinned: true, expectedVersion: 7, CancellationToken.None);

        Assert.That(bulletin.PinnedAt, Is.Not.Null);
        Assert.That(bulletin.Version,  Is.EqualTo(7));
    }

    [Test]
    public async Task UpdatePinAsync_Unpin_ClearsPinnedAt()
    {
        SetupScope(UserType.Staff, CurrentUserId, canView: true, canEdit: false, canPin: true);
        RequirePermission(Permissions.School.PinSchoolBulletins);

        var bulletinId = Guid.NewGuid();
        var bulletin = MakeBulletin(bulletinId, version: 3, pinnedAt: DateTime.UtcNow);

        _bulletinRepository.Setup(r => r.GetByIdAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);
        _accessPolicy.Setup(p => p.CanView(bulletin, It.IsAny<BulletinVisibilityScope>())).Returns(true);
        _bulletinRepository.Setup(r => r.UpdateAsync(bulletin, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(bulletin);

        await _service.UpdatePinAsync(bulletinId, isPinned: false, expectedVersion: 3, CancellationToken.None);

        Assert.That(bulletin.PinnedAt, Is.Null);
    }

    // ─── AcknowledgeAsync ────────────────────────────────────────────────────

    [Test]
    public async Task AcknowledgeAsync_RecordsAck_WhenBulletinRequiresIt_AndIsVisible()
    {
        var bulletinId = Guid.NewGuid();
        var details = new BulletinDetailsResponse { Id = bulletinId, Title = "T", RequiresAcknowledgement = true };

        _bulletinRepository.Setup(r => r.GetDetailsByIdAsync(bulletinId,
                It.IsAny<BulletinVisibilityScope>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(details);
        _ackRepository.Setup(r => r.AcknowledgeAsync(bulletinId, CurrentUserId,
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(true);

        await _service.AcknowledgeAsync(bulletinId, CancellationToken.None);

        _ackRepository.Verify(r => r.AcknowledgeAsync(bulletinId, CurrentUserId,
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Once);
    }

    [Test]
    public void AcknowledgeAsync_Throws_NotFound_WhenBulletinNotVisible()
    {
        var bulletinId = Guid.NewGuid();
        _bulletinRepository.Setup(r => r.GetDetailsByIdAsync(bulletinId,
                It.IsAny<BulletinVisibilityScope>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BulletinDetailsResponse?)null);

        Assert.That(async () => await _service.AcknowledgeAsync(bulletinId, CancellationToken.None),
            Throws.TypeOf<NotFoundException>());

        _ackRepository.Verify(r => r.AcknowledgeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    [Test]
    public void AcknowledgeAsync_Throws_WhenBulletinDoesNotRequireAck()
    {
        var bulletinId = Guid.NewGuid();
        var details = new BulletinDetailsResponse { Id = bulletinId, Title = "T", RequiresAcknowledgement = false };

        _bulletinRepository.Setup(r => r.GetDetailsByIdAsync(bulletinId,
                It.IsAny<BulletinVisibilityScope>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(details);

        Assert.That(async () => await _service.AcknowledgeAsync(bulletinId, CancellationToken.None),
            Throws.TypeOf<InvalidOperationException>());

        _ackRepository.Verify(r => r.AcknowledgeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    // ─── DeleteAsync ─────────────────────────────────────────────────────────

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
        _directoryService.Setup(d => d.DeleteAsync(directoryId, It.IsAny<CancellationToken>(), It.IsAny<IUnitOfWork?>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        await _service.DeleteAsync(bulletinId, CancellationToken.None);

        _bulletinRepository.Verify(r => r.DeleteAsync(bulletinId, It.IsAny<CancellationToken>(), It.IsAny<bool>(), It.IsAny<IDbTransaction?>()),
            Times.Once);
        _directoryService.Verify(d => d.DeleteAsync(directoryId, It.IsAny<CancellationToken>(), It.IsAny<IUnitOfWork?>(), It.IsAny<bool>()),
            Times.Once);
    }
}
