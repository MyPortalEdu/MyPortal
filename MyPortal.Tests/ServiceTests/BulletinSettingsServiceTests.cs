using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Constants;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Data.Interfaces;
using MyPortal.Services.School.Bulletins;
using IAuthorizationService = MyPortal.Auth.Interfaces.IAuthorizationService;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class BulletinSettingsServiceTests
{
    private Mock<IAuthorizationService> _authorizationService = null!;
    private Mock<IBulletinSettingsRepository> _repository = null!;
    private Mock<ILogger<BulletinSettingsService>> _logger = null!;

    private BulletinSettingsService _service = null!;

    [SetUp]
    public void Setup()
    {
        _authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        _repository = new Mock<IBulletinSettingsRepository>(MockBehavior.Strict);
        _logger = new Mock<ILogger<BulletinSettingsService>>(MockBehavior.Loose);

        _service = new BulletinSettingsService(
            _authorizationService.Object,
            _repository.Object,
            _logger.Object);
    }

    // ─── GetAsync ───────────────────────────────────────────────────────────

    [Test]
    public async Task GetAsync_RequiresViewBulletinsPermission_AndReturnsAllowedGroups()
    {
        var groups = new List<BulletinAllowedGroupResponse>
        {
            new() { StudentGroupId = Guid.NewGuid(), Code = "7A", Name = "7A" },
            new() { StudentGroupId = Guid.NewGuid(), Code = "8B", Name = "8B" }
        };

        _authorizationService.Setup(a =>
                a.RequirePermissionAsync(Permissions.School.ViewSchoolBulletins, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repository.Setup(r => r.GetAllowedAudienceGroupsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(groups);

        var result = await _service.GetAsync(CancellationToken.None);

        Assert.That(result.AllowedAudienceGroups, Is.SameAs(groups));
        _authorizationService.Verify(a =>
            a.RequirePermissionAsync(Permissions.School.ViewSchoolBulletins, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void GetAsync_PropagatesPermissionDenial()
    {
        _authorizationService.Setup(a =>
                a.RequirePermissionAsync(Permissions.School.ViewSchoolBulletins, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenException("missing permission"));

        Assert.That(async () => await _service.GetAsync(CancellationToken.None),
            Throws.TypeOf<ForbiddenException>());

        _repository.Verify(r => r.GetAllowedAudienceGroupsAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ─── UpdateAsync ────────────────────────────────────────────────────────

    [Test]
    public async Task UpdateAsync_RequiresBulletinSettingsPermission_AndPassesIdsToRepository()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var model = new BulletinSettingsUpdateRequest { AllowedAudienceGroupIds = ids };

        _authorizationService.Setup(a =>
                a.RequirePermissionAsync(Permissions.SystemAdmin.BulletinSettings, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repository.Setup(r => r.ReplaceAllowedAudienceGroupsAsync(ids,
                It.IsAny<CancellationToken>(), It.IsAny<System.Data.IDbTransaction?>()))
            .Returns(Task.CompletedTask);

        await _service.UpdateAsync(model, CancellationToken.None);

        _repository.Verify(r => r.ReplaceAllowedAudienceGroupsAsync(ids,
            It.IsAny<CancellationToken>(), It.IsAny<System.Data.IDbTransaction?>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_AllowsEmptyList()
    {
        // Clearing the allowlist is a legitimate operation (revoke all groups). The service
        // shouldn't second-guess an empty list — the repo handles the empty-INSERT case.
        var model = new BulletinSettingsUpdateRequest { AllowedAudienceGroupIds = new List<Guid>() };

        _authorizationService.Setup(a =>
                a.RequirePermissionAsync(Permissions.SystemAdmin.BulletinSettings, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repository.Setup(r => r.ReplaceAllowedAudienceGroupsAsync(It.IsAny<IList<Guid>>(),
                It.IsAny<CancellationToken>(), It.IsAny<System.Data.IDbTransaction?>()))
            .Returns(Task.CompletedTask);

        await _service.UpdateAsync(model, CancellationToken.None);

        _repository.Verify(r => r.ReplaceAllowedAudienceGroupsAsync(
            It.Is<IList<Guid>>(l => l.Count == 0),
            It.IsAny<CancellationToken>(), It.IsAny<System.Data.IDbTransaction?>()), Times.Once);
    }

    [Test]
    public void UpdateAsync_PropagatesPermissionDenial()
    {
        var model = new BulletinSettingsUpdateRequest { AllowedAudienceGroupIds = new List<Guid> { Guid.NewGuid() } };

        _authorizationService.Setup(a =>
                a.RequirePermissionAsync(Permissions.SystemAdmin.BulletinSettings, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenException("missing permission"));

        Assert.That(async () => await _service.UpdateAsync(model, CancellationToken.None),
            Throws.TypeOf<ForbiddenException>());

        _repository.Verify(r => r.ReplaceAllowedAudienceGroupsAsync(It.IsAny<IList<Guid>>(),
            It.IsAny<CancellationToken>(), It.IsAny<System.Data.IDbTransaction?>()), Times.Never);
    }
}
