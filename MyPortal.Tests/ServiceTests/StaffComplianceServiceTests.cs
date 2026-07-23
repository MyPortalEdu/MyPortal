using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.Providers;
using MyPortal.Services.People;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class StaffComplianceServiceTests
{
    private static readonly DateTime Today = new(2026, 7, 23);

    private Mock<IStaffMemberRepository> _staffRepository = null!;
    private (DateTime today, DateTime horizon) _captured;
    private StaffComplianceService _service = null!;

    [SetUp]
    public void Setup()
    {
        _staffRepository = new Mock<IStaffMemberRepository>();
        _staffRepository
            .Setup(r => r.GetComplianceItemsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>(), It.IsAny<System.Data.IDbTransaction?>()))
            .Callback<DateTime, DateTime, CancellationToken, System.Data.IDbTransaction?>(
                (today, horizon, _, _) => _captured = (today, horizon))
            .ReturnsAsync(new List<ComplianceItemResponse>
            {
                Item("Expired"), Item("Expired"),
                Item("ExpiringSoon"),
                Item("Missing"), Item("Missing"), Item("Missing")
            });

        var dateProvider = new Mock<IDateTimeProvider>();
        dateProvider.SetupGet(d => d.UtcNow).Returns(Today.AddHours(9));

        var authorization = new Mock<IAuthorizationService>();
        authorization
            .Setup(a => a.RequirePermissionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _service = new StaffComplianceService(
            authorization.Object,
            Mock.Of<ILogger<StaffComplianceService>>(),
            _staffRepository.Object,
            dateProvider.Object);
    }

    private static ComplianceItemResponse Item(string kind) => new()
    {
        StaffMemberId = Guid.NewGuid(),
        StaffName = "Test Person",
        StaffCode = "TST",
        Category = "Dbs",
        Detail = "DBS certificate",
        Kind = kind
    };

    [Test]
    public async Task GetDashboard_CountsEachKind()
    {
        var result = await _service.GetDashboardAsync(90, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExpiredCount, Is.EqualTo(2));
            Assert.That(result.ExpiringSoonCount, Is.EqualTo(1));
            Assert.That(result.MissingCount, Is.EqualTo(3));
            Assert.That(result.Items, Has.Count.EqualTo(6));
        });
    }

    [Test]
    public async Task GetDashboard_HorizonIsTodayPlusDays()
    {
        await _service.GetDashboardAsync(30, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_captured.today, Is.EqualTo(Today), "date only, time stripped");
            Assert.That(_captured.horizon, Is.EqualTo(Today.AddDays(30)));
        });
    }

    [Test]
    public async Task GetDashboard_ClampsHorizon()
    {
        await _service.GetDashboardAsync(9999, CancellationToken.None);
        Assert.That((_captured.horizon - _captured.today).Days, Is.EqualTo(365));

        await _service.GetDashboardAsync(1, CancellationToken.None);
        Assert.That((_captured.horizon - _captured.today).Days, Is.EqualTo(7));
    }

    [Test]
    public async Task GetDashboard_DefaultsHorizon_WhenNonPositive()
    {
        await _service.GetDashboardAsync(0, CancellationToken.None);
        Assert.That((_captured.horizon - _captured.today).Days, Is.EqualTo(90));
    }
}
