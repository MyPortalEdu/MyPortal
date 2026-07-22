using System.Data;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Pastoral;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Pastoral;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class StudentGroupServiceTests
{
    private Mock<IAuthorizationService> _auth = null!;
    private Mock<ILogger<StudentGroupService>> _logger = null!;
    private Mock<IUnitOfWorkFactory> _uowFactory = null!;
    private Mock<IStudentGroupRepository> _studentGroupRepository = null!;
    private Mock<IStudentGroupSupervisorRepository> _supervisorRepository = null!;
    private Mock<IAcademicYearRepository> _academicYearRepository = null!;
    private Mock<IUnitOfWork> _uow = null!;
    private StudentGroupService _service = null!;

    [SetUp]
    public void Setup()
    {
        _auth = new Mock<IAuthorizationService>(MockBehavior.Strict);
        _logger = new Mock<ILogger<StudentGroupService>>(MockBehavior.Loose);
        _uowFactory = new Mock<IUnitOfWorkFactory>(MockBehavior.Strict);
        _studentGroupRepository = new Mock<IStudentGroupRepository>(MockBehavior.Strict);
        _supervisorRepository = new Mock<IStudentGroupSupervisorRepository>(MockBehavior.Strict);
        _academicYearRepository = new Mock<IAcademicYearRepository>(MockBehavior.Strict);
        _uow = new Mock<IUnitOfWork>(MockBehavior.Loose);

        _uowFactory.Setup(f => f.BeginAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_uow.Object);

        // The academic year is unlocked by default so the lock guard passes.
        _academicYearRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new AcademicYear { Id = Guid.NewGuid(), IsLocked = false });

        _service = new StudentGroupService(_auth.Object, _logger.Object, _uowFactory.Object,
            _studentGroupRepository.Object, _supervisorRepository.Object, _academicYearRepository.Object);
    }

    private static StudentGroupUpsertCore Core() => new()
    {
        Code = "7A",
        Name = "7A",
        Active = true
    };

    [Test]
    public void CreateAsync_Throws_Validation_WhenCodeAlreadyExistsInAcademicYear()
    {
        var academicYearId = Guid.NewGuid();

        _studentGroupRepository
            .Setup(r => r.CodeExistsAsync(academicYearId, "7A", null, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(true);

        Assert.That(async () => await _service.CreateAsync(academicYearId, Core(),
                new List<StudentGroupSupervisorUpsertRequest>(), CancellationToken.None),
            Throws.TypeOf<ValidationException>());

        _studentGroupRepository.Verify(r => r.InsertAsync(It.IsAny<StudentGroup>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    [Test]
    public void CreateAsync_Throws_Validation_WhenSameSupervisorListedTwice()
    {
        var academicYearId = Guid.NewGuid();
        var staffMemberId = Guid.NewGuid();

        _studentGroupRepository
            .Setup(r => r.CodeExistsAsync(academicYearId, "7A", null, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(false);
        _studentGroupRepository
            .Setup(r => r.InsertAsync(It.IsAny<StudentGroup>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((StudentGroup g, CancellationToken _, IDbTransaction? _) => g);

        var supervisors = new List<StudentGroupSupervisorUpsertRequest>
        {
            new() { StaffMemberId = staffMemberId, Title = "Form Tutor" },
            new() { StaffMemberId = staffMemberId, Title = "Head of Year" }
        };

        Assert.That(async () => await _service.CreateAsync(academicYearId, Core(), supervisors, CancellationToken.None),
            Throws.TypeOf<ValidationException>());
    }
}
