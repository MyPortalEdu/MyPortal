using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using MyPortal.Auth.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Curriculum.Timetable;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Tests.TimetablerTests;

[TestFixture]
public class TimetableMaterialisationServiceTests
{
    private static readonly Guid TimetableId    = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid AcademicYearId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    private Mock<ITimetableRepository> _repository = null!;
    private TimetableMaterialisationService _service = null!;

    private List<Session> _capturedSessions = null!;
    private List<SessionPeriod> _capturedSessionPeriods = null!;
    private List<StaffNonContactAllocation> _capturedAllocations = null!;

    [SetUp]
    public void Setup()
    {
        _repository = new Mock<ITimetableRepository>(MockBehavior.Strict);
        _capturedSessions = new List<Session>();
        _capturedSessionPeriods = new List<SessionPeriod>();
        _capturedAllocations = new List<StaffNonContactAllocation>();

        _repository
            .Setup(r => r.BulkInsertSessionsAsync(It.IsAny<IReadOnlyList<Session>>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .Callback<IReadOnlyList<Session>, CancellationToken, IDbTransaction?>((s, _, _) =>
                _capturedSessions.AddRange(s))
            .Returns(Task.CompletedTask);

        _repository
            .Setup(r => r.BulkInsertSessionPeriodsAsync(It.IsAny<IReadOnlyList<SessionPeriod>>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .Callback<IReadOnlyList<SessionPeriod>, CancellationToken, IDbTransaction?>((sp, _, _) =>
                _capturedSessionPeriods.AddRange(sp))
            .Returns(Task.CompletedTask);

        // Default stubs for the PPA path: no teachers / no allocations. PPA-specific tests
        // override these with their own teacher fixtures.
        _repository
            .Setup(r => r.GetAssignedTeachersAsync(TimetableId, It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<StaffMember>());

        _repository
            .Setup(r => r.BulkInsertNonContactAllocationsAsync(
                It.IsAny<IReadOnlyList<StaffNonContactAllocation>>(),
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .Callback<IReadOnlyList<StaffNonContactAllocation>, CancellationToken, IDbTransaction?>(
                (a, _, _) => _capturedAllocations.AddRange(a))
            .Returns(Task.CompletedTask);

        var auth = new Mock<IAuthorizationService>(MockBehavior.Loose);
        var logger = new Mock<ILogger<TimetableMaterialisationService>>(MockBehavior.Loose);
        _service = new TimetableMaterialisationService(auth.Object, logger.Object, _repository.Object);
    }

    private static AttendancePeriod Period(string id, int hour) => new()
    {
        Id = Guid.Parse(id),
        Name = $"P{hour}",
        AcademicYearId = AcademicYearId,
        CycleDayIndex = 0,
        StartTime = new TimeOnly(hour, 0),
        EndTime = new TimeOnly(hour, 55),
    };

    // 4 consecutive Monday periods.
    private static readonly AttendancePeriod[] DayPeriods =
    {
        Period("11111111-0000-0000-0000-000000000001", 9),
        Period("11111111-0000-0000-0000-000000000002", 10),
        Period("11111111-0000-0000-0000-000000000003", 11),
        Period("11111111-0000-0000-0000-000000000004", 12),
    };

    private void StubRepoWith(params TimetableAssignment[] assignments)
    {
        _repository.Setup(r => r.ListAssignmentsAsync(TimetableId, It.IsAny<CancellationToken>(),
                It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(assignments.ToList());
        _repository.Setup(r => r.GetAttendancePeriodsForAssignmentsAsync(TimetableId,
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(DayPeriods.ToList());
    }

    private static TimetableAssignment Assignment(Guid startPeriodId, int size, Guid? teacherId = null) => new()
    {
        Id = Guid.NewGuid(),
        TimetableId = TimetableId,
        CurriculumBlockId = Guid.NewGuid(),
        SlotIndex = 0,
        ClassId = Guid.NewGuid(),
        TeacherId = teacherId ?? Guid.NewGuid(),
        RoomId = Guid.NewGuid(),
        StartAttendancePeriodId = startPeriodId,
        Size = size,
    };

    [Test]
    public async Task Materialise_NoAssignments_DoesNothing()
    {
        StubRepoWith();
        _repository.Setup(r => r.ListAssignmentsAsync(TimetableId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<TimetableAssignment>());

        await _service.MaterialiseAsync(TimetableId, new DateTime(2026, 9, 1), null,
            CancellationToken.None);

        _repository.Verify(r => r.BulkInsertSessionsAsync(It.IsAny<IReadOnlyList<Session>>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    [Test]
    public async Task Materialise_SingleSlotAssignment_ProducesOneSessionAndOneSessionPeriod()
    {
        var p1 = DayPeriods[0].Id;
        StubRepoWith(Assignment(p1, size: 1));

        await _service.MaterialiseAsync(TimetableId, new DateTime(2026, 9, 1), null,
            CancellationToken.None);

        Assert.That(_capturedSessions, Has.Count.EqualTo(1));
        Assert.That(_capturedSessionPeriods, Has.Count.EqualTo(1));
        Assert.That(_capturedSessionPeriods[0].PeriodId, Is.EqualTo(p1));
        Assert.That(_capturedSessionPeriods[0].SessionId, Is.EqualTo(_capturedSessions[0].Id));
    }

    [Test]
    public async Task Materialise_DoubleSlot_EmitsTwoConsecutiveSessionPeriods()
    {
        var p1 = DayPeriods[0].Id;
        var p2 = DayPeriods[1].Id;
        StubRepoWith(Assignment(p1, size: 2));

        await _service.MaterialiseAsync(TimetableId, new DateTime(2026, 9, 1), null,
            CancellationToken.None);

        Assert.That(_capturedSessions, Has.Count.EqualTo(1));
        Assert.That(_capturedSessionPeriods.Select(sp => sp.PeriodId),
            Is.EqualTo(new[] { p1, p2 }));
        // All session periods belong to the same Session.
        Assert.That(_capturedSessionPeriods.Select(sp => sp.SessionId).Distinct().Count(),
            Is.EqualTo(1));
    }

    [Test]
    public async Task Materialise_TripleSlot_WalksThreePeriodsForward()
    {
        var p2 = DayPeriods[1].Id;
        var p3 = DayPeriods[2].Id;
        var p4 = DayPeriods[3].Id;
        StubRepoWith(Assignment(p2, size: 3));

        await _service.MaterialiseAsync(TimetableId, new DateTime(2026, 9, 1), null,
            CancellationToken.None);

        Assert.That(_capturedSessionPeriods.Select(sp => sp.PeriodId),
            Is.EqualTo(new[] { p2, p3, p4 }));
    }

    [Test]
    public void Materialise_SizeRunsOffEndOfDay_Throws()
    {
        // Day has 4 periods total. Starting a triple at the third period would need P3, P4, P5 —
        // there's no P5, so we can't satisfy the size. Defensive: solver shouldn't produce this,
        // but if it did, surface it instead of silently dropping a period.
        var p3 = DayPeriods[2].Id;
        StubRepoWith(Assignment(p3, size: 3));

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.MaterialiseAsync(TimetableId, new DateTime(2026, 9, 1), null,
                CancellationToken.None));
    }

    [Test]
    public async Task Materialise_SetsSessionDates_FromInputAndDefaultsEndToOneYear()
    {
        var p1 = DayPeriods[0].Id;
        StubRepoWith(Assignment(p1, size: 1));
        var start = new DateTime(2026, 9, 1);

        await _service.MaterialiseAsync(TimetableId, start, endDate: null, CancellationToken.None);

        Assert.That(_capturedSessions[0].StartDate, Is.EqualTo(start));
        // Default fallback when EffectiveTo is null — placeholder, admin can adjust.
        Assert.That(_capturedSessions[0].EndDate, Is.EqualTo(start.AddYears(1)));
    }

    [Test]
    public async Task Materialise_HonoursExplicitEndDate()
    {
        var p1 = DayPeriods[0].Id;
        StubRepoWith(Assignment(p1, size: 1));
        var start = new DateTime(2026, 9, 1);
        var end = new DateTime(2026, 12, 20);

        await _service.MaterialiseAsync(TimetableId, start, end, CancellationToken.None);

        Assert.That(_capturedSessions[0].EndDate, Is.EqualTo(end));
    }

    // ─── PPA allocation ─────────────────────────────────────────────────────

    private static StaffMember Teacher(Guid id, int ppa) => new()
    {
        Id = id, Code = $"T{id:N}".Substring(0, 4), IsTeachingStaff = true, PpaPeriodsPerWeek = ppa,
    };

    [Test]
    public async Task Materialise_PpaAllocations_FillFreePeriodsForTeacherWithPpa()
    {
        // 4 periods in the day. Teacher T1 teaches P1 (size=1) → 3 free periods. PPA=2 means
        // we should emit allocations for P2 and P3 (the first two free chronological slots).
        var teacherId = Guid.NewGuid();
        var p1 = DayPeriods[0].Id;
        var p2 = DayPeriods[1].Id;
        var p3 = DayPeriods[2].Id;

        var assignment = Assignment(p1, size: 1, teacherId: teacherId);
        StubRepoWith(assignment);

        _repository.Setup(r => r.GetAssignedTeachersAsync(TimetableId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<StaffMember> { Teacher(teacherId, ppa: 2) });

        await _service.MaterialiseAsync(TimetableId, new DateTime(2026, 9, 1), null,
            CancellationToken.None);

        Assert.That(_capturedAllocations, Has.Count.EqualTo(2));
        Assert.That(_capturedAllocations.Select(a => a.AttendancePeriodId), Is.EqualTo(new[] { p2, p3 }));
        Assert.That(_capturedAllocations.All(a => a.StaffMemberId == teacherId));
        Assert.That(_capturedAllocations.All(a => a.Code == "PPA"));
    }

    [Test]
    public async Task Materialise_PpaSkipsPeriodsTeacherIsTeaching()
    {
        // Teacher teaches P2 (single). PPA=2 across 4 periods → free periods are P1, P3, P4.
        // First two free slots in chronological order = P1, P3.
        var teacherId = Guid.NewGuid();
        var p1 = DayPeriods[0].Id;
        var p2 = DayPeriods[1].Id;
        var p3 = DayPeriods[2].Id;

        var assignment = Assignment(p2, size: 1, teacherId: teacherId);
        StubRepoWith(assignment);

        _repository.Setup(r => r.GetAssignedTeachersAsync(TimetableId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<StaffMember> { Teacher(teacherId, ppa: 2) });

        await _service.MaterialiseAsync(TimetableId, new DateTime(2026, 9, 1), null,
            CancellationToken.None);

        Assert.That(_capturedAllocations.Select(a => a.AttendancePeriodId),
            Is.EqualTo(new[] { p1, p3 }));
    }

    [Test]
    public async Task Materialise_PpaCapsAtConfiguredCount_EvenWithMoreFreePeriods()
    {
        // 4 free periods, PPA=1 → only one allocation emitted, the first chronological one.
        var teacherId = Guid.NewGuid();

        // No assignments for this teacher — they have a full week free.
        _repository.Setup(r => r.ListAssignmentsAsync(TimetableId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<TimetableAssignment>
                { Assignment(DayPeriods[0].Id, size: 1, teacherId: Guid.NewGuid()) });
        _repository.Setup(r => r.GetAttendancePeriodsForAssignmentsAsync(TimetableId,
                It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(DayPeriods.ToList());
        _repository.Setup(r => r.GetAssignedTeachersAsync(TimetableId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<StaffMember> { Teacher(teacherId, ppa: 1) });

        await _service.MaterialiseAsync(TimetableId, new DateTime(2026, 9, 1), null,
            CancellationToken.None);

        Assert.That(_capturedAllocations, Has.Count.EqualTo(1));
        Assert.That(_capturedAllocations[0].AttendancePeriodId, Is.EqualTo(DayPeriods[0].Id));
    }

    [Test]
    public async Task Materialise_NoPpa_WhenTeacherPpaIsZero()
    {
        var teacherId = Guid.NewGuid();
        var assignment = Assignment(DayPeriods[0].Id, size: 1, teacherId: teacherId);
        StubRepoWith(assignment);

        _repository.Setup(r => r.GetAssignedTeachersAsync(TimetableId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<StaffMember> { Teacher(teacherId, ppa: 0) });

        await _service.MaterialiseAsync(TimetableId, new DateTime(2026, 9, 1), null,
            CancellationToken.None);

        // Teacher with PPA=0 produces no allocations. The bulk-insert should never be called.
        Assert.That(_capturedAllocations, Is.Empty);
        _repository.Verify(r => r.BulkInsertNonContactAllocationsAsync(
            It.IsAny<IReadOnlyList<StaffNonContactAllocation>>(),
            It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()), Times.Never);
    }

    [Test]
    public async Task Materialise_PpaInheritsTimetableDates()
    {
        var teacherId = Guid.NewGuid();
        var assignment = Assignment(DayPeriods[0].Id, size: 1, teacherId: teacherId);
        StubRepoWith(assignment);

        _repository.Setup(r => r.GetAssignedTeachersAsync(TimetableId, It.IsAny<CancellationToken>(), It.IsAny<IDbTransaction?>()))
            .ReturnsAsync(new List<StaffMember> { Teacher(teacherId, ppa: 1) });

        var start = new DateTime(2026, 9, 1);
        var end = new DateTime(2026, 12, 20);
        await _service.MaterialiseAsync(TimetableId, start, end, CancellationToken.None);

        Assert.That(_capturedAllocations[0].StartDate, Is.EqualTo(start));
        Assert.That(_capturedAllocations[0].EndDate, Is.EqualTo(end));
        Assert.That(_capturedAllocations[0].TimetableId, Is.EqualTo(TimetableId));
    }
}
