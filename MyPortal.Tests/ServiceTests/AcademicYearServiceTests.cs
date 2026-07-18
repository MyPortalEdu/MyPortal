using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Curriculum;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Curriculum;
using MyPortal.Services.Interfaces;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class AcademicYearServiceTests
{
    private Mock<IAcademicYearRepository> _academicYearRepository = null!;
    private Mock<IUnitOfWorkFactory> _uowFactory = null!;
    private AcademicYearService _service = null!;

    [SetUp]
    public void Setup()
    {
        _academicYearRepository = new Mock<IAcademicYearRepository>();
        _uowFactory = new Mock<IUnitOfWorkFactory>();

        _uowFactory.Setup(f => f.BeginAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<IUnitOfWork>().Object);

        // No existing year overlaps, so the create reaches the copy-from existence guard.
        _academicYearRepository
            .Setup(r => r.HasOverlapAsync(It.IsAny<Guid?>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(false);

        _service = new AcademicYearService(
            Mock.Of<IAuthorizationService>(),
            Mock.Of<ILogger<AcademicYearService>>(),
            _academicYearRepository.Object,
            Mock.Of<IAcademicTermRepository>(),
            Mock.Of<IAttendancePeriodRepository>(),
            Mock.Of<IAttendanceWeekRepository>(),
            Mock.Of<ISchoolHolidayRepository>(),
            Mock.Of<IDiaryEventRepository>(),
            Mock.Of<IStudentGroupRepository>(),
            Mock.Of<IStudentGroupSupervisorRepository>(),
            Mock.Of<IYearGroupRepository>(),
            Mock.Of<IRegGroupRepository>(),
            Mock.Of<IHouseRepository>(),
            _uowFactory.Object,
            Mock.Of<IValidationService>());
    }

    private static AcademicYearUpsertRequest RequestCopyingPeriodsFrom(Guid copyFromId) => new()
    {
        AcademicTerms =
        [
            new() { Name = "T1", StartDate = new DateTime(2026, 9, 1), EndDate = new DateTime(2027, 7, 20) }
        ],
        CopyPeriodsFromAcademicYearId = copyFromId
    };

    [Test]
    public void CreateAcademicYear_Throws_NotFound_WhenCopyFromAcademicYearMissing()
    {
        var copyFromId = Guid.NewGuid();

        _academicYearRepository
            .Setup(r => r.GetByIdAsync(copyFromId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((AcademicYear?)null);

        Assert.That(async () => await _service.CreateAcademicYear(RequestCopyingPeriodsFrom(copyFromId), CancellationToken.None),
            Throws.TypeOf<NotFoundException>());

        _academicYearRepository.Verify(r => r.InsertAsync(It.IsAny<AcademicYear>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }
}
