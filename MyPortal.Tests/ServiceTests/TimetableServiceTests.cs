using System.Data;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Timetabler;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Curriculum.Timetable;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Timetable;
using TimetableEntity = MyPortal.Core.Entities.Timetable;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class TimetableServiceTests
{
    private Mock<ITimetableRepository> _repository = null!;
    private Mock<IAcademicYearRepository> _academicYearRepository = null!;
    private TimetableService _service = null!;

    [SetUp]
    public void Setup()
    {
        _repository = new Mock<ITimetableRepository>();
        _academicYearRepository = new Mock<IAcademicYearRepository>();

        _service = new TimetableService(
            Mock.Of<IAuthorizationService>(),
            Mock.Of<ILogger<TimetableService>>(),
            _repository.Object,
            Mock.Of<ITimetableRunRepository>(),
            Mock.Of<ITimetableMaterialisationService>(),
            _academicYearRepository.Object,
            Mock.Of<IValidationService>(),
            Mock.Of<IUnitOfWorkFactory>());
    }

    [Test]
    public void CreateDraftAsync_Throws_NotFound_WhenAcademicYearMissing()
    {
        var ayId = Guid.NewGuid();
        _academicYearRepository
            .Setup(r => r.GetByIdAsync(ayId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((AcademicYear?)null);

        Assert.That(async () => await _service.CreateDraftAsync(
                new TimetableUpsertRequest { AcademicYearId = ayId, Name = "Main" }, CancellationToken.None),
            Throws.TypeOf<NotFoundException>());
    }

    [Test]
    public void CreateDraftAsync_Throws_Validation_WhenNameExistsInYear()
    {
        var ayId = Guid.NewGuid();
        _academicYearRepository
            .Setup(r => r.GetByIdAsync(ayId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new AcademicYear { Id = ayId });
        _repository
            .Setup(r => r.ListByAcademicYearAsync(ayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TimetableEntity> { new() { Id = Guid.NewGuid(), Name = "Main" } });

        Assert.That(async () => await _service.CreateDraftAsync(
                new TimetableUpsertRequest { AcademicYearId = ayId, Name = "Main" }, CancellationToken.None),
            Throws.TypeOf<ValidationException>());
    }

    [Test]
    public void ApplyAsync_Throws_WhenDraftHasNoAssignments()
    {
        var ttId = Guid.NewGuid();
        var ayId = Guid.NewGuid();
        _repository
            .Setup(r => r.GetByIdAsync(ttId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new TimetableEntity { Id = ttId, AcademicYearId = ayId, Status = TimetableStatus.Draft });
        _repository
            .Setup(r => r.ListAssignmentsAsync(ttId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<TimetableAssignment>());

        Assert.That(async () => await _service.ApplyAsync(ttId,
                new TimetableApplyRequest { EffectiveFrom = new DateTime(2026, 9, 1) }, CancellationToken.None),
            Throws.TypeOf<InvalidOperationException>());
    }

    [Test]
    public void ApplyAsync_Throws_WhenEffectiveFromNotAfterActiveTimetable()
    {
        var ttId = Guid.NewGuid();
        var ayId = Guid.NewGuid();
        _repository
            .Setup(r => r.GetByIdAsync(ttId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new TimetableEntity { Id = ttId, AcademicYearId = ayId, Status = TimetableStatus.Draft });
        _repository
            .Setup(r => r.ListAssignmentsAsync(ttId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<TimetableAssignment> { new() { Id = Guid.NewGuid() } });
        _repository
            .Setup(r => r.ListByAcademicYearAsync(ayId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TimetableEntity>
            {
                new() { Id = Guid.NewGuid(), Status = TimetableStatus.Active, EffectiveFrom = new DateTime(2026, 9, 1) }
            });

        Assert.That(async () => await _service.ApplyAsync(ttId,
                new TimetableApplyRequest { EffectiveFrom = new DateTime(2026, 1, 1) }, CancellationToken.None),
            Throws.TypeOf<ArgumentException>());
    }
}
