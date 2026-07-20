using System.Data;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.School;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.Agencies;
using MyPortal.Services.School;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class SchoolServiceTests
{
    private Mock<ISchoolRepository> _schoolRepository = null!;
    private Mock<IUnitOfWorkFactory> _uowFactory = null!;
    private SchoolService _service = null!;

    [SetUp]
    public void Setup()
    {
        _schoolRepository = new Mock<ISchoolRepository>();
        _uowFactory = new Mock<IUnitOfWorkFactory>();

        _uowFactory.Setup(f => f.BeginAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<IUnitOfWork>().Object);

        _service = new SchoolService(
            Mock.Of<IAuthorizationService>(),
            Mock.Of<ILogger<MyPortal.Services.BaseService>>(),
            _schoolRepository.Object,
            _uowFactory.Object,
            Mock.Of<IAgencyService>());
    }

    [Test]
    public void CreateAsync_Throws_Validation_WhenUrnAlreadyInUse()
    {
        _schoolRepository
            .Setup(r => r.UrnExistsAsync("123456", null, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(true);

        var model = new SchoolUpsertRequest { Name = "Test", Urn = "123456", Uprn = "100000000001" };

        Assert.That(async () => await _service.CreateAsync(model, CancellationToken.None),
            Throws.TypeOf<ValidationException>());

        _schoolRepository.Verify(r => r.InsertAsync(It.IsAny<MyPortal.Core.Entities.School>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    [Test]
    public async Task IsUrnAvailableAsync_ReturnsTrue_ForBlankUrn_WithoutHittingRepo()
    {
        var available = await _service.IsUrnAvailableAsync("  ", null, CancellationToken.None);

        Assert.That(available, Is.True);
        _schoolRepository.Verify(r => r.UrnExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    [Test]
    public async Task IsUrnAvailableAsync_ReturnsFalse_WhenUrnExists()
    {
        _schoolRepository
            .Setup(r => r.UrnExistsAsync("123456", null, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(true);

        var available = await _service.IsUrnAvailableAsync("123456", null, CancellationToken.None);

        Assert.That(available, Is.False);
    }

    [Test]
    public async Task IsUrnAvailableAsync_TrimsAndPassesExcludeId()
    {
        var excludeId = Guid.NewGuid();
        _schoolRepository
            .Setup(r => r.UrnExistsAsync("123456", excludeId, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(false);

        var available = await _service.IsUrnAvailableAsync("  123456  ", excludeId, CancellationToken.None);

        Assert.That(available, Is.True);
        _schoolRepository.Verify(
            r => r.UrnExistsAsync("123456", excludeId, It.IsAny<CancellationToken>(), null), Times.Once);
    }
}
