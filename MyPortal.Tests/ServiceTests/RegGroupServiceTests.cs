using System.Data;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Pastoral;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.Pastoral;
using MyPortal.Services.Pastoral;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class RegGroupServiceTests
{
    private Mock<IAuthorizationService> _auth = null!;
    private Mock<ILogger<RegGroupService>> _logger = null!;
    private Mock<IUnitOfWorkFactory> _uowFactory = null!;
    private Mock<IRegGroupRepository> _regGroupRepository = null!;
    private Mock<IYearGroupRepository> _yearGroupRepository = null!;
    private Mock<IStudentGroupService> _studentGroupService = null!;
    private Mock<IUnitOfWork> _uow = null!;
    private RegGroupService _service = null!;

    [SetUp]
    public void Setup()
    {
        _auth = new Mock<IAuthorizationService>(MockBehavior.Strict);
        _logger = new Mock<ILogger<RegGroupService>>(MockBehavior.Loose);
        _uowFactory = new Mock<IUnitOfWorkFactory>(MockBehavior.Strict);
        _regGroupRepository = new Mock<IRegGroupRepository>(MockBehavior.Strict);
        _yearGroupRepository = new Mock<IYearGroupRepository>(MockBehavior.Strict);
        _studentGroupService = new Mock<IStudentGroupService>(MockBehavior.Strict);
        _uow = new Mock<IUnitOfWork>(MockBehavior.Loose);

        _auth.Setup(a => a.RequirePermissionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uowFactory.Setup(f => f.BeginAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_uow.Object);

        _service = new RegGroupService(_auth.Object, _logger.Object, _uowFactory.Object,
            _regGroupRepository.Object, _yearGroupRepository.Object, _studentGroupService.Object);
    }

    private static RegGroupUpsertRequest ValidRequest(Guid academicYearId, Guid yearGroupId) => new()
    {
        AcademicYearId = academicYearId,
        YearGroupId = yearGroupId,
        Code = "7A",
        Name = "7A"
    };

    [Test]
    public void CreateAsync_Throws_Validation_WhenYearGroupInDifferentAcademicYear()
    {
        var academicYearId = Guid.NewGuid();
        var yearGroupId = Guid.NewGuid();

        // The year group resolves to a *different* academic year.
        _yearGroupRepository
            .Setup(r => r.GetAcademicYearIdAsync(yearGroupId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(Guid.NewGuid());

        Assert.That(async () => await _service.CreateAsync(ValidRequest(academicYearId, yearGroupId), CancellationToken.None),
            Throws.TypeOf<ValidationException>());

        _studentGroupService.Verify(s => s.CreateAsync(It.IsAny<Guid>(), It.IsAny<StudentGroupUpsertCore>(),
            It.IsAny<IList<StudentGroupSupervisorUpsertRequest>>(), It.IsAny<CancellationToken>(), It.IsAny<IUnitOfWork?>()),
            Times.Never);
    }

    [Test]
    public void CreateAsync_Throws_NotFound_WhenYearGroupMissing()
    {
        var academicYearId = Guid.NewGuid();
        var yearGroupId = Guid.NewGuid();

        _yearGroupRepository
            .Setup(r => r.GetAcademicYearIdAsync(yearGroupId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((Guid?)null);

        Assert.That(async () => await _service.CreateAsync(ValidRequest(academicYearId, yearGroupId), CancellationToken.None),
            Throws.TypeOf<NotFoundException>());
    }
}
