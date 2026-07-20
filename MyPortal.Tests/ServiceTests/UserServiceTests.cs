using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using MyPortal.Common.Constants;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.System.Permissions;
using MyPortal.Contracts.Models.System.Users;
using MyPortal.Services.Interfaces;
using MyPortal.Services.System;
using MyPortal.Tests.Mocks;
using Task = System.Threading.Tasks.Task;
using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class UserServiceTests
{
    private Mock<IAuthorizationService> _authorizationService;
    private Mock<ILogger<UserService>> _logger;
    private Mock<IUserRepository> _userRepository;
    private Mock<IPermissionRepository> _permissionRepository;
    private Mock<IRolePermissionRepository> _rolePermissionRepository;
    private Mock<UserManager<ApplicationUser>> _userManager;
    private Mock<RoleManager<ApplicationRole>> _roleManager;
    private Mock<IValidationService> _validationService;
    private Mock<IUserStatusCache> _userStatusCache;

    private UserService _userService;

    [SetUp]
    public void Setup()
    {
        _authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        _logger = new Mock<ILogger<UserService>>(MockBehavior.Strict);
        _userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        _permissionRepository = new Mock<IPermissionRepository>(MockBehavior.Strict);
        _rolePermissionRepository = new Mock<IRolePermissionRepository>(MockBehavior.Strict);
        _userManager = IdentityMocks.MockUserManager<ApplicationUser>();
        _roleManager = IdentityMocks.MockRoleManager<ApplicationRole>();
        _validationService = new Mock<IValidationService>(MockBehavior.Strict);
        _userStatusCache = new Mock<IUserStatusCache>(MockBehavior.Loose);

        _authorizationService
            .Setup(a => a.RequirePermissionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _authorizationService
            .Setup(a => a.GetCurrentUserId())
            .Returns(Guid.NewGuid());

        _authorizationService
            .Setup(a => a.GetCurrentUserIpAddress())
            .Returns("::1");

        _validationService
            .Setup(v => v.ValidateAsync(It.IsAny<UserUpsertRequest>()))
            .Returns(Task.CompletedTask);

        _validationService
            .Setup(v => v.ValidateAsync(It.IsAny<UserUpdateRequest>()))
            .Returns(Task.CompletedTask);

        // Defaults for the privilege-guard paths. The actor holds no permissions and there is no admin
        // role by default, so the ceiling/outrank/last-admin guards are no-ops. Tests that exercise a
        // specific guard override the relevant setup.
        _authorizationService
            .Setup(a => a.GetPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlySet<string>)new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        _permissionRepository
            .Setup(p => p.GetListAsync(It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<Permission>());

        _permissionRepository
            .Setup(p => p.GetPermissionsByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PermissionResponse>());

        _rolePermissionRepository
            .Setup(r => r.GetByRoleIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePermission>());

        _roleManager
            .Setup(r => r.FindByNameAsync(SystemRoles.SystemAdministratorRoleName))
            .ReturnsAsync((ApplicationRole?)null);

        _userService = new UserService(
            _authorizationService.Object,
            _logger.Object,
            _userRepository.Object,
            _permissionRepository.Object,
            _rolePermissionRepository.Object,
            _userManager.Object,
            _roleManager.Object,
            _validationService.Object,
            _userStatusCache.Object
        );
    }

    [Test]
    public async Task GetDetailsByIdAsync_RequiresPermission_ThenReturnsUserDetails()
    {
        var id = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var dto = new UserDetailsResponse { Id = id };

        _userRepository.Setup(r => r.GetDetailsByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);
        _userRepository.Setup(r => r.GetRoleIdsByUserIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid> { roleId });

        var result = await _userService.GetDetailsByIdAsync(id, CancellationToken.None);

        _authorizationService.Verify(
            a => a.RequirePermissionAsync(Permissions.SystemAdmin.ViewUsers, It.IsAny<CancellationToken>()),
            Times.Exactly(1));

        Assert.That(result, Is.EqualTo(dto));
    }

    [Test]
    public async Task GetInfoByIdAsync_SameUser_DoesNotRequirePermission_ReturnsUserInfo()
    {
        var id = Guid.NewGuid();

        _authorizationService.Setup(a => a.GetCurrentUserId()).Returns(id);

        var info = new UserInfoResponse { Id = id, Permissions = [] };

        _userRepository.Setup(r => r.GetInfoByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(info);

        var perms = new[]
        {
            new PermissionResponse { Name = "A", FriendlyName = "A", Area = "A" },
            new PermissionResponse { Name = "B", FriendlyName = "B", Area = "B" }
        };

        _permissionRepository.Setup(p => p.GetPermissionsByUserIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(perms);

        var result = await _userService.GetInfoByIdAsync(id, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Permissions, Is.EquivalentTo(new[] { "A", "B" }));

        _authorizationService.Verify(
            a => a.RequirePermissionAsync(Permissions.SystemAdmin.ViewUsers, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetInfoByIdAsync_DifferentUser_RequiresPermission()
    {
        var requestedId = Guid.NewGuid();
        _authorizationService.Setup(a => a.GetCurrentUserId()).Returns(Guid.NewGuid());
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.SystemAdmin.ViewUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var info = new UserInfoResponse { Id = requestedId, Permissions = [] };
        _userRepository.Setup(r => r.GetInfoByIdAsync(requestedId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(info);

        _permissionRepository.Setup(p => p.GetPermissionsByUserIdAsync(requestedId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PermissionResponse>());

        var result = await _userService.GetInfoByIdAsync(requestedId, CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        _authorizationService.Verify(
            a => a.RequirePermissionAsync(Permissions.SystemAdmin.ViewUsers, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetInfoByIdAsync_ReturnsNull_WhenUserMissing()
    {
        var id = Guid.NewGuid();
        _authorizationService.Setup(a => a.GetCurrentUserId()).Returns(id);

        _userRepository.Setup(r => r.GetInfoByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserInfoResponse?)null);

        var result = await _userService.GetInfoByIdAsync(id, CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void SetPasswordAsync_Throws_NotFound_WhenUserMissing()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        var dto = new UserSetPasswordRequest
        {
            Password = "NewP@ssw0rd!"
        };

        Assert.That(async () => await _userService.SetPasswordAsync(Guid.NewGuid(), dto, CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("User not found."));
    }

    [Test]
    public async Task SetPasswordAsync_ResetsPassword_AndReturnsResult()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var user = new ApplicationUser { Id = Guid.NewGuid() };

        _userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManager.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
        _userManager.Setup(m => m.ResetPasswordAsync(user, "reset-token", "X")).ReturnsAsync(IdentityResult.Success);

        var res = await _userService.SetPasswordAsync(user.Id, new UserSetPasswordRequest
        {
            Password = "X"
        }, CancellationToken.None);

        Assert.That(res.Succeeded, Is.True);
    }

    [Test]
    public void ChangePasswordAsync_Forbidden_WhenNotSelf()
    {
        var current = Guid.NewGuid();
        var target = Guid.NewGuid();
        _authorizationService.Setup(a => a.GetCurrentUserId()).Returns(current);

        var dto = new UserChangePasswordRequest
        {
            CurrentPassword = "old",
            Password = "new"
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

        var res = await _userService.ChangePasswordAsync(id, new UserChangePasswordRequest
        {
            CurrentPassword = "old",
            Password = "new"
        }, CancellationToken.None);

        Assert.That(res.Succeeded, Is.True);
    }

    [Test]
    public async Task CreateAsync_CreatesUser_AndUpdatesRoles()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Admin", UserType = UserType.Staff };

        _userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "pw"))
            .ReturnsAsync(IdentityResult.Success);
        _roleManager.Setup(r => r.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Admin"))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(Array.Empty<string>());
        _userManager.Setup(m => m.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _userService.CreateAsync(new UserUpsertRequest
        {
            Username = "staff",
            Email = "staff@test.com",
            Password = "pw",
            IsEnabled = true,
            UserType = UserType.Staff,
            PersonId = Guid.NewGuid(),
            RoleIds = new List<Guid> { roleId }
        }, CancellationToken.None);

        Assert.That(result.Result.Succeeded, Is.True);
    }

    [Test]
    public async Task UpdateAsync_UpdatesSecurityStamp_WhenUserDisabled()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var id = Guid.NewGuid();
        var user = new ApplicationUser { Id = id, IsEnabled = true, UserType = UserType.Staff };

        _userManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(user);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(Array.Empty<string>());
        _userManager.Setup(m => m.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);

        var res = await _userService.UpdateAsync(id, new UserUpdateRequest
        {
            PersonId = Guid.NewGuid(),
            Username = "student",
            UserType = UserType.Staff,
            IsEnabled = false, // turned off
            RoleIds = new List<Guid>() // no role changes
        }, CancellationToken.None);

        Assert.That(res.Succeeded, Is.True);
        _userManager.Verify(m => m.UpdateSecurityStampAsync(user), Times.Once);
        _userManager.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_UpdatesSecurityStamp_WhenRolesChanged()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var id = Guid.NewGuid();
        var user = new ApplicationUser { Id = id, IsEnabled = true, UserType = UserType.Staff };

        var newRoleId = Guid.NewGuid();
        var newRole = new ApplicationRole { Id = newRoleId, Name = "NewRole", UserType = UserType.Staff };

        _userManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(user);

        // Existing roles contain "OldRole"; new desired roles contain "NewRole"
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new[] { "OldRole" });
        _roleManager.Setup(r => r.FindByIdAsync(newRoleId.ToString())).ReturnsAsync(newRole);
        _userManager.Setup(m => m.AddToRoleAsync(user, "NewRole")).ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.RemoveFromRoleAsync(user, "OldRole")).ReturnsAsync(IdentityResult.Success);

        _userManager.Setup(m => m.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);

        var res = await _userService.UpdateAsync(id, new UserUpdateRequest
        {
            PersonId = Guid.NewGuid(),
            Username = "student",
            UserType = UserType.Staff,
            IsEnabled = true,
            RoleIds = new List<Guid> { newRoleId }
        }, CancellationToken.None);

        Assert.That(res.Succeeded, Is.True);
        _userManager.Verify(m => m.UpdateSecurityStampAsync(user), Times.Once);
        _userManager.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_UpdatesUser_WhenNoStateChange()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var id = Guid.NewGuid();
        var user = new ApplicationUser { Id = id, IsEnabled = true, UserType = UserType.Staff };

        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Same", UserType = UserType.Staff };

        _userManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(user);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new[] { "Same" });

        // Role is found but same as existing -> no add/remove expected
        _roleManager.Setup(r => r.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);

        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);

        var res = await _userService.UpdateAsync(id, new UserUpdateRequest()
        {
            PersonId = Guid.NewGuid(),
            Username = "student",
            UserType = UserType.Staff, // unchanged
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
    public async Task UpdateAsync_SkipsAddingRole_WhenRoleNameEmpty()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var id = Guid.NewGuid();
        var user = new ApplicationUser { Id = id, IsEnabled = true, UserType = UserType.Staff };
        var roleId = Guid.NewGuid();

        var emptyRole = new ApplicationRole { Id = roleId, Name = "   ", UserType = UserType.Staff }; // whitespace

        _userManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(user);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(Array.Empty<string>());

        _roleManager.Setup(r => r.FindByIdAsync(roleId.ToString())).ReturnsAsync(emptyRole);

        // No add/remove expected; since no other changes, UpdateAsync should be called
        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var res = await _userService.UpdateAsync(id, new UserUpdateRequest()
        {
            PersonId = Guid.NewGuid(),
            Username = "student",
            UserType = UserType.Staff,
            IsEnabled = true,
            RoleIds = new List<Guid> { roleId }
        }, CancellationToken.None);

        Assert.That(res.Succeeded, Is.True);
        _userManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _userManager.Verify(m => m.UpdateAsync(user), Times.Once);
    }

    [Test]
    public void UpdateAsync_Throws_NotFound_WhenRoleMissing()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var id = Guid.NewGuid();
        var missingRoleId = Guid.NewGuid();

        var user = new ApplicationUser { Id = id, IsEnabled = true, UserType = UserType.Staff };

        _userManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(user);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(Array.Empty<string>());
        _roleManager.Setup(r => r.FindByIdAsync(missingRoleId.ToString()))
            .ReturnsAsync((ApplicationRole?)null);

        var dto = new UserUpdateRequest()
        {
            Username = "student",
            PersonId = Guid.NewGuid(),
            UserType = UserType.Staff,
            IsEnabled = true,
            RoleIds = new List<Guid> { missingRoleId }
        };

        Assert.That(async () => await _userService.UpdateAsync(id, dto, CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("Role not found."));
    }

    [Test]
    public void DeleteAsync_Throws_NotFound_WhenMissing()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        Assert.That(async () => await _userService.DeleteAsync(Guid.NewGuid(), CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("User not found."));
    }

    [Test]
    public void UpdateAsync_Throws_SystemEntityException_WhenUserIsSystem()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var id = Guid.NewGuid();
        var user = new ApplicationUser { Id = id, IsSystem = true };
        _userManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(user);

        Assert.That(async () => await _userService.UpdateAsync(id, new UserUpdateRequest
        {
            Username = "x", RoleIds = new List<Guid>()
        }, CancellationToken.None), Throws.TypeOf<SystemEntityException>());

        _userManager.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Test]
    public void DeleteAsync_Throws_SystemEntityException_WhenUserIsSystem()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var id = Guid.NewGuid();
        var user = new ApplicationUser { Id = id, IsSystem = true };
        _userManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(user);

        Assert.That(async () => await _userService.DeleteAsync(id, CancellationToken.None),
            Throws.TypeOf<SystemEntityException>());

        _userManager.Verify(m => m.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Test]
    public async Task CreateAsync_AutoAssigns_DefaultRole_ForStudentUser()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var studentRole = new ApplicationRole
        {
            Id = SystemRoles.StudentRoleId, Name = "Student", UserType = UserType.Student
        };

        _userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "pw")).ReturnsAsync(IdentityResult.Success);
        _roleManager.Setup(r => r.FindByIdAsync(SystemRoles.StudentRoleId.ToString())).ReturnsAsync(studentRole);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(Array.Empty<string>());
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Student")).ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // No roles requested, yet the student still ends up with the Student role.
        var result = await _userService.CreateAsync(new UserUpsertRequest
        {
            Username = "pupil", Email = "p@test.com", Password = "pw", IsEnabled = true,
            UserType = UserType.Student, RoleIds = new List<Guid>()
        }, CancellationToken.None);

        Assert.That(result.Result.Succeeded, Is.True);
        _userManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Student"), Times.Once);
    }

    [Test]
    public void CreateAsync_Rejects_StaffRole_ForStudentUser()
    {
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var staffRoleId = Guid.NewGuid();
        var staffRole = new ApplicationRole { Id = staffRoleId, Name = "Teacher", UserType = UserType.Staff };

        _userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "pw")).ReturnsAsync(IdentityResult.Success);
        _roleManager.Setup(r => r.FindByIdAsync(staffRoleId.ToString())).ReturnsAsync(staffRole);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(Array.Empty<string>());

        Assert.That(async () => await _userService.CreateAsync(new UserUpsertRequest
        {
            Username = "pupil", Email = "p@test.com", Password = "pw", IsEnabled = true,
            UserType = UserType.Student, RoleIds = new List<Guid> { staffRoleId }
        }, CancellationToken.None), Throws.TypeOf<ValidationException>());
    }

    [Test]
    public void SetPasswordAsync_Throws_SystemEntity_WhenTargetIsSystem()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), IsSystem = true };
        _userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

        Assert.That(async () => await _userService.SetPasswordAsync(user.Id,
                new UserSetPasswordRequest { Password = "X" }, CancellationToken.None),
            Throws.TypeOf<SystemEntityException>());

        _userManager.Verify(m => m.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public void SetPasswordAsync_Forbidden_WhenTargetOutranksActor()
    {
        // Default actor id (from Setup) differs from the target, so the outrank guard runs.
        var user = new ApplicationUser { Id = Guid.NewGuid() };
        _userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

        _authorizationService
            .Setup(a => a.GetPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlySet<string>)new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "System.ViewUsers" });

        // Target holds an administrative permission the actor does not — resetting their password is takeover.
        _permissionRepository
            .Setup(p => p.GetPermissionsByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new PermissionResponse { Name = "System.EditRoles", FriendlyName = "Edit Roles", Area = "System.Users" } });

        Assert.That(async () => await _userService.SetPasswordAsync(user.Id,
                new UserSetPasswordRequest { Password = "X" }, CancellationToken.None),
            Throws.TypeOf<ForbiddenException>());

        _userManager.Verify(m => m.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task SetPasswordAsync_Allows_WhenTargetHoldsOnlyFunctionalPermissions()
    {
        // IT support (no functional permissions of their own) resetting a teacher's password must work:
        // the teacher holds only functional permissions, no administrative ones beyond the actor.
        var user = new ApplicationUser { Id = Guid.NewGuid() };
        _userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManager.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
        _userManager.Setup(m => m.ResetPasswordAsync(user, "reset-token", "X")).ReturnsAsync(IdentityResult.Success);

        _permissionRepository
            .Setup(p => p.GetPermissionsByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new PermissionResponse { Name = "Attendance.EditAttendanceMarks", FriendlyName = "Take Register", Area = "Attendance.Marks" }
            });

        var res = await _userService.SetPasswordAsync(user.Id,
            new UserSetPasswordRequest { Password = "X" }, CancellationToken.None);

        Assert.That(res.Succeeded, Is.True);
    }

    [Test]
    public void DeleteAsync_Forbidden_WhenDeletingSelf()
    {
        var id = Guid.NewGuid();
        _authorizationService.Setup(a => a.GetCurrentUserId()).Returns(id);

        var user = new ApplicationUser { Id = id };
        _userManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(user);

        Assert.That(async () => await _userService.DeleteAsync(id, CancellationToken.None),
            Throws.TypeOf<ForbiddenException>());

        _userManager.Verify(m => m.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Test]
    public void UpdateAsync_Forbidden_WhenDisablingSelf()
    {
        var id = Guid.NewGuid();
        _authorizationService.Setup(a => a.GetCurrentUserId()).Returns(id);

        var user = new ApplicationUser { Id = id, IsEnabled = true, UserType = UserType.Staff };
        _userManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(user);

        Assert.That(async () => await _userService.UpdateAsync(id, new UserUpdateRequest
        {
            Username = "me", PersonId = Guid.NewGuid(), UserType = UserType.Staff,
            IsEnabled = false, RoleIds = new List<Guid>()
        }, CancellationToken.None), Throws.TypeOf<ForbiddenException>());

        _userManager.Verify(m => m.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Test]
    public void DeleteAsync_Throws_WhenTargetIsLastEnabledAdmin()
    {
        // Actor (default id from Setup) differs from the target, so the self-guard is bypassed and the
        // last-admin guard is what fires.
        var admin = new ApplicationUser { Id = Guid.NewGuid(), IsEnabled = true };
        _userManager.Setup(m => m.FindByIdAsync(admin.Id.ToString())).ReturnsAsync(admin);
        _userManager.Setup(m => m.GetUsersInRoleAsync(SystemRoles.SystemAdministratorRoleName))
            .ReturnsAsync(new List<ApplicationUser> { admin });

        Assert.That(async () => await _userService.DeleteAsync(admin.Id, CancellationToken.None),
            Throws.TypeOf<ValidationException>());

        _userManager.Verify(m => m.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Test]
    public void CreateAsync_Forbidden_WhenAssigningRoleWithAdminPermissionAboveActor()
    {
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Admin", UserType = UserType.Staff };
        var permId = Guid.NewGuid();

        _userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "pw")).ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(Array.Empty<string>());
        _roleManager.Setup(r => r.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);

        _authorizationService
            .Setup(a => a.GetPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlySet<string>)new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "System.EditUsers" });
        _permissionRepository
            .Setup(p => p.GetListAsync(It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<Permission>
            {
                new() { Id = permId, Name = "System.EditRoles", FriendlyName = "Edit Roles", UserType = UserType.Staff }
            });
        _rolePermissionRepository.Setup(r => r.GetByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePermission> { new() { PermissionId = permId } });

        Assert.That(async () => await _userService.CreateAsync(new UserUpsertRequest
        {
            Username = "staff", Email = "s@test.com", Password = "pw", IsEnabled = true,
            UserType = UserType.Staff, PersonId = Guid.NewGuid(), RoleIds = new List<Guid> { roleId }
        }, CancellationToken.None), Throws.TypeOf<ForbiddenException>());

        _userManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CreateAsync_Allows_AssigningFunctionalRoleActorLacks()
    {
        // The core least-privilege case: IT support (holds only System.EditUsers, cannot take a register)
        // provisions a teacher account whose role grants the register permission. Must succeed.
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Teacher", UserType = UserType.Staff };
        var permId = Guid.NewGuid();

        _userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "pw")).ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(Array.Empty<string>());
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Teacher")).ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _roleManager.Setup(r => r.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);

        _authorizationService
            .Setup(a => a.GetPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlySet<string>)new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "System.EditUsers" });
        _permissionRepository
            .Setup(p => p.GetListAsync(It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<Permission>
            {
                new() { Id = permId, Name = "Attendance.EditAttendanceMarks", FriendlyName = "Take Register", UserType = UserType.Staff }
            });
        _rolePermissionRepository.Setup(r => r.GetByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePermission> { new() { PermissionId = permId } });

        var result = await _userService.CreateAsync(new UserUpsertRequest
        {
            Username = "teacher", Email = "t@test.com", Password = "pw", IsEnabled = true,
            UserType = UserType.Staff, PersonId = Guid.NewGuid(), RoleIds = new List<Guid> { roleId }
        }, CancellationToken.None);

        Assert.That(result.Result.Succeeded, Is.True);
        _userManager.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Teacher"), Times.Once);
    }

    [Test]
    public void UpdateAsync_Forbidden_WhenRemovingRoleWithAdminPermissionAboveActor()
    {
        // Symmetric to assignment: an actor who lacks a role's administrative permission cannot UNASSIGN
        // that role from a user either (that would let them sabotage access control they can't wield).
        _authorizationService
            .Setup(a => a.RequirePermissionAsync(Permissions.SystemAdmin.EditUsers, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var id = Guid.NewGuid();
        var user = new ApplicationUser { Id = id, IsEnabled = true, UserType = UserType.Staff };
        var adminRoleId = Guid.NewGuid();
        var adminRole = new ApplicationRole { Id = adminRoleId, Name = "AdminRole", UserType = UserType.Staff };
        var permId = Guid.NewGuid();

        _userManager.Setup(m => m.FindByIdAsync(id.ToString())).ReturnsAsync(user);
        // The user currently holds "AdminRole"; the update drops it (empty desired set).
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new[] { "AdminRole" });
        _roleManager.Setup(r => r.FindByNameAsync("AdminRole")).ReturnsAsync(adminRole);

        _authorizationService
            .Setup(a => a.GetPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlySet<string>)new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "System.EditUsers" });
        _permissionRepository
            .Setup(p => p.GetListAsync(It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<Permission>
            {
                new() { Id = permId, Name = "System.EditRoles", FriendlyName = "Edit Roles", UserType = UserType.Staff }
            });
        _rolePermissionRepository.Setup(r => r.GetByRoleIdAsync(adminRoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePermission> { new() { PermissionId = permId } });

        Assert.That(async () => await _userService.UpdateAsync(id, new UserUpdateRequest
        {
            PersonId = Guid.NewGuid(),
            Username = "staff",
            UserType = UserType.Staff,
            IsEnabled = true,
            RoleIds = new List<Guid>()
        }, CancellationToken.None), Throws.TypeOf<ForbiddenException>());

        _userManager.Verify(m => m.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }
}
