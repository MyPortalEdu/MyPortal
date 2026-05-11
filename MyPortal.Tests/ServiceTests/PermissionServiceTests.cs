using Microsoft.AspNetCore.Identity;
using Moq;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using MyPortal.Common.Enums;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Security;
using MyPortal.Tests.Mocks;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class PermissionServiceTests
{
    private Mock<ICurrentUser> _currentUser;
    private Mock<IRolePermissionProvider> _provider;
    private Mock<IPermissionRepository> _permissionRepository;
    private Mock<UserManager<ApplicationUser>> _userManager;
    private Mock<IUserStatusCache> _userStatusCache;

    private PermissionService _service;

    [SetUp]
    public void Setup()
    {
        _currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);
        _provider = new Mock<IRolePermissionProvider>(MockBehavior.Strict);
        _permissionRepository = new Mock<IPermissionRepository>(MockBehavior.Strict);
        _userManager = IdentityMocks.MockUserManager<ApplicationUser>();
        _userStatusCache = new Mock<IUserStatusCache>(MockBehavior.Strict);

        _service = new PermissionService(
            _currentUser.Object,
            _provider.Object,
            _permissionRepository.Object,
            _userManager.Object,
            _userStatusCache.Object
        );
    }

    [Test]
    public async Task HasPermissionAsync_ReturnsFalse_WhenUserIdMissing()
    {
        _currentUser.Setup(c => c.UserId).Returns((Guid?)null);

        var result = await _service.HasPermissionAsync("Foo");

        Assert.That(result, Is.False);
        // Short-circuit: nothing else should be consulted.
        _userStatusCache.Verify(c => c.IsEnabledAsync(It.IsAny<Guid>(),
            It.IsAny<Func<CancellationToken, Task<bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
        _provider.Verify(p => p.GetPermissionsForRolesAsync(It.IsAny<IEnumerable<Guid>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task HasPermissionAsync_ReturnsFalse_WhenUserDisabled()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(userId);
        _userStatusCache.Setup(c => c.IsEnabledAsync(userId,
                It.IsAny<Func<CancellationToken, Task<bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.HasPermissionAsync("System.ViewUsers");

        Assert.That(result, Is.False);
        // Disabled user must not be considered for role/permission lookup at all.
        _provider.Verify(p => p.GetPermissionsForRolesAsync(It.IsAny<IEnumerable<Guid>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task HasPermissionAsync_ReturnsTrue_WhenPermissionPresent()
    {
        var userId = Guid.NewGuid();
        var roleIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        _currentUser.Setup(c => c.UserId).Returns(userId);
        _userStatusCache.Setup(c => c.IsEnabledAsync(userId,
                It.IsAny<Func<CancellationToken, Task<bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _currentUser.Setup(c => c.GetRolesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(roleIds);
        _provider.Setup(p => p.GetPermissionsForRolesAsync(roleIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "School.ViewSchoolBulletins", "System.ViewUsers" });

        var result = await _service.HasPermissionAsync("System.ViewUsers");

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task HasPermissionAsync_PermissionMatchIsCaseInsensitive()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(userId);
        _userStatusCache.Setup(c => c.IsEnabledAsync(userId,
                It.IsAny<Func<CancellationToken, Task<bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _currentUser.Setup(c => c.GetRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>());
        _provider.Setup(p => p.GetPermissionsForRolesAsync(It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "system.viewusers" });

        var result = await _service.HasPermissionAsync("System.ViewUsers");

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task HasPermissionAsync_ReturnsFalse_WhenPermissionAbsent()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(userId);
        _userStatusCache.Setup(c => c.IsEnabledAsync(userId,
                It.IsAny<Func<CancellationToken, Task<bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _currentUser.Setup(c => c.GetRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { Guid.NewGuid() });
        _provider.Setup(p => p.GetPermissionsForRolesAsync(It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "School.ViewSchoolBulletins" });

        var result = await _service.HasPermissionAsync("System.ViewUsers");

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HasPermissionAsync_FactoryReturnsFalse_WhenUserManagerHasNoUser()
    {
        // Drives the real IUserStatusCache factory: when FindByIdAsync returns null we should
        // treat the user as disabled (fail closed) rather than passing through.
        var userId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(userId);

        Func<CancellationToken, Task<bool>>? capturedFactory = null;
        _userStatusCache.Setup(c => c.IsEnabledAsync(userId,
                It.IsAny<Func<CancellationToken, Task<bool>>>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, Func<CancellationToken, Task<bool>>, CancellationToken>(
                (_, factory, _) => capturedFactory = factory)
            .ReturnsAsync(false);

        _userManager.Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await _service.HasPermissionAsync("Anything");

        Assert.That(result, Is.False);
        Assert.That(capturedFactory, Is.Not.Null);
        var factoryResult = await capturedFactory!(CancellationToken.None);
        Assert.That(factoryResult, Is.False, "Factory must report disabled when the user does not exist.");
    }

    [Test]
    public async Task HasPermissionAsync_FactoryHonoursIsEnabledFlag()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(userId);

        Func<CancellationToken, Task<bool>>? capturedFactory = null;
        _userStatusCache.Setup(c => c.IsEnabledAsync(userId,
                It.IsAny<Func<CancellationToken, Task<bool>>>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, Func<CancellationToken, Task<bool>>, CancellationToken>(
                (_, factory, _) => capturedFactory = factory)
            .ReturnsAsync(true);
        _currentUser.Setup(c => c.GetRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Guid>());
        _provider.Setup(p => p.GetPermissionsForRolesAsync(It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<string>());

        _userManager.Setup(m => m.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(new ApplicationUser { Id = userId, IsEnabled = true, UserType = UserType.Staff });

        await _service.HasPermissionAsync("Anything");

        Assert.That(capturedFactory, Is.Not.Null);
        Assert.That(await capturedFactory!(CancellationToken.None), Is.True);
    }
}
