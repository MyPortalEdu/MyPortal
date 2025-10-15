using Microsoft.AspNetCore.Identity;
using Moq;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.System.Permissions;
using MyPortal.Contracts.Models.System.Users;
using MyPortal.Services.Interfaces.Repositories;
using MyPortal.Services.Services;
using MyPortal.Tests.Mocks;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class UserServiceTests
{
    private Mock<IAuthorizationService> _authorizationService;
    private Mock<IUserRepository> _userRepository;
    private Mock<IPermissionRepository> _permissionRepository;
    private Mock<UserManager<ApplicationUser>> _userManager;
    private Mock<RoleManager<ApplicationRole>> _roleManager;

    private UserService _userService;

    [SetUp]
    public void Setup()
    {
        _authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        _userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        _permissionRepository = new Mock<IPermissionRepository>(MockBehavior.Strict);
        _userManager = IdentityMocks.MockUserManager<ApplicationUser>();
        _roleManager = IdentityMocks.MockRoleManager<ApplicationRole>();

        _authorizationService
            .Setup(a => a.RequirePermissionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userService = new UserService(
            _authorizationService.Object,
            _userRepository.Object,
            _permissionRepository.Object,
            _userManager.Object,
            _roleManager.Object
        );
    }

    [Test]
    public async Task GetDetailsByIdAsync_RequiresPermission_ThenReturnsUserDetails()
    {
        var id = Guid.NewGuid();
        var dto = new UserDetailsDto { Id = id };

        _userRepository.Setup(r => r.GetDetailsByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _userService.GetDetailsByIdAsync(id, CancellationToken.None);

        _authorizationService.Verify(
            a => a.RequirePermissionAsync(Permissions.System.ViewUsers, It.IsAny<CancellationToken>()),
            Times.Exactly(1));

        Assert.That(result, Is.EqualTo(dto));
    }

    [Test]
    public async Task GetInfoByIdAsync_SameUser_DoesNotRequirePermission_ReturnsUserInfo()
    {
        var id = Guid.NewGuid();

        _authorizationService.Setup(a => a.GetCurrentUserId()).Returns(id);

        var info = new UserInfoDto { Id = id, Permissions = [] };

        _userRepository.Setup(r => r.GetInfoByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(info);

        var perms = new[]
        {
            new PermissionDto { Name = "A", FriendlyName = "A", Area = "A" },
            new PermissionDto { Name = "B", FriendlyName = "B", Area = "B" }
        };

        _permissionRepository.Setup(p => p.GetPermissionsByUserIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(perms);

        var result = await _userService.GetInfoByIdAsync(id, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Permissions, Is.EquivalentTo(new[] { "A", "B" }));

        _authorizationService.Verify(
            a => a.RequirePermissionAsync(Permissions.System.ViewUsers, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetInfoByIdAsync_DifferentUser_RequiresPermission()
    {
        var requestedId = Guid.NewGuid();
        _authorizationService.Setup(a => a.GetCurrentUserId()).Returns(Guid.NewGuid());
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.System.ViewUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var info = new UserInfoDto { Id = requestedId, Permissions = [] };
        _userRepository.Setup(r => r.GetInfoByIdAsync(requestedId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(info);

        _permissionRepository.Setup(p => p.GetPermissionsByUserIdAsync(requestedId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PermissionDto>());

        var result = await _userService.GetInfoByIdAsync(requestedId, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        _authorizationService.Verify(
            a => a.RequirePermissionAsync(Permissions.System.ViewUsers, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetInfoByIdAsync_ReturnsNull_WhenUserMissing()
    {
        var id = Guid.NewGuid();
        _authorizationService.Setup(a => a.GetCurrentUserId()).Returns(id);

        _userRepository.Setup(r => r.GetInfoByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserInfoDto?)null);

        var result = await _userService.GetInfoByIdAsync(id, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void SetPasswordAsync_Throws_NotFound_WhenUserMissing()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.System.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var dto = new UserSetPasswordDto
        {
            NewPassword = "NewP@ssw0rd!"
        };

        Assert.That(async () => await _userService.SetPasswordAsync(Guid.NewGuid(), dto, CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("User not found."));
    }

    [Test]
    public async Task SetPasswordAsync_RemovesThenAddsPassword_AndReturnsResult()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.System.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var user = new ApplicationUser { Id = Guid.NewGuid() };

        _userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManager.Setup(m => m.RemovePasswordAsync(user)).ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddPasswordAsync(user, "X")).ReturnsAsync(IdentityResult.Success);

        var res = await _userService.SetPasswordAsync(user.Id, new UserSetPasswordDto
        {
            NewPassword = "X"
        }, CancellationToken.None);

        Assert.That(res.Succeeded, Is.True);
    }

    [Test]
    public void ChangePasswordAsync_Forbidden_WhenNotSelf()
    {
        var current = Guid.NewGuid();
        var target = Guid.NewGuid();
        _authorizationService.Setup(a => a.GetCurrentUserId()).Returns(current);

        var dto = new UserChangePasswordDto
        {
            CurrentPassword = "old",
            NewPassword = "new"
        };

        Assert.That(async () => await _userService.ChangePasswordAsync(target, dto, CancellationToken.None),
            Throws.TypeOf<ForbiddenException>().With.Message.EqualTo("You can only change your own password."));
    }

    [Test]
    public async Task ChangePasswordAsync_Ok_WhenSelf()
    {
        var id = Guid.NewGuid();
        _authorizationService.Setup(a => a.GetCurrentUserId()).Returns(id);

        var user = new ApplicationUser { Id = id };
        _userManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(user);
        _userManager.Setup(m => m.ChangePasswordAsync(user, "old", "new"))
            .ReturnsAsync(IdentityResult.Success);

        var res = await _userService.ChangePasswordAsync(id, new UserChangePasswordDto
        {
            CurrentPassword = "old",
            NewPassword = "new"
        }, CancellationToken.None);

        Assert.That(res.Succeeded, Is.True);
    }

    [Test]
    public async Task CreateUserAsync_CreatesUser_AndUpdatesRoles()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.System.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Admin" };

        _userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "pw"))
            .ReturnsAsync(IdentityResult.Success);
        _roleManager.Setup(r => r.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Admin"))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(Array.Empty<string>());
        _userManager.Setup(m => m.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _userService.CreateUserAsync(new UserUpsertDto
        {
            Username = "staff",
            Email = "staff@test.com",
            Password = "pw",
            IsEnabled = true,
            UserType = UserType.Staff,
            PersonId = Guid.NewGuid(),
            RoleIds = new List<Guid> { roleId }
        }, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public async Task UpdateUserAsync_UpdatesSecurityStamp_WhenUserDisabled()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.System.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var id = Guid.NewGuid();
        var user = new ApplicationUser { Id = id, IsEnabled = true };

        _userManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(user);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(Array.Empty<string>());
        _userManager.Setup(m => m.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);

        var res = await _userService.UpdateUserAsync(id, new UserUpsertDto
        {
            PersonId = Guid.NewGuid(),
            Username = "student",
            UserType = UserType.Student,
            IsEnabled = false, // turned off
            RoleIds = new List<Guid>() // no role changes
        }, CancellationToken.None);

        Assert.That(res.Succeeded, Is.True);
        _userManager.Verify(m => m.UpdateSecurityStampAsync(user), Times.Once);
        _userManager.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Test]
    public async Task UpdateUserAsync_UpdatesSecurityStamp_WhenRolesChanged()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.System.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var id = Guid.NewGuid();
        var user = new ApplicationUser { Id = id, IsEnabled = true };

        var newRoleId = Guid.NewGuid();
        var newRole = new ApplicationRole { Id = newRoleId, Name = "NewRole" };

        _userManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(user);

        // Existing roles contain "OldRole"; new desired roles contain "NewRole"
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new[] { "OldRole" });
        _roleManager.Setup(r => r.FindByIdAsync(newRoleId.ToString())).ReturnsAsync(newRole);
        _userManager.Setup(m => m.AddToRoleAsync(user, "NewRole")).ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.RemoveFromRoleAsync(user, "OldRole")).ReturnsAsync(IdentityResult.Success);

        _userManager.Setup(m => m.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);

        var res = await _userService.UpdateUserAsync(id, new UserUpsertDto
        {
            PersonId = Guid.NewGuid(),
            Username = "student",
            UserType = UserType.Student,
            IsEnabled = true,
            RoleIds = new List<Guid> { newRoleId }
        }, CancellationToken.None);

        Assert.That(res.Succeeded, Is.True);
        _userManager.Verify(m => m.UpdateSecurityStampAsync(user), Times.Once);
        _userManager.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Test]
    public async Task UpdateUserAsync_UpdatesUser_WhenNoStateChange()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.System.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var id = Guid.NewGuid();
        var user = new ApplicationUser { Id = id, IsEnabled = true };

        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Same" };

        _userManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(user);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new[] { "Same" });

        // Role is found but same as existing -> no add/remove expected
        _roleManager.Setup(r => r.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);

        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);

        var res = await _userService.UpdateUserAsync(id, new UserUpsertDto()
        {
            PersonId = Guid.NewGuid(),
            Username = "student",
            UserType = UserType.Student,
            IsEnabled = true, // unchanged
            RoleIds = new List<Guid> { roleId } // same set as before
        }, CancellationToken.None);

        Assert.That(res.Succeeded, Is.True);
        _userManager.Verify(m => m.UpdateAsync(user), Times.Once);
        _userManager.Verify(m => m.UpdateSecurityStampAsync(user), Times.Never);
        _userManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _userManager.Verify(m => m.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task UpdateUserAsync_SkipsAddingRole_WhenRoleNameEmpty()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.System.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var id = Guid.NewGuid();
        var user = new ApplicationUser { Id = id, IsEnabled = true };
        var roleId = Guid.NewGuid();

        var emptyRole = new ApplicationRole { Id = roleId, Name = "   " }; // whitespace

        _userManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(user);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(Array.Empty<string>());

        _roleManager.Setup(r => r.FindByIdAsync(roleId.ToString())).ReturnsAsync(emptyRole);

        // No add/remove expected; since no other changes, UpdateAsync should be called
        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var res = await _userService.UpdateUserAsync(id, new UserUpsertDto()
        {
            PersonId = Guid.NewGuid(),
            Username = "student",
            UserType = UserType.Student,
            IsEnabled = true,
            RoleIds = new List<Guid> { roleId }
        }, CancellationToken.None);

        Assert.That(res.Succeeded, Is.True);
        _userManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _userManager.Verify(m => m.UpdateAsync(user), Times.Once);
    }

    [Test]
    public void UpdateUserAsync_Throws_NotFound_WhenRoleMissing()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.System.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var id = Guid.NewGuid();
        var missingRoleId = Guid.NewGuid();

        var user = new ApplicationUser { Id = id, IsEnabled = true };

        _userManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(user);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(Array.Empty<string>());
        _roleManager.Setup(r => r.FindByIdAsync(missingRoleId.ToString()))
            .ReturnsAsync((ApplicationRole?)null);

        var dto = new UserUpsertDto()
        {
            Username = "student",
            PersonId = Guid.NewGuid(),
            UserType = UserType.Student,
            IsEnabled = true,
            RoleIds = new List<Guid> { missingRoleId }
        };

        Assert.That(async () => await _userService.UpdateUserAsync(id, dto, CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("Role not found."));
    }

    [Test]
    public void DeleteUserAsync_Throws_NotFound_WhenMissing()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.System.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        Assert.That(async () => await _userService.DeleteUserAsync(Guid.NewGuid(), CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("User not found."));
    }
}
