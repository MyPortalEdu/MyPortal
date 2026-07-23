using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.People;
using MyPortal.Services.Interfaces.Providers;
using MyPortal.Services.People;
using FluentValidation;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class StaffMemberServiceTests
{
    private Mock<IStaffMemberRepository> _staffMemberRepository = null!;
    private Mock<IUnitOfWorkFactory> _uowFactory = null!;
    private StaffMemberService _service = null!;

    [SetUp]
    public void Setup()
    {
        _staffMemberRepository = new Mock<IStaffMemberRepository>();
        _uowFactory = new Mock<IUnitOfWorkFactory>();

        _uowFactory.Setup(f => f.BeginAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<IUnitOfWork>().Object);

        _service = new StaffMemberService(
            Mock.Of<IAuthorizationService>(),
            Mock.Of<ILogger<StaffMemberService>>(),
            _staffMemberRepository.Object,
            Mock.Of<IPersonRepository>(),
            Mock.Of<IStaffLineManagerRepository>(),
            Mock.Of<IDateTimeProvider>(),
            Mock.Of<IStaffMemberAccessService>(),
            Mock.Of<IPersonService>(),
            Mock.Of<IPhotoService>(),
            Mock.Of<IValidationService>(),
            _uowFactory.Object);
    }

    [Test]
    public void CreateAsync_Throws_Validation_WhenStaffCodeAlreadyInUse()
    {
        _staffMemberRepository
            .Setup(r => r.CodeExistsAsync("ABC123", null, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(true);

        var model = new StaffBasicDetailsUpsertRequest { Code = "ABC123", FirstName = "A", LastName = "B" };

        Assert.That(async () => await _service.CreateAsync(model, CancellationToken.None),
            Throws.TypeOf<ValidationException>());

        _staffMemberRepository.Verify(r => r.InsertAsync(It.IsAny<MyPortal.Core.Entities.StaffMember>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    [Test]
    public async Task IsCodeAvailableAsync_ReturnsTrue_ForBlankCode_WithoutHittingRepo()
    {
        var available = await _service.IsCodeAvailableAsync("   ", null, CancellationToken.None);

        Assert.That(available, Is.True);
        _staffMemberRepository.Verify(r => r.CodeExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    [Test]
    public async Task IsCodeAvailableAsync_ReturnsFalse_WhenCodeExists()
    {
        _staffMemberRepository
            .Setup(r => r.CodeExistsAsync("ABC123", null, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(true);

        var available = await _service.IsCodeAvailableAsync("ABC123", null, CancellationToken.None);

        Assert.That(available, Is.False);
    }

    [Test]
    public async Task IsCodeAvailableAsync_TrimsAndPassesExcludeId()
    {
        var excludeId = Guid.NewGuid();
        _staffMemberRepository
            .Setup(r => r.CodeExistsAsync("ABC123", excludeId, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(false);

        var available = await _service.IsCodeAvailableAsync("  ABC123  ", excludeId, CancellationToken.None);

        Assert.That(available, Is.True);
        _staffMemberRepository.Verify(
            r => r.CodeExistsAsync("ABC123", excludeId, It.IsAny<CancellationToken>(), null), Times.Once);
    }
}
