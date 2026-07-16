using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Constants;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.School.Bulletins;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using IAuthorizationService = MyPortal.Auth.Interfaces.IAuthorizationService;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class BulletinCategoryServiceTests
{
    private Mock<IAuthorizationService> _authorizationService = null!;
    private Mock<IBulletinCategoryRepository> _repository = null!;
    private Mock<ILogger<BulletinCategoryService>> _logger = null!;

    private BulletinCategoryService _service = null!;

    [SetUp]
    public void Setup()
    {
        _authorizationService = new Mock<IAuthorizationService>(MockBehavior.Strict);
        _repository = new Mock<IBulletinCategoryRepository>(MockBehavior.Strict);
        _logger = new Mock<ILogger<BulletinCategoryService>>(MockBehavior.Loose);

        _service = new BulletinCategoryService(
            _authorizationService.Object,
            _repository.Object,
            _logger.Object);
    }

    private static BulletinCategory MakeCategory(string name, int displayOrder, bool active,
        bool isSystem = false, long version = 1) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Icon = "pi pi-info-circle",
            ColourCode = "#0066CC",
            DisplayOrder = displayOrder,
            Active = active,
            IsSystem = isSystem,
            CreatedByIpAddress = "::1",
            LastModifiedByIpAddress = "::1",
            Version = version
        };

    private void RequireViewPermission(bool succeeds = true)
    {
        var setup = _authorizationService.Setup(a =>
            a.RequirePermissionAsync(Permissions.School.ViewSchoolBulletins, It.IsAny<CancellationToken>()));
        if (succeeds) setup.Returns(Task.CompletedTask);
        else setup.ThrowsAsync(new ForbiddenException("missing view permission"));
    }

    private void RequireSettingsPermission(bool succeeds = true)
    {
        var setup = _authorizationService.Setup(a =>
            a.RequirePermissionAsync(Permissions.SystemAdmin.BulletinSettings, It.IsAny<CancellationToken>()));
        if (succeeds) setup.Returns(Task.CompletedTask);
        else setup.ThrowsAsync(new ForbiddenException("missing bulletin settings permission"));
    }

    private void StubGetListPaged(IEnumerable<BulletinCategory> items)
    {
        var list = items.ToList();
        _repository.Setup(r => r.GetListPagedAsync(
                It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<PageOptions?>(),
                It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new PageResult<BulletinCategory> { Items = list, TotalItems = list.Count });
    }

    [Test]
    public async Task GetAllAsync_DefaultIncludeInactiveFalse_FiltersInactive_AndSortsByDisplayOrderThenName()
    {
        RequireViewPermission();
        StubGetListPaged(new[]
        {
            MakeCategory("Zebra",   displayOrder: 2, active: true),
            MakeCategory("Inactive", displayOrder: 1, active: false), // filtered out
            MakeCategory("Apple",   displayOrder: 2, active: true),  // same DisplayOrder as Zebra; sorts by Name
            MakeCategory("Banana",  displayOrder: 1, active: true)
        });

        var result = await _service.GetAllAsync(includeInactive: false, CancellationToken.None);

        Assert.That(result.Select(c => c.Name), Is.EqualTo(new[] { "Banana", "Apple", "Zebra" }));
    }

    [Test]
    public async Task GetAllAsync_IncludeInactiveTrue_ReturnsAll()
    {
        RequireViewPermission();
        StubGetListPaged(new[]
        {
            MakeCategory("Active",   displayOrder: 1, active: true),
            MakeCategory("Inactive", displayOrder: 2, active: false)
        });

        var result = await _service.GetAllAsync(includeInactive: true, CancellationToken.None);

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Select(c => c.Name), Does.Contain("Inactive"));
    }

    [Test]
    public void GetAllAsync_PropagatesPermissionDenial()
    {
        RequireViewPermission(succeeds: false);

        Assert.That(async () => await _service.GetAllAsync(includeInactive: false, CancellationToken.None),
            Throws.TypeOf<ForbiddenException>());

        _repository.Verify(r => r.GetListPagedAsync(
            It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<PageOptions?>(),
            It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    [Test]
    public async Task GetByIdAsync_ReturnsMappedCategory_WhenFound()
    {
        RequireViewPermission();
        var entity = MakeCategory("Notices", displayOrder: 1, active: true, isSystem: true, version: 7);

        _repository.Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(entity);

        var result = await _service.GetByIdAsync(entity.Id, CancellationToken.None);

        Assert.That(result.Id,           Is.EqualTo(entity.Id));
        Assert.That(result.Name,         Is.EqualTo("Notices"));
        Assert.That(result.IsSystem,     Is.True);
        Assert.That(result.Version,      Is.EqualTo(7));
    }

    [Test]
    public void GetByIdAsync_Throws_NotFound_WhenMissing()
    {
        RequireViewPermission();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((BulletinCategory?)null);

        Assert.That(async () => await _service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None),
            Throws.TypeOf<NotFoundException>());
    }

    [Test]
    public async Task CreateAsync_RequiresBulletinSettingsPermission_AndInsertsCategoryWithIsSystemFalse()
    {
        RequireSettingsPermission();
        _repository.Setup(r => r.InsertAsync(It.IsAny<BulletinCategory>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((BulletinCategory c, CancellationToken _, IDbTransaction? _) => c);

        var model = new BulletinCategoryUpsertRequest
        {
            Name = "Notices",
            Icon = "pi pi-megaphone",
            ColourCode = "#FF0000",
            DisplayOrder = 3,
            Active = true
        };

        var id = await _service.CreateAsync(model, CancellationToken.None);

        Assert.That(id, Is.Not.EqualTo(Guid.Empty));
        _repository.Verify(r => r.InsertAsync(
            It.Is<BulletinCategory>(c =>
                c.Id == id && c.Name == "Notices" && c.Icon == "pi pi-megaphone" &&
                c.ColourCode == "#FF0000" && c.DisplayOrder == 3 && c.Active && !c.IsSystem),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Once);
    }

    [Test]
    public void CreateAsync_PropagatesPermissionDenial()
    {
        RequireSettingsPermission(succeeds: false);

        Assert.That(async () => await _service.CreateAsync(new BulletinCategoryUpsertRequest
        {
            Name = "X", Icon = "i", ColourCode = "#000000"
        }, CancellationToken.None), Throws.TypeOf<ForbiddenException>());

        _repository.Verify(r => r.InsertAsync(It.IsAny<BulletinCategory>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_AppliesFields_AndHandsExpectedVersionToRepo()
    {
        RequireSettingsPermission();
        var existing = MakeCategory("Old", displayOrder: 1, active: true, version: 3);
        _repository.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(existing);
        _repository.Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(existing);

        var model = new BulletinCategoryUpsertRequest
        {
            Name = "New",
            Icon = "pi pi-bell",
            ColourCode = "#00FF00",
            DisplayOrder = 5,
            Active = false,
            ExpectedVersion = 3
        };

        await _service.UpdateAsync(existing.Id, model, CancellationToken.None);

        Assert.That(existing.Name,         Is.EqualTo("New"));
        Assert.That(existing.Icon,         Is.EqualTo("pi pi-bell"));
        Assert.That(existing.ColourCode,   Is.EqualTo("#00FF00"));
        Assert.That(existing.DisplayOrder, Is.EqualTo(5));
        Assert.That(existing.Active,       Is.False);
        Assert.That(existing.Version,      Is.EqualTo(3));
        _repository.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()),
            Times.Once);
    }

    [Test]
    public void UpdateAsync_Throws_NotFound_WhenMissing()
    {
        RequireSettingsPermission();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((BulletinCategory?)null);

        Assert.That(async () => await _service.UpdateAsync(Guid.NewGuid(), new BulletinCategoryUpsertRequest
        {
            Name = "X", Icon = "i", ColourCode = "#000000"
        }, CancellationToken.None), Throws.TypeOf<NotFoundException>());

        _repository.Verify(r => r.UpdateAsync(It.IsAny<BulletinCategory>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    [Test]
    public async Task DeleteAsync_RequiresBulletinSettingsPermission_AndDeletes()
    {
        RequireSettingsPermission();
        var id = Guid.NewGuid();
        _repository.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>(),
                It.IsAny<bool>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(true);

        await _service.DeleteAsync(id, CancellationToken.None);

        _repository.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>(),
            It.IsAny<bool>(), It.IsAny<IDbTransaction?>()), Times.Once);
    }
}
