using System.Data;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Models;
using MyPortal.Services.Interfaces;
using MyPortal.Services.People;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Sorting;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class PayScaleServiceTests
{
    private static readonly Guid TermId = Guid.Parse("44444444-0000-4000-8000-000000000001");
    private static readonly Guid MpsScaleId = Guid.Parse("11111111-0000-4000-8000-000000000001");
    private static readonly Guid UpsScaleId = Guid.Parse("11111111-0000-4000-8000-000000000002");
    private static readonly Guid ZoneId = Guid.Parse("33333333-0000-4000-8000-000000000001");
    private static readonly DateTime Source = new(2024, 9, 1);

    private Mock<IPayScalePointRateRepository> _rateRepository = null!;
    private Mock<IPayScalePointRepository> _pointRepository = null!;
    private Mock<IPayScaleRepository> _scaleRepository = null!;
    private Mock<IServiceTermRepository> _serviceTermRepository = null!;
    private PayScaleService _service = null!;

    private ServiceTerm _term = null!;
    private List<PayScale> _scales = null!;
    private List<PayScalePoint> _points = null!;
    private List<PayScalePointRate> _rates = null!;
    private List<PayScaleUsageRow> _pointUsage = null!;
    private List<PayScaleUsageRow> _scaleUsage = null!;

    private List<PayScalePoint> _insertedPoints = null!;
    private List<PayScalePointRate> _insertedRates = null!;
    private List<Guid> _deletedPointIds = null!;

    [SetUp]
    public void Setup()
    {
        _term = new ServiceTerm
        {
            Id = TermId, Code = "NJC", Description = "NJC Green Book",
            SinglePaySpine = false, MinimumPoint = 1m, MaximumPoint = 43m, PointInterval = 1m
        };

        _scales =
        [
            Scale(MpsScaleId, "MPS", "Main Pay Scale", 1m, 3m),
            Scale(UpsScaleId, "UPS", "Upper Pay Scale", 1m, 3m)
        ];
        _points = [];
        _rates = [];
        _pointUsage = [];
        _scaleUsage = [];
        _insertedPoints = [];
        _insertedRates = [];
        _deletedPointIds = [];

        _serviceTermRepository = new Mock<IServiceTermRepository>();
        _serviceTermRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(() => _term);
        _serviceTermRepository
            .Setup(r => r.UpdateAsync(It.IsAny<ServiceTerm>(), It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((ServiceTerm e, CancellationToken _, IDbTransaction? _) => e);

        _scaleRepository = new Mock<IPayScaleRepository>();
        _scaleRepository
            .Setup(r => r.GetListAsync(It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(() => _scales);
        _scaleRepository
            .Setup(r => r.GetContractCountsAsync(It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(() => _scaleUsage);
        _scaleRepository
            .Setup(r => r.InsertAsync(It.IsAny<PayScale>(), It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((PayScale e, CancellationToken _, IDbTransaction? _) => e);
        _scaleRepository
            .Setup(r => r.UpdateAsync(It.IsAny<PayScale>(), It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((PayScale e, CancellationToken _, IDbTransaction? _) => e);
        _scaleRepository
            .Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<bool>(),
                It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(true);

        _pointRepository = new Mock<IPayScalePointRepository>();
        _pointRepository
            .Setup(r => r.GetListAsync(It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(() => _points);
        _pointRepository
            .Setup(r => r.GetContractCountsAsync(It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(() => _pointUsage);
        _pointRepository
            .Setup(r => r.InsertAsync(It.IsAny<PayScalePoint>(), It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction?>()))
            .Callback<PayScalePoint, CancellationToken, IDbTransaction?>((e, _, _) => _insertedPoints.Add(e))
            .ReturnsAsync((PayScalePoint e, CancellationToken _, IDbTransaction? _) => e);
        _pointRepository
            .Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<bool>(),
                It.IsAny<IDbTransaction?>()))
            .Callback<Guid, CancellationToken, bool, IDbTransaction?>((id, _, _, _) => _deletedPointIds.Add(id))
            .ReturnsAsync(true);

        _rateRepository = new Mock<IPayScalePointRateRepository>();
        _rateRepository
            .Setup(r => r.GetListAsync(It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(() => _rates);
        _rateRepository
            .Setup(r => r.InsertAsync(It.IsAny<PayScalePointRate>(), It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction?>()))
            .Callback<PayScalePointRate, CancellationToken, IDbTransaction?>((e, _, _) => _insertedRates.Add(e))
            .ReturnsAsync((PayScalePointRate e, CancellationToken _, IDbTransaction? _) => e);
        _rateRepository
            .Setup(r => r.UpdateAsync(It.IsAny<PayScalePointRate>(), It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((PayScalePointRate e, CancellationToken _, IDbTransaction? _) => e);
        _rateRepository
            .Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<bool>(),
                It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(true);

        var uowFactory = new Mock<IUnitOfWorkFactory>();
        uowFactory.Setup(f => f.BeginAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<IUnitOfWork>().Object);

        var authorization = new Mock<IAuthorizationService>();
        authorization
            .Setup(a => a.HasPermissionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _service = new PayScaleService(
            authorization.Object,
            Mock.Of<ILogger<PayScaleService>>(),
            _scaleRepository.Object,
            _pointRepository.Object,
            _rateRepository.Object,
            Mock.Of<IPayZoneRepository>(),
            Mock.Of<ISchoolRepository>(),
            _serviceTermRepository.Object,
            Mock.Of<IValidationService>(),
            uowFactory.Object);
    }

    private static PayScale Scale(Guid id, string code, string description, decimal? min, decimal? max,
        decimal? interval = 1m) => new()
    {
        Id = id, ServiceTermId = TermId, Code = code, Description = description, Active = true,
        MinimumPoint = min, MaximumPoint = max, PointInterval = interval
    };

    private static PayScalePoint ScalePoint(Guid scaleId, decimal value, string code) => new()
    {
        Id = Guid.NewGuid(), PayScaleId = scaleId, PointValue = value, Code = code, Description = code
    };

    private static PayScalePoint SpinePoint(decimal value, string code) => new()
    {
        Id = Guid.NewGuid(), ServiceTermId = TermId, PointValue = value, Code = code, Description = code
    };

    private static PayScalePointRate Rate(Guid pointId, decimal salary, DateTime? from = null) => new()
    {
        Id = Guid.NewGuid(), PayScalePointId = pointId, PayZoneId = ZoneId,
        EffectiveFrom = from ?? Source, EffectiveTo = null, AnnualSalary = salary
    };

    private static ServiceTermPayUpsertRequest Pay(params PayScaleUpsertItem[] scales) => new()
    {
        EffectiveFrom = Source,
        MinimumPoint = 1m,
        MaximumPoint = 43m,
        PointInterval = 1m,
        Scales = scales.ToList()
    };

    private static PayScaleUpsertItem ScaleItem(Guid? id, string code, decimal? min, decimal? max,
        decimal? interval = 1m, params PointSalaryItem[] salaries) => new()
    {
        Id = id, Code = code, Description = code, Active = true,
        MinimumPoint = min, MaximumPoint = max, PointInterval = interval,
        Salaries = salaries.ToList()
    };

    private static PointSalaryItem Salary(decimal pointValue, decimal amount) =>
        new() { PointValue = pointValue, PayZoneId = ZoneId, AnnualSalary = amount };

    [Test]
    public async Task Update_GeneratesAPointForEveryStepInAScalesRange()
    {
        await _service.UpdateServiceTermPayAsync(TermId, Pay(ScaleItem(MpsScaleId, "MPS", 1m, 5m)),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_insertedPoints.Select(p => p.PointValue), Is.EqualTo(new[] { 1m, 2m, 3m, 4m, 5m }));
            Assert.That(_insertedPoints.Select(p => p.Code),
                Is.EqualTo(new[] { "MPS1", "MPS2", "MPS3", "MPS4", "MPS5" }));
            Assert.That(_insertedPoints, Has.All.Matches<PayScalePoint>(p => p.ServiceTermId == null),
                "a scale that owns its range owns its points");
        });
    }

    [Test]
    public async Task Update_StepsByAFractionalInterval()
    {
        await _service.UpdateServiceTermPayAsync(TermId,
            Pay(ScaleItem(MpsScaleId, "MPS", 1m, 3m, 0.5m)), CancellationToken.None);

        Assert.That(_insertedPoints.Select(p => p.PointValue),
            Is.EqualTo(new[] { 1m, 1.5m, 2m, 2.5m, 3m }));
    }

    [Test]
    public async Task Update_OnASingleSpineTerm_GeneratesPointsOnTheTermNotTheScales()
    {
        var request = Pay(ScaleItem(MpsScaleId, "GR3", 5m, 8m));
        request.SinglePaySpine = true;
        request.MinimumPoint = 5m;
        request.MaximumPoint = 8m;

        await _service.UpdateServiceTermPayAsync(TermId, request, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_insertedPoints.Select(p => p.PointValue), Is.EqualTo(new[] { 5m, 6m, 7m, 8m }));
            Assert.That(_insertedPoints, Has.All.Matches<PayScalePoint>(p => p.ServiceTermId == TermId),
                "the spine belongs to the term, so overlapping grades share its points");
            Assert.That(_insertedPoints, Has.All.Matches<PayScalePoint>(p => p.PayScaleId == null));
        });
    }

    [Test]
    public async Task Update_OnASingleSpineTerm_OverlappingScalesShareOnePointPerValue()
    {
        _scales = [];

        var request = Pay(
            ScaleItem(null, "GR3", 5m, 8m),
            ScaleItem(null, "GR4", 7m, 10m));
        request.SinglePaySpine = true;
        request.MinimumPoint = 5m;
        request.MaximumPoint = 10m;

        await _service.UpdateServiceTermPayAsync(TermId, request, CancellationToken.None);

        Assert.Multiple(() =>
        {
            // 7 and 8 sit in both grades but exist once, so their salary cannot drift apart.
            Assert.That(_insertedPoints.Select(p => p.PointValue),
                Is.EqualTo(new[] { 5m, 6m, 7m, 8m, 9m, 10m }));
            Assert.That(_insertedPoints.Select(p => p.PointValue).Distinct().Count(),
                Is.EqualTo(_insertedPoints.Count));
        });
    }

    [Test]
    public void Update_OnASingleSpineTerm_Throws_WhenAScaleReachesPastTheSpine()
    {
        var request = Pay(ScaleItem(MpsScaleId, "GR3", 5m, 20m));
        request.SinglePaySpine = true;
        request.MinimumPoint = 5m;
        request.MaximumPoint = 10m;

        Assert.That(async () => await _service.UpdateServiceTermPayAsync(TermId, request,
                CancellationToken.None),
            Throws.TypeOf<ValidationException>());
    }

    [Test]
    public async Task Update_ExtendingARange_LeavesExistingPointsAlone()
    {
        _points =
        [
            ScalePoint(MpsScaleId, 1m, "M1"),
            ScalePoint(MpsScaleId, 2m, "M2"),
            ScalePoint(MpsScaleId, 3m, "M3")
        ];

        await _service.UpdateServiceTermPayAsync(TermId, Pay(ScaleItem(MpsScaleId, "MPS", 1m, 5m)),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            // Only the new steps are written; M1-M3 keep their ids, so contracts stay attached.
            Assert.That(_insertedPoints.Select(p => p.PointValue), Is.EqualTo(new[] { 4m, 5m }));
            Assert.That(_deletedPointIds, Is.Empty);
        });
    }

    [Test]
    public void Update_NarrowingARange_Throws_WhenADiscardedPointIsOnAContract()
    {
        var top = ScalePoint(MpsScaleId, 3m, "M3");
        _points = [ScalePoint(MpsScaleId, 1m, "M1"), ScalePoint(MpsScaleId, 2m, "M2"), top];
        _pointUsage = [new PayScaleUsageRow { Id = top.Id, ContractCount = 2 }];

        Assert.That(async () => await _service.UpdateServiceTermPayAsync(TermId,
                Pay(ScaleItem(MpsScaleId, "MPS", 1m, 2m)), CancellationToken.None),
            Throws.TypeOf<EntityInUseException>().With.Message.Contains("M3"));
    }

    [Test]
    public async Task Update_NarrowingARange_DropsUnusedPointsAndTheirSalaries()
    {
        var top = ScalePoint(MpsScaleId, 3m, "M3");
        _points = [ScalePoint(MpsScaleId, 1m, "M1"), ScalePoint(MpsScaleId, 2m, "M2"), top];

        var stranded = Rate(top.Id, 40_000m);
        _rates = [stranded];

        var deletedRateIds = new List<Guid>();
        _rateRepository
            .Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<bool>(),
                It.IsAny<IDbTransaction?>()))
            .Callback<Guid, CancellationToken, bool, IDbTransaction?>((id, _, _, _) => deletedRateIds.Add(id))
            .ReturnsAsync(true);

        await _service.UpdateServiceTermPayAsync(TermId, Pay(ScaleItem(MpsScaleId, "MPS", 1m, 2m)),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_deletedPointIds, Is.EqualTo(new[] { top.Id }));
            Assert.That(deletedRateIds, Does.Contain(stranded.Id),
                "the rate goes first or the point delete breaks the foreign key");
        });
    }

    [Test]
    public async Task Update_WritesSalariesAgainstPointsCreatedInTheSameSave()
    {
        await _service.UpdateServiceTermPayAsync(TermId,
            Pay(ScaleItem(MpsScaleId, "MPS", 1m, 2m, 1m, Salary(1m, 31_650m), Salary(2m, 33_075m))),
            CancellationToken.None);

        var byPoint = _insertedPoints.ToDictionary(p => p.Id, p => p.PointValue);

        Assert.Multiple(() =>
        {
            Assert.That(_insertedRates, Has.Count.EqualTo(2));
            Assert.That(_insertedRates.Select(r => (byPoint[r.PayScalePointId], r.AnnualSalary)),
                Is.EquivalentTo(new[] { (1m, 31_650m), (2m, 33_075m) }));
        });
    }

    [Test]
    public void Update_Throws_WhenTwoScalesShareACode()
    {
        _scales = [];

        Assert.That(async () => await _service.UpdateServiceTermPayAsync(TermId,
                Pay(ScaleItem(null, "MPS", 1m, 3m), ScaleItem(null, "mps", 4m, 6m)),
                CancellationToken.None),
            Throws.TypeOf<ValidationException>());
    }

    [Test]
    public void Update_RemovingAScale_Throws_WhenItIsOnAContract()
    {
        _scaleUsage = [new PayScaleUsageRow { Id = MpsScaleId, ContractCount = 3 }];

        Assert.That(async () => await _service.UpdateServiceTermPayAsync(TermId, Pay(),
                CancellationToken.None),
            Throws.TypeOf<EntityInUseException>());
    }

    [Test]
    public async Task PreviewPayAward_AppliesUplift_RoundedToWholePounds()
    {
        var point = ScalePoint(MpsScaleId, 1m, "M1");
        _points = [point];
        _rates = [Rate(point.Id, 31_650m)];

        var result = await _service.PreviewPayAwardAsync(TermId, Award(2.8m), CancellationToken.None);

        // 31,650 x 1.028 = 32,536.20 -> 32,536
        Assert.Multiple(() =>
        {
            Assert.That(result.Rates.Single().PreviousAnnualSalary, Is.EqualTo(31_650m));
            Assert.That(result.Rates.Single().AnnualSalary, Is.EqualTo(32_536m));
            Assert.That(result.Rates.Single().AnnualSalary % 1, Is.Zero,
                "spine values should stay whole pounds");
        });
    }

    [Test]
    public async Task PreviewPayAward_ScaleOverride_BeatsDefaultPercentage()
    {
        var mps = ScalePoint(MpsScaleId, 1m, "M1");
        var ups = ScalePoint(UpsScaleId, 1m, "U1");
        _points = [mps, ups];
        _rates = [Rate(mps.Id, 31_650m), Rate(ups.Id, 43_607m)];

        var request = Award(2.8m, new PayAwardScaleOverride { PayScaleId = UpsScaleId, Percentage = 5m });

        var result = await _service.PreviewPayAwardAsync(TermId, request, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Rates.Single(r => r.PayScalePointId == mps.Id).AnnualSalary,
                Is.EqualTo(32_536m));
            Assert.That(result.Rates.Single(r => r.PayScalePointId == ups.Id).AnnualSalary,
                Is.EqualTo(45_787m));
        });
    }

    [Test]
    public void ApplyPayAward_Throws_WhenNotAfterLatestGeneration()
    {
        var point = ScalePoint(MpsScaleId, 1m, "M1");
        _points = [point];
        _rates = [Rate(point.Id, 31_650m)];

        var request = Award(2.8m);
        request.EffectiveFrom = Source;

        Assert.That(async () => await _service.ApplyPayAwardAsync(TermId, request, CancellationToken.None),
            Throws.TypeOf<ValidationException>());
    }

    [Test]
    public void PreviewPayAward_Throws_WhenSourceGenerationDoesNotExist()
    {
        var point = ScalePoint(MpsScaleId, 1m, "M1");
        _points = [point];
        _rates = [Rate(point.Id, 31_650m)];

        var request = Award(2.8m);
        request.SourceEffectiveFrom = new DateTime(2020, 1, 1);

        Assert.That(async () => await _service.PreviewPayAwardAsync(TermId, request, CancellationToken.None),
            Throws.TypeOf<ValidationException>());
    }

    [Test]
    public async Task ApplyPayAward_ClosesPreviousGeneration_AndInsertsOpenRows()
    {
        var mps = ScalePoint(MpsScaleId, 1m, "M1");
        var ups = ScalePoint(UpsScaleId, 1m, "U1");
        _points = [mps, ups];
        _rates = [Rate(mps.Id, 31_650m), Rate(ups.Id, 43_607m)];

        var updated = new List<PayScalePointRate>();
        _rateRepository
            .Setup(r => r.UpdateAsync(It.IsAny<PayScalePointRate>(), It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction?>()))
            .Callback<PayScalePointRate, CancellationToken, IDbTransaction?>((e, _, _) => updated.Add(e))
            .ReturnsAsync((PayScalePointRate e, CancellationToken _, IDbTransaction? _) => e);

        await _service.ApplyPayAwardAsync(TermId, Award(2.8m), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(updated, Has.Count.EqualTo(2), "both open rows should be closed");
            Assert.That(updated.Select(r => r.EffectiveTo),
                Is.All.EqualTo(new DateTime(2025, 8, 31)), "closed the day before the new generation");

            Assert.That(_insertedRates, Has.Count.EqualTo(2));
            Assert.That(_insertedRates.Select(r => r.EffectiveFrom), Is.All.EqualTo(new DateTime(2025, 9, 1)));
            Assert.That(_insertedRates.Select(r => r.EffectiveTo), Is.All.Null,
                "the new generation stays open");
        });
    }

    private static PayAwardRequest Award(decimal percentage, params PayAwardScaleOverride[] overrides) => new()
    {
        EffectiveFrom = new DateTime(2025, 9, 1),
        SourceEffectiveFrom = Source,
        DefaultPercentage = percentage,
        ScaleOverrides = overrides.ToList()
    };
}
