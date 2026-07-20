using System.Data;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Constants;
using MyPortal.Common.Enums;
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
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class RoleServiceTests
{
    private Mock<IAuthorizationService> _authorizationService;
    private Mock<ILogger<RoleService>> _logger;
    private Mock<IRoleRepository> _roleRepository;
    private Mock<IRolePermissionRepository> _rolePermissionRepository;
    private Mock<IPermissionRepository> _permissionRepository;
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
        _permissionRepository = new Mock<IPermissionRepository>(MockBehavior.Strict);
        _rolePermissionCache = new Mock<IRolePermissionCache>(MockBehavior.Loose);
        _roleManager = IdentityMocks.MockRoleManager<ApplicationRole>();
        _validationService = new Mock<IValidationService>(MockBehavior.Strict);

        _authorizationService
            .Setup(a => a.RequirePermissionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Default: actor holds no permissions. Tests that exercise the grant-ceiling guard override this.
        _authorizationService
            .Setup(a => a.GetPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlySet<string>)new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        _validationService
            .Setup(v => v.ValidateAsync(It.IsAny<RoleUpsertRequest>()))
            .Returns(Task.CompletedTask);

        // Default: no permissions known, so the audience check is a no-op. Tests that exercise the
        // audience guard override this with specific permissions.
        _permissionRepository
            .Setup(r => r.GetListAsync(It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<Permission>());

        _roleService = new RoleService(
            _authorizationService.Object,
            _logger.Object,
            _roleRepository.Object,
            _rolePermissionRepository.Object,
            _permissionRepository.Object,
            _rolePermissionCache.Object,
            _roleManager.Object,
            _validationService.Object
        );
    }

    [Test]
    public async Task GetDetailsByIdAsync_RequiresViewRoles_ThenReturnsDetails()
    {
        var id = Guid.NewGuid();
        var permId = Guid.NewGuid();
        var dto = new RoleDetailsResponse { Id = id, Name = "Some role" };

        _roleRepository.Setup(r => r.GetDetailsByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);
        _rolePermissionRepository.Setup(r => r.GetByRoleIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePermission> { new() { Id = Guid.NewGuid(), RoleId = id, PermissionId = permId } });

        var result = await _roleService.GetDetailsByIdAsync(id, CancellationToken.None);

        _authorizationService.Verify(
            a => a.RequirePermissionAsync(Permissions.SystemAdmin.ViewRoles, It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.That(result, Is.SameAs(dto));
        Assert.That(result!.PermissionIds, Is.EquivalentTo(new[] { permId }));
    }

    [Test]
    public async Task CreateAsync_ValidatesModel_ThenCreatesRole_AndAssignsPermissions()
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
        // Both requested permissions exist and match the (default) role audience.
        _permissionRepository
            .Setup(r => r.GetListAsync(It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<Permission>
            {
                new() { Id = permId1, Name = "P1", FriendlyName = "P1", Area = "A", UserType = UserType.Unknown },
                new() { Id = permId2, Name = "P2", FriendlyName = "P2", Area = "A", UserType = UserType.Unknown }
            });

        var result = await _roleService.CreateAsync(model, CancellationToken.None);

        Assert.That(result.Result.Succeeded, Is.True);
        Assert.That(result.RoleId, Is.Not.EqualTo(Guid.Empty));
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
    public async Task CreateAsync_DoesNotAssignPermissions_WhenRoleManagerFails()
    {
        var model = new RoleUpsertRequest
        {
            Name = "Custom Role",
            PermissionIds = new List<Guid> { Guid.NewGuid() }
        };

        _roleManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "X", Description = "no" }));

        var result = await _roleService.CreateAsync(model, CancellationToken.None);

        Assert.That(result.Result.Succeeded, Is.False);
        _rolePermissionRepository.Verify(r => r.GetByRoleIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _rolePermissionRepository.Verify(r => r.InsertAsync(It.IsAny<RolePermission>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()),
            Times.Never);
    }

    [Test]
    public async Task UpdateAsync_ValidatesModel_AndUpdatesPermissions()
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
        // The requested permissions exist and match the role audience.
        _permissionRepository
            .Setup(r => r.GetListAsync(It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<Permission>
            {
                new() { Id = keptPermId, Name = "K", FriendlyName = "K", Area = "A", UserType = UserType.Unknown },
                new() { Id = addedPermId, Name = "A", FriendlyName = "A", Area = "A", UserType = UserType.Unknown }
            });

        var model = new RoleUpsertRequest
        {
            Name = "New",
            Description = "new desc",
            PermissionIds = new List<Guid> { keptPermId, addedPermId }
        };

        var result = await _roleService.UpdateAsync(roleId, model, CancellationToken.None);

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
    public void UpdateAsync_Throws_NotFound_WhenRoleMissing()
    {
        var roleId = Guid.NewGuid();
        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync((ApplicationRole?)null);

        Assert.That(async () => await _roleService.UpdateAsync(roleId, new RoleUpsertRequest { Name = "X" },
                CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("Role not found."));
    }

    [Test]
    public void UpdateAsync_Throws_SystemEntityException_WhenRoleIsSystem()
    {
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "System Administrator", IsSystem = true };
        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);

        Assert.That(async () => await _roleService.UpdateAsync(roleId, new RoleUpsertRequest { Name = "X" },
                CancellationToken.None),
            Throws.TypeOf<SystemEntityException>());
    }

    [Test]
    public async Task DeleteAsync_DeletesPermissions_AndRole_AndInvalidatesCache()
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

        var result = await _roleService.DeleteAsync(roleId, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        _rolePermissionRepository.Verify(r => r.DeleteAsync(existingRolePermission.Id,
            It.IsAny<CancellationToken>(), It.IsAny<bool>(), It.IsAny<IDbTransaction?>()), Times.Once);
        _roleManager.Verify(m => m.DeleteAsync(role), Times.Once);
        _rolePermissionCache.Verify(c => c.Invalidate(roleId), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_DoesNotInvalidateCache_WhenRoleManagerFails()
    {
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Custom", IsSystem = false };

        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);
        _rolePermissionRepository.Setup(r => r.GetByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePermission>());
        _roleManager.Setup(m => m.DeleteAsync(role))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "X", Description = "no" }));

        var result = await _roleService.DeleteAsync(roleId, CancellationToken.None);

        Assert.That(result.Succeeded, Is.False);
        _rolePermissionCache.Verify(c => c.Invalidate(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public void DeleteAsync_Throws_NotFound_WhenRoleMissing()
    {
        var roleId = Guid.NewGuid();
        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync((ApplicationRole?)null);

        Assert.That(async () => await _roleService.DeleteAsync(roleId, CancellationToken.None),
            Throws.TypeOf<NotFoundException>().With.Message.EqualTo("Role not found."));
    }

    [Test]
    public void DeleteAsync_Throws_SystemEntityException_WhenRoleIsSystem()
    {
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "System Administrator", IsSystem = true };
        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);

        Assert.That(async () => await _roleService.DeleteAsync(roleId, CancellationToken.None),
            Throws.TypeOf<SystemEntityException>());
    }

    [Test]
    public void UpdateAsync_Throws_WhenRenamingDefaultRole()
    {
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Student", IsDefault = true, UserType = UserType.Student };
        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);

        Assert.That(async () => await _roleService.UpdateAsync(roleId,
                new RoleUpsertRequest { Name = "Pupils", UserType = UserType.Student }, CancellationToken.None),
            Throws.TypeOf<SystemEntityException>());
    }

    [Test]
    public async Task UpdateAsync_AllowsDescriptionAndPermissionEdit_OnDefaultRole()
    {
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Student", IsDefault = true, UserType = UserType.Student };
        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);
        _roleManager.Setup(m => m.UpdateAsync(role)).ReturnsAsync(IdentityResult.Success);
        _rolePermissionRepository.Setup(r => r.GetByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePermission>());

        var result = await _roleService.UpdateAsync(roleId, new RoleUpsertRequest
        {
            Name = "Student", // unchanged — allowed
            Description = "Updated description",
            UserType = UserType.Student,
            PermissionIds = new List<Guid>()
        }, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        _roleManager.Verify(m => m.UpdateAsync(role), Times.Once);
    }

    [Test]
    public void UpdateAsync_Rejects_PermissionOfWrongAudience()
    {
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Student", IsDefault = true, UserType = UserType.Student };
        var staffPermId = Guid.NewGuid();

        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);
        _roleManager.Setup(m => m.UpdateAsync(role)).ReturnsAsync(IdentityResult.Success);
        _permissionRepository
            .Setup(r => r.GetListAsync(It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<Permission>
            {
                new() { Id = staffPermId, Name = "X", FriendlyName = "X", Area = "A", UserType = UserType.Staff }
            });

        Assert.That(async () => await _roleService.UpdateAsync(roleId, new RoleUpsertRequest
        {
            Name = "Student",
            UserType = UserType.Student,
            PermissionIds = new List<Guid> { staffPermId }
        }, CancellationToken.None), Throws.TypeOf<ValidationException>());
    }

    [Test]
    public void UpdateAsync_Forbidden_WhenGrantingAdminPermissionActorDoesNotHold()
    {
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Teacher", UserType = UserType.Staff };
        var permId = Guid.NewGuid();

        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);
        _roleManager.Setup(m => m.UpdateAsync(role)).ReturnsAsync(IdentityResult.Success);

        // The role currently holds no permissions, so the admin permission below is being ADDED.
        _rolePermissionRepository.Setup(r => r.GetByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePermission>());

        // An administrative (System.*) permission the actor does not hold — granting it would escalate.
        _permissionRepository
            .Setup(r => r.GetListAsync(It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<Permission>
            {
                new() { Id = permId, Name = "System.EditRoles", FriendlyName = "Edit Roles", Area = "System.Users", UserType = UserType.Staff }
            });

        _authorizationService
            .Setup(a => a.GetPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlySet<string>)new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "System.ViewUsers" });

        Assert.That(async () => await _roleService.UpdateAsync(roleId, new RoleUpsertRequest
        {
            Name = "Teacher",
            UserType = UserType.Staff,
            PermissionIds = new List<Guid> { permId }
        }, CancellationToken.None), Throws.TypeOf<ForbiddenException>());
    }

    [Test]
    public void UpdateAsync_Forbidden_WhenRemovingAdminPermissionActorDoesNotHold()
    {
        // Symmetric to granting: an actor who does not hold an administrative permission cannot strip it
        // from a role either (sabotaging access control they can't wield themselves).
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Teacher", UserType = UserType.Staff };
        var adminPermId = Guid.NewGuid();

        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);
        _roleManager.Setup(m => m.UpdateAsync(role)).ReturnsAsync(IdentityResult.Success);

        // The role currently HOLDS the admin permission; the update drops it (empty desired set).
        _rolePermissionRepository.Setup(r => r.GetByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePermission>
            {
                new() { Id = Guid.NewGuid(), RoleId = roleId, PermissionId = adminPermId }
            });

        _permissionRepository
            .Setup(r => r.GetListAsync(It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<Permission>
            {
                new() { Id = adminPermId, Name = "System.EditRoles", FriendlyName = "Edit Roles", Area = "System.Users", UserType = UserType.Staff }
            });

        _authorizationService
            .Setup(a => a.GetPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlySet<string>)new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "System.ViewUsers" });

        Assert.That(async () => await _roleService.UpdateAsync(roleId, new RoleUpsertRequest
        {
            Name = "Teacher",
            UserType = UserType.Staff,
            PermissionIds = new List<Guid>()
        }, CancellationToken.None), Throws.TypeOf<ForbiddenException>());
    }

    [Test]
    public async Task UpdateAsync_Allows_KeepingAdminPermissionActorDoesNotHold_WhenEditingOthers()
    {
        // Only CHANGED permissions are gated: an actor lacking System.EditRoles may still edit a role that
        // carries it, as long as that admin permission is left untouched. Here they swap a functional perm.
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Teacher", UserType = UserType.Staff };
        var adminPermId = Guid.NewGuid();   // System.* the actor lacks — stays on the role, unchanged
        var funcOldId = Guid.NewGuid();     // functional perm being removed
        var funcNewId = Guid.NewGuid();     // functional perm being added

        var existing = new List<RolePermission>
        {
            new() { Id = Guid.NewGuid(), RoleId = roleId, PermissionId = adminPermId },
            new() { Id = Guid.NewGuid(), RoleId = roleId, PermissionId = funcOldId }
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

        _permissionRepository
            .Setup(r => r.GetListAsync(It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<Permission>
            {
                new() { Id = adminPermId, Name = "System.EditRoles", FriendlyName = "Edit Roles", Area = "System.Users", UserType = UserType.Staff },
                new() { Id = funcOldId, Name = "Attendance.EditMarks", FriendlyName = "Edit Marks", Area = "Attendance", UserType = UserType.Staff },
                new() { Id = funcNewId, Name = "Attendance.ViewMarks", FriendlyName = "View Marks", Area = "Attendance", UserType = UserType.Staff }
            });

        _authorizationService
            .Setup(a => a.GetPermissionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlySet<string>)new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "System.ViewUsers" });

        var result = await _roleService.UpdateAsync(roleId, new RoleUpsertRequest
        {
            Name = "Teacher",
            UserType = UserType.Staff,
            PermissionIds = new List<Guid> { adminPermId, funcNewId }
        }, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        // The untouched admin permission is neither re-inserted nor deleted; only the functional swap happens.
        _rolePermissionRepository.Verify(r => r.InsertAsync(
            It.Is<RolePermission>(rp => rp.PermissionId == funcNewId), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Once);
        _rolePermissionRepository.Verify(r => r.InsertAsync(
            It.Is<RolePermission>(rp => rp.PermissionId == adminPermId), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
        _rolePermissionRepository.Verify(r => r.DeleteAsync(
            It.Is<Guid>(id => existing.Any(e => e.Id == id && e.PermissionId == funcOldId)),
            It.IsAny<CancellationToken>(), It.IsAny<bool>(), It.IsAny<IDbTransaction?>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_Allows_GrantingFunctionalPermissionActorDoesNotHold()
    {
        // Least-privilege delegation: an admin who cannot take a register can still grant that functional
        // permission to a role. Actor holds no permissions at all (default setup).
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Teacher", UserType = UserType.Staff };
        var permId = Guid.NewGuid();

        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);
        _roleManager.Setup(m => m.UpdateAsync(role)).ReturnsAsync(IdentityResult.Success);
        _permissionRepository
            .Setup(r => r.GetListAsync(It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<Permission>
            {
                new() { Id = permId, Name = "Attendance.EditAttendanceMarks", FriendlyName = "Take Register", Area = "Attendance.Marks", UserType = UserType.Staff }
            });
        _rolePermissionRepository.Setup(r => r.GetByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RolePermission>());
        _rolePermissionRepository
            .Setup(r => r.InsertAsync(It.IsAny<RolePermission>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((RolePermission rp, CancellationToken _, IDbTransaction? _) => rp);

        var result = await _roleService.UpdateAsync(roleId, new RoleUpsertRequest
        {
            Name = "Teacher",
            UserType = UserType.Staff,
            PermissionIds = new List<Guid> { permId }
        }, CancellationToken.None);

        Assert.That(result.Succeeded, Is.True);
        _rolePermissionRepository.Verify(r => r.InsertAsync(
            It.Is<RolePermission>(rp => rp.PermissionId == permId), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()),
            Times.Once);
    }

    [Test]
    public void UpdateAsync_Throws_Validation_WhenPermissionDoesNotExist()
    {
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole { Id = roleId, Name = "Teacher", UserType = UserType.Staff };

        _roleManager.Setup(m => m.FindByIdAsync(roleId.ToString())).ReturnsAsync(role);
        _roleManager.Setup(m => m.UpdateAsync(role)).ReturnsAsync(IdentityResult.Success);
        // Default GetListAsync returns an empty catalogue, so the requested id is unknown.

        Assert.That(async () => await _roleService.UpdateAsync(roleId, new RoleUpsertRequest
        {
            Name = "Teacher",
            UserType = UserType.Staff,
            PermissionIds = new List<Guid> { Guid.NewGuid() }
        }, CancellationToken.None), Throws.TypeOf<ValidationException>());
    }
}
