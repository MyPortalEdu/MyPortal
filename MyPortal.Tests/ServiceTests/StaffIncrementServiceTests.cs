using System.Data;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Models;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Providers;
using MyPortal.Services.People;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Sorting;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Tests.ServiceTests;

[TestFixture]
public class StaffIncrementServiceTests
{
    private static readonly Guid TermId = Guid.Parse("44444444-0000-4000-8000-000000000001");
    private static readonly Guid MpsScaleId = Guid.Parse("11111111-0000-4000-8000-000000000001");
    private static readonly Guid ZoneId = Guid.Parse("33333333-0000-4000-8000-000000000001");
    private static readonly DateTime Eff = new(2026, 9, 1);
    private static readonly DateTime Today = new(2026, 7, 23); // matches the fixed date provider

    private ServiceTerm _term = null!;
    private List<IncrementCandidateRow> _candidates = null!;
    private List<PayScalePoint> _points = null!;
    private List<PayScalePointRate> _rates = null!;

    private Mock<IStaffContractRepository> _contractRepository = null!;
    private Mock<IStaffContractSalaryChangeRepository> _salaryChangeRepository = null!;
    private List<Guid> _alreadyIncremented = null!;
    private StaffIncrementService _service = null!;

    // Fixed point ids M1..M6 so candidates and points line up.
    private static readonly Guid[] MpsPointIds =
    [
        Guid.Parse("22222222-0000-4000-8000-000000000001"),
        Guid.Parse("22222222-0000-4000-8000-000000000002"),
        Guid.Parse("22222222-0000-4000-8000-000000000003"),
        Guid.Parse("22222222-0000-4000-8000-000000000004"),
        Guid.Parse("22222222-0000-4000-8000-000000000005"),
        Guid.Parse("22222222-0000-4000-8000-000000000006")
    ];

