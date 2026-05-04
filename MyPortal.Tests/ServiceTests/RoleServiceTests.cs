using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.System.Roles;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces;
using MyPortal.Services.System;
using MyPortal.Tests.Mocks;
using Task = System.Threading.Tasks.Task;
using MyPortal.Data.Interfaces;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class RoleServiceTests
{
    private Mock<IAuthorizationService> _authorizationService;
    private Mock<ILogger<RoleService>> _logger;
    private Mock<IRoleRepository> _roleRepository;
    private Mock<IRolePermissionRepository> _rolePermissionRepository;
    private Mock<IRolePermissionCache> _rolePermissionCache;
    private Mock<RoleManager<ApplicationRole>> _roleManager;
    private Mock<IValidationService> _validationService;

    private RoleService _roleService;

    [SetUp]
    public void Setup()
    {
        _authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        _logger = new Mock<ILogger<RoleService>>(MockBehavior.Loose);
        _roleRepository = new Mock<IRoleRepository>(MockBehavior.Strict);
        _rolePermissionRepository = new Mock<IRolePermissionRepository>(MockBehavior.Strict);
        _rolePermissionCache = new Mock<IRolePermissionCache>(MockBehavior.Loose);
        _roleManager = IdentityMocks.MockRoleManager<ApplicationRole>();
        _validationService = new Mock<IValidationService>(MockBehavior.Strict);

        _authorizationService
            .Setup(a => a.RequirePermissionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _validationService
            .Setup(v => v.ValidateAsync(It.IsAny<RoleUpsertRequest>()))
            .Returns(Task.CompletedTask);

        _roleService = new RoleService(
            _authorizationService.Object,
            _logger.Object,
            _roleRepository.Object,
            _rolePermissionRepository.Object,
            _rolePermissionCache.Object,
            _roleManager.Object,
            _validationService.Object
        );
    }

    [Test]
    public async Task GetDetailsByIdAsync_RequiresViewRoles_ThenReturnsDetails()
    {
        var id = Guid.NewGuid();
        var dto = new RoleDetailsResponse { Id = id, Name = "Some role" };

        _roleRepository.Setup(r => r.GetDetailsByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _roleService.GetDetailsByIdAsync(id, CancellationToken.None);

        _authorizationService.Verify(
            a => a.RequirePermissionAsync(Permissions.SystemAdmin.ViewRoles, It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.That(result, Is.SameAs(dto));
    }

    [Test]
    public async Task CreateRoleAsync_ValidatesModel_ThenCreatesRole_AndAssignsPermissions()
    {
        var permId1 = Guid.NewGuid();
        var permId2 = Guid.NewGuid();
        var model = new RoleUpsertRequest
        {
            Name = "Custom Role",
            Description = "desc",
            PermissionIds = new List<Guid> { permId1, permId2 }
        };

        _roleManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);
        _rolePermissionRepository.Setup(r => r.GetByRoleIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePermission>());
        _rolePermissionRepository
            .Setup(r => r.InsertAsync(It.IsAny<RolePermission>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((RolePermission rp, CancellationToken _, IDbTransaction? _) => rp);

        var result = await _roleService.CreateRoleAsync(model, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        _validationService.Verify(v => v.ValidateAsync(model), Times.Once);
        _roleManager.Verify(m => m.CreateAsync(
            It.Is<ApplicationRole>(r => r.Name == "Custom Role" && r.Description == "desc" && !r.IsSystem)),
            Times.Once);
        // Two new permissions, no removals.
        _rolePermissionRepository.Verify(r => r.InsertAsync(
            It.Is<RolePermission>(rp => rp.PermissionId == permId1), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Once);
        _rolePermissionRepository.Verify(r => r.InsertAsync(
            It.Is<RolePermission>(rp => rp.PermissionId == permId2), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Once);
    }

    [Test]
    public async Task CreateRoleAsync_DoesNotAssignPermissions_WhenRoleManagerFails()
    {
        var model = new RoleUpsertRequest
        {
            Name = "Custom Role",
            PermissionIds = new List<Guid> { Guid.NewGuid() }
        };

        _roleManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "X", Description = "no" }));

        var result = await _roleService.CreateRoleAsync(model, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        _rolePermissionRepository.Verify(r => r.GetByRoleIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _rolePermissionRepository.Verify(r => r.InsertAsync(It.IsAny<RolePermission>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()),
            Times.Never);
    }

    [Test]
    public async Task UpdateRoleAsync_ValidatesModel_AndUpdatesPermissions()
    {
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Old", IsSystem = false };

        var keptPermId = Guid.NewGuid();
        var addedPermId = Guid.NewGuid();
        var removedPermId = Guid.NewGuid();

        var existing = new List<RolePermission>
        {
            new RolePermission { Id = Guid.NewGuid(), RoleId = roleId, PermissionId = keptPermId },
            new RolePermission { Id = Guid.NewGuid(), RoleId = roleId, PermissionId = removedPermId }
        };

        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);
        _roleManager.Setup(m => m.UpdateAsync(role)).ReturnsAsync(IdentityResult.Success);
        _rolePermissionRepository.Setup(r => r.GetByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _rolePermissionRepository
            .Setup(r => r.InsertAsync(It.IsAny<RolePermission>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((RolePermission rp, CancellationToken _, IDbTransaction? _) => rp);
        _rolePermissionRepository
            .Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<bool>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(true);

        var model = new RoleUpsertRequest
        {
            Name = "New",
            Description = "new desc",
            PermissionIds = new List<Guid> { keptPermId, addedPermId }
        };

        var result = await _roleService.UpdateRoleAsync(roleId, model, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(role.Name, Is.EqualTo("New"));
        Assert.That(role.Description, Is.EqualTo("new desc"));
        _validationService.Verify(v => v.ValidateAsync(model), Times.Once);
        // Only the new permission is inserted; only the dropped one is deleted.
        _rolePermissionRepository.Verify(r => r.InsertAsync(
            It.Is<RolePermission>(rp => rp.PermissionId == addedPermId), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Once);
        _rolePermissionRepository.Verify(r => r.DeleteAsync(
            It.Is<Guid>(id => existing.Any(e => e.Id == id && e.PermissionId == removedPermId)),
            It.IsAny<CancellationToken>(), It.IsAny<bool>(), It.IsAny<IDbTransaction?>()), Times.Once);
        // The kept permission is neither re-inserted nor deleted.
        _rolePermissionRepository.Verify(r => r.InsertAsync(
            It.Is<RolePermission>(rp => rp.PermissionId == keptPermId), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    [Test]
    public void UpdateRoleAsync_Throws_NotFound_WhenRoleMissing()
    {
        var roleId = Guid.NewGuid();
        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync((ApplicationRole?)null);

        Assert.That(async () => await _roleService.UpdateRoleAsync(roleId, new RoleUpsertRequest { Name = "X" },
                CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("Role not found."));
    }

    [Test]
    public void UpdateRoleAsync_Throws_SystemEntityException_WhenRoleIsSystem()
    {
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "System Administrator", IsSystem = true };
        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);

        Assert.That(async () => await _roleService.UpdateRoleAsync(roleId, new RoleUpsertRequest { Name = "X" },
                CancellationToken.None),
            Throws.TypeOf<SystemEntityException>());
    }

    [Test]
    public async Task DeleteRoleAsync_DeletesPermissions_AndRole_AndInvalidatesCache()
    {
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Custom", IsSystem = false };
        var existingRolePermission = new RolePermission { Id = Guid.NewGuid(), RoleId = roleId };

        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);
        _rolePermissionRepository.Setup(r => r.GetByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePermission> { existingRolePermission });
        _rolePermissionRepository
            .Setup(r => r.DeleteAsync(existingRolePermission.Id, It.IsAny<CancellationToken>(), It.IsAny<bool>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(true);
        _roleManager.Setup(m => m.DeleteAsync(role)).ReturnsAsync(IdentityResult.Success);

        var result = await _roleService.DeleteRoleAsync(roleId, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        _rolePermissionRepository.Verify(r => r.DeleteAsync(existingRolePermission.Id,
            It.IsAny<CancellationToken>(), It.IsAny<bool>(), It.IsAny<IDbTransaction?>()), Times.Once);
        _roleManager.Verify(m => m.DeleteAsync(role), Times.Once);
        _rolePermissionCache.Verify(c => c.Invalidate(roleId), Times.Once);
    }

    [Test]
    public async Task DeleteRoleAsync_DoesNotInvalidateCache_WhenRoleManagerFails()
    {
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Custom", IsSystem = false };

        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);
        _rolePermissionRepository.Setup(r => r.GetByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePermission>());
        _roleManager.Setup(m => m.DeleteAsync(role))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "X", Description = "no" }));

        var result = await _roleService.DeleteRoleAsync(roleId, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        _rolePermissionCache.Verify(c => c.Invalidate(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public void DeleteRoleAsync_Throws_NotFound_WhenRoleMissing()
    {
        var roleId = Guid.NewGuid();
        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync((ApplicationRole?)null);

        Assert.That(async () => await _roleService.DeleteRoleAsync(roleId, CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("Role not found."));
    }

    [Test]
    public void DeleteRoleAsync_Throws_SystemEntityException_WhenRoleIsSystem()
    {
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "System Administrator", IsSystem = true };
        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);

        Assert.That(async () => await _roleService.DeleteRoleAsync(roleId, CancellationToken.None),
            Throws.TypeOf<SystemEntityException>());
    }
}