    [SetUp]
    public void Setup()
    {
        _term = new ServiceTerm
        {
            Id = TermId, Code = "MPS", Description = "Main", SpinalProgression = true, SinglePaySpine = false
        };

        _points = Enumerable.Range(1, 6).Select(n => new PayScalePoint
        {
            Id = MpsPointIds[n - 1], PayScaleId = MpsScaleId, PointValue = n, Code = $"M{n}", Description = $"M{n}"
        }).ToList();

        // Rates for M2..M6 (M1 has one too); FTE 1.0 → salary == rate.
        _rates = Enumerable.Range(1, 6).Select(n => new PayScalePointRate
        {
            Id = Guid.NewGuid(), PayScalePointId = MpsPointIds[n - 1], PayZoneId = ZoneId,
            EffectiveFrom = new DateTime(2024, 9, 1), AnnualSalary = 30_000m + n * 1_000m
        }).ToList();

        _candidates = [Candidate("M1", 1m, 1.0m)];

        var serviceTermRepository = new Mock<IServiceTermRepository>();
        serviceTermRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(() => _term);

        _contractRepository = new Mock<IStaffContractRepository>();
        _contractRepository
            .Setup(r => r.GetIncrementCandidatesAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(() => _candidates);

        _alreadyIncremented = [];

        _salaryChangeRepository = new Mock<IStaffContractSalaryChangeRepository>();
        _salaryChangeRepository
            .Setup(r => r.InsertAsync(It.IsAny<StaffContractSalaryChange>(), It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((StaffContractSalaryChange e, CancellationToken _, IDbTransaction? _) => e);
        _salaryChangeRepository
            .Setup(r => r.GetIncrementedContractIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(() => _alreadyIncremented);

        var pointRepository = new Mock<IPayScalePointRepository>();
        pointRepository
            .Setup(r => r.GetListAsync(It.IsAny<FilterOptions?>(), It.IsAny<SortOptions?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(() => _points);

        var rateRepository = new Mock<IPayScalePointRateRepository>();
        rateRepository
            .Setup(r => r.GetCurrentByZoneAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(() => _rates);

        var schoolRepository = new Mock<ISchoolRepository>();
        schoolRepository.Setup(r => r.GetLocalSchoolPayZoneIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ZoneId);

        var uowFactory = new Mock<IUnitOfWorkFactory>();
        uowFactory.Setup(f => f.BeginAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<IUnitOfWork>().Object);

        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(a => a.RequirePermissionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var dateProvider = new Mock<IDateTimeProvider>();
        dateProvider.SetupGet(d => d.UtcNow).Returns(new DateTime(2026, 7, 23, 9, 0, 0));

        _service = new StaffIncrementService(
            authorization.Object,
            Mock.Of<ILogger<StaffIncrementService>>(),
            serviceTermRepository.Object,
            _contractRepository.Object,
            _salaryChangeRepository.Object,
            Mock.Of<IScheduledIncrementRepository>(),
            pointRepository.Object,
            rateRepository.Object,
            schoolRepository.Object,
            dateProvider.Object,
            Mock.Of<IValidationService>(),
            uowFactory.Object);
    }

    private static IncrementCandidateRow Candidate(string code, decimal value, decimal fte, Guid? id = null) => new()
    {
        ContractId = id ?? Guid.NewGuid(),
        StaffMemberId = Guid.NewGuid(),
        StaffName = "Test Person",
        StaffCode = "TST",
        PayScaleId = MpsScaleId,
        PayScalePointId = Guid.NewGuid(),
        Fte = fte,
        AnnualSalary = 30_000m + value * 1_000m,
        CurrentPointValue = value,
        CurrentPointCode = code,
        ScaleCode = "MPS",
        ScaleDescription = "Main",
        ScaleMaximumPoint = 6m
    };

    [Test]
    public async Task Preview_MovesToTheNextPoint_AndRepricesAtItsRate()
    {
        var result = await _service.PreviewAsync(TermId, new IncrementPreviewRequest { EffectiveFrom = Eff },
            CancellationToken.None);

        var item = result.Items.Single();
        Assert.Multiple(() =>
        {
            Assert.That(item.NextPointCode, Is.EqualTo("M2"));
            Assert.That(item.NextPointValue, Is.EqualTo(2m));
            Assert.That(item.NewSalary, Is.EqualTo(32_000m)); // rate for M2, FTE 1.0
            Assert.That(item.AtMaximum, Is.False);
            Assert.That(result.EligibleCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Preview_ProRatesTheNewSalaryByFte()
    {
        _candidates = [Candidate("M1", 1m, 0.5m)];

        var result = await _service.PreviewAsync(TermId, new IncrementPreviewRequest { EffectiveFrom = Eff },
            CancellationToken.None);

        Assert.That(result.Items.Single().NewSalary, Is.EqualTo(16_000m)); // 32,000 x 0.5
    }

    [Test]
    public async Task Preview_FlagsAtMaximum_WhenOnTheTopPoint()
    {
        _candidates = [Candidate("M6", 6m, 1.0m)];

        var result = await _service.PreviewAsync(TermId, new IncrementPreviewRequest { EffectiveFrom = Eff },
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items.Single().AtMaximum, Is.True);
            Assert.That(result.Items.Single().NextPointId, Is.Null);
            Assert.That(result.EligibleCount, Is.Zero);
        });
    }

    [Test]
    public async Task Preview_StopsAtTheGradeWindow_OnASingleSpineTerm()
    {
        _term.SinglePaySpine = true;
        // Spine owned by the term; grade window caps at 8.
        _points = Enumerable.Range(1, 12).Select(n => new PayScalePoint
        {
            Id = Guid.NewGuid(), ServiceTermId = TermId, PointValue = n, Code = $"SCP{n}", Description = $"SCP{n}"
        }).ToList();
        _rates = _points.Select(p => new PayScalePointRate
        {
            Id = Guid.NewGuid(), PayScalePointId = p.Id, PayZoneId = ZoneId,
            EffectiveFrom = new DateTime(2024, 9, 1), AnnualSalary = 20_000m + p.PointValue * 500m
        }).ToList();

        var atTop = Candidate("SCP8", 8m, 1.0m);
        atTop.ScaleMaximumPoint = 8m; // grade window ends at 8
        _candidates = [atTop];

        var result = await _service.PreviewAsync(TermId, new IncrementPreviewRequest { EffectiveFrom = Eff },
            CancellationToken.None);

        Assert.That(result.Items.Single().AtMaximum, Is.True, "can't progress past the grade's top point");
    }

    [Test]
    public async Task Preview_FlagsAlreadyIncremented_ForThatEffectiveDate()
    {
        var contractId = Guid.NewGuid();
        _candidates = [Candidate("M1", 1m, 1.0m, contractId)];
        _alreadyIncremented = [contractId];

        var result = await _service.PreviewAsync(TermId, new IncrementPreviewRequest { EffectiveFrom = Eff },
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items.Single().AlreadyIncremented, Is.True);
            Assert.That(result.EligibleCount, Is.Zero, "an already-incremented contract isn't eligible again");
        });
    }

    [Test]
    public void Preview_Throws_WhenTermHasNoSpinalProgression()
    {
        _term.SpinalProgression = false;

        Assert.That(async () => await _service.PreviewAsync(TermId,
                new IncrementPreviewRequest { EffectiveFrom = Eff }, CancellationToken.None),
            Throws.TypeOf<ValidationException>());
    }

    [Test]
    public async Task Apply_UpdatesTheContract_AndRecordsSalaryHistory()
    {
        var contractId = Guid.NewGuid();
        _candidates = [Candidate("M1", 1m, 1.0m, contractId)];

        var contract = new StaffContract
        {
            Id = contractId, PayScalePointId = _candidates[0].PayScalePointId, AnnualSalary = 31_000m
        };
        _contractRepository
            .Setup(r => r.GetByIdAsync(contractId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(contract);

        StaffContract? updated = null;
        _contractRepository
            .Setup(r => r.UpdateAsync(It.IsAny<StaffContract>(), It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction?>()))
            .Callback<StaffContract, CancellationToken, IDbTransaction?>((c, _, _) => updated = c)
            .ReturnsAsync((StaffContract c, CancellationToken _, IDbTransaction? _) => c);

        StaffContractSalaryChange? change = null;
        _salaryChangeRepository
            .Setup(r => r.InsertAsync(It.IsAny<StaffContractSalaryChange>(), It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction?>()))
            .Callback<StaffContractSalaryChange, CancellationToken, IDbTransaction?>((s, _, _) => change = s)
            .ReturnsAsync((StaffContractSalaryChange s, CancellationToken _, IDbTransaction? _) => s);

        await _service.ApplyAsync(TermId,
            new IncrementApplyRequest { EffectiveFrom = Today, ContractIds = [contractId] }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(updated!.PayScalePointId, Is.EqualTo(MpsPointIds[1]), "moved to M2");
            Assert.That(updated.AnnualSalary, Is.EqualTo(32_000m));
            Assert.That(change!.OldAnnualSalary, Is.EqualTo(31_000m));
            Assert.That(change.NewAnnualSalary, Is.EqualTo(32_000m));
            Assert.That(change.NewPayScalePointId, Is.EqualTo(MpsPointIds[1]));
        });
    }

    [Test]
    public void Apply_Throws_WhenEffectiveDateIsInTheFuture()
    {
        var contractId = Guid.NewGuid();
        _candidates = [Candidate("M1", 1m, 1.0m, contractId)];

        // 'today' is fixed at 2026-07-23 in Setup.
        var request = new IncrementApplyRequest
        {
            EffectiveFrom = new DateTime(2026, 9, 1), ContractIds = [contractId]
        };

        Assert.That(async () => await _service.ApplyAsync(TermId, request, CancellationToken.None),
            Throws.TypeOf<ValidationException>());
    }

    [Test]
    public async Task Apply_IgnoresContractsNotChosen()
    {
        var a = Candidate("M1", 1m, 1.0m);
        var b = Candidate("M1", 1m, 1.0m);
        _candidates = [a, b];

        _contractRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((Guid id, CancellationToken _, IDbTransaction? _) => new StaffContract { Id = id });
        _contractRepository
            .Setup(r => r.UpdateAsync(It.IsAny<StaffContract>(), It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction?>()))
            .ReturnsAsync((StaffContract c, CancellationToken _, IDbTransaction? _) => c);

        await _service.ApplyAsync(TermId,
            new IncrementApplyRequest { EffectiveFrom = Today, ContractIds = [a.ContractId] }, CancellationToken.None);

        _contractRepository.Verify(
            r => r.UpdateAsync(It.IsAny<StaffContract>(), It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()),
            Times.Once);
    }
}
