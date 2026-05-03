using MyPortal.Core.Entities;
using MyPortal.Data.Timetabler;
using MyPortal.Services.Timetable;
using MyPortal.Timetabler.Solver;
using Directory = MyPortal.Core.Entities.Directory;

namespace MyPortal.Tests.TimetablerTests;

[TestFixture]
public class TimetableInputBuilderTests
{
    private TimetableInputBuilder _builder = null!;

    [SetUp]
    public void Setup() => _builder = new TimetableInputBuilder();

    // ─── period derivation ──────────────────────────────────────────────────

    [Test]
    public void Build_OrdersPeriodsByDayThenStartTime_AndAssignsOrderInDay()
    {
        var fx = MinimalFixture();
        // Two days, P1/P2 each, supplied in shuffled order to prove ordering happens.
        fx.Periods = new[]
        {
            Period("D1P2", DayOfWeek.Tuesday, "10:00", "10:55"),
            Period("D0P1", DayOfWeek.Monday, "09:00", "09:55"),
            Period("D0P2", DayOfWeek.Monday, "10:00", "10:55"),
            Period("D1P1", DayOfWeek.Tuesday, "09:00", "09:55"),
        };

        var result = _builder.Build(fx.ToSources());

        Assert.That(result.Periods.Select(p => p.Id),
            Is.EqualTo(new[]
            {
                fx.Periods[1].Id.ToString(), // Mon P1
                fx.Periods[2].Id.ToString(), // Mon P2
                fx.Periods[3].Id.ToString(), // Tue P1
                fx.Periods[0].Id.ToString(), // Tue P2
            }));
        Assert.That(result.Periods.Select(p => p.OrderInDay), Is.EqualTo(new[] { 1, 2, 1, 2 }));
    }

    [Test]
    public void Build_NoDoubleAfter_IsTrue_WhenNextPeriodHasGap()
    {
        var fx = MinimalFixture();
        // P1 ends 9:55, P2 starts 10:00 → 5min gap → P1.NoDoubleAfter=true.
        // P2 ends 10:55, P3 starts 10:55 → no gap → P2.NoDoubleAfter=false.
        fx.Periods = new[]
        {
            Period("P1", DayOfWeek.Monday, "09:00", "09:55"),
            Period("P2", DayOfWeek.Monday, "10:00", "10:55"),
            Period("P3", DayOfWeek.Monday, "10:55", "11:50"),
        };

        var result = _builder.Build(fx.ToSources());

        Assert.That(result.Periods[0].NoDoubleAfter, Is.True,  "Gap between P1 and P2 ⇒ no double bridges it.");
        Assert.That(result.Periods[1].NoDoubleAfter, Is.False, "P2 → P3 with no gap ⇒ doubles allowed.");
        Assert.That(result.Periods[2].NoDoubleAfter, Is.True,  "Last period of day always NoDoubleAfter.");
    }

    // ─── teachers / rooms ───────────────────────────────────────────────────

    [Test]
    public void Build_Teachers_OnlyIncludesTeachingStaff_WithSubjectsAndPpa()
    {
        var fx = MinimalFixture();
        var teaching = Staff("T_Teaching", isTeaching: true, ppa: 4);
        var nonTeaching = Staff("T_Office", isTeaching: false);
        fx.Teachers = new[] { teaching, nonTeaching };
        fx.StaffSubjects = new[]
        {
            new SubjectStaffMember { Id = Guid.NewGuid(), StaffMemberId = teaching.Id, SubjectId = fx.MathsSubjectId },
        };

        var result = _builder.Build(fx.ToSources());

        Assert.That(result.Teachers, Has.Count.EqualTo(1));
        Assert.That(result.Teachers[0].Id, Is.EqualTo(teaching.Id.ToString()));
        Assert.That(result.Teachers[0].PpaPeriodsPerWeek, Is.EqualTo(4));
        Assert.That(result.Teachers[0].SubjectIds, Does.Contain(fx.MathsSubjectId.ToString()));
    }

    [Test]
    public void Build_Rooms_CarrySubjectSuitabilityFromSubjectRooms()
    {
        var fx = MinimalFixture();
        var labRoom = Room("Lab1");
        fx.Rooms = new[] { fx.Rooms.Single(), labRoom };
        var scienceSubject = Guid.NewGuid();
        fx.RoomSubjects = new[]
        {
            fx.RoomSubjects.Single(),
            new SubjectRoom { Id = Guid.NewGuid(), RoomId = labRoom.Id, SubjectId = scienceSubject },
        };

        var result = _builder.Build(fx.ToSources());

        var lab = result.Rooms.Single(r => r.Id == labRoom.Id.ToString());
        Assert.That(lab.SubjectIds, Does.Contain(scienceSubject.ToString()));
    }

    // ─── blocks / slot sizes ────────────────────────────────────────────────

    [Test]
    public void Build_BlockSlotSizes_DerivedFromGroupSessionsTimesSessionTypeLength()
    {
        var fx = MinimalFixture();
        // Override the default single slot to: 3 singles + 1 double.
        var single = fx.SessionTypes.Single(s => s.Length == 1);
        var doubleType = new SessionType { Id = Guid.NewGuid(), Length = 2, Description = "Double" };
        fx.SessionTypes = new[] { single, doubleType };
        var group = fx.Groups.Single();
        fx.GroupSessions = new[]
        {
            new CurriculumGroupSession
                { Id = Guid.NewGuid(), CurriculumGroupId = group.Id, SubjectId = fx.MathsSubjectId,
                  SessionTypeId = single.Id,     SessionAmount = 3 },
            new CurriculumGroupSession
                { Id = Guid.NewGuid(), CurriculumGroupId = group.Id, SubjectId = fx.MathsSubjectId,
                  SessionTypeId = doubleType.Id, SessionAmount = 1 },
        };

        var result = _builder.Build(fx.ToSources());

        Assert.That(result.Blocks.Single().SlotSizes, Is.EqualTo(new[] { 1, 1, 1, 2 }));
    }

    // ─── pins ───────────────────────────────────────────────────────────────

    [Test]
    public void Build_Pins_CarryAllOptionalFields()
    {
        var fx = MinimalFixture();
        var teacherId = fx.Teachers.Single().Id;
        var roomId = fx.Rooms.Single().Id;
        var periodId = fx.Periods[0].Id;
        var classId = fx.Classes.Single().Id;
        fx.Pins = new[]
        {
            new TimetablePin
            {
                Id = Guid.NewGuid(),
                CurriculumBlockId = fx.Blocks.Single().Id,
                SlotIndex = 0,
                ClassId = classId,
                TeacherId = teacherId,
                RoomId = roomId,
                StartAttendancePeriodId = periodId,
            }
        };

        var result = _builder.Build(fx.ToSources());

        var pin = result.Pins.Single();
        Assert.That(pin.BlockId,       Is.EqualTo(fx.Blocks.Single().Id.ToString()));
        Assert.That(pin.SlotIndex,     Is.EqualTo(0));
        Assert.That(pin.ClassId,       Is.EqualTo(classId.ToString()));
        Assert.That(pin.TeacherId,     Is.EqualTo(teacherId.ToString()));
        Assert.That(pin.RoomId,        Is.EqualTo(roomId.ToString()));
        Assert.That(pin.StartPeriodId, Is.EqualTo(periodId.ToString()));
    }

    // ─── validation ─────────────────────────────────────────────────────────

    [Test]
    public void Build_Throws_WhenSubjectHasNoQualifiedTeacher()
    {
        var fx = MinimalFixture();
        fx.StaffSubjects = Array.Empty<SubjectStaffMember>();

        Assert.Throws<InvalidOperationException>(() => _builder.Build(fx.ToSources()));
    }

    [Test]
    public void Build_Throws_WhenSubjectHasNoSuitableRoom()
    {
        var fx = MinimalFixture();
        fx.RoomSubjects = Array.Empty<SubjectRoom>();

        Assert.Throws<InvalidOperationException>(() => _builder.Build(fx.ToSources()));
    }

    // ─── end-to-end smoke ───────────────────────────────────────────────────

    [Test]
    public void Build_OutputFeedsSolverToFeasibleSolution()
    {
        var fx = MinimalFixture();

        var input = _builder.Build(fx.ToSources());
        var result = new CpSatTimetableSolver().Solve(input,
            new SolveOptions(MaxSeconds: 10, RandomSeed: 1, MaximiseSpread: false));

        Assert.That(result.Status, Is.AnyOf(
            MyPortal.Timetabler.Models.SolveStatus.Feasible,
            MyPortal.Timetabler.Models.SolveStatus.Optimal));
        Assert.That(result.Assignments, Is.Not.Empty);
    }

    // ─── fixture builders ───────────────────────────────────────────────────

    private static AttendancePeriod Period(string name, DayOfWeek day, string start, string end) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Weekday = day,
            StartTime = TimeOnly.Parse(start),
            EndTime = TimeOnly.Parse(end),
            WeekPatternId = Guid.NewGuid(),
        };

    private static StaffMember Staff(string code, bool isTeaching, int ppa = 0) =>
        new() { Id = Guid.NewGuid(), Code = code, IsTeachingStaff = isTeaching, PpaPeriodsPerWeek = ppa };

    private static Room Room(string name) =>
        new() { Id = Guid.NewGuid(), Name = name, MaxGroupSize = 30 };

    private sealed class Fixture
    {
        public IReadOnlyList<AttendancePeriod>             Periods       = Array.Empty<AttendancePeriod>();
        public IReadOnlyList<StaffMember>                  Teachers      = Array.Empty<StaffMember>();
        public IReadOnlyList<SubjectStaffMember>           StaffSubjects = Array.Empty<SubjectStaffMember>();
        public IReadOnlyList<MyPortal.Core.Entities.Room>  Rooms         = Array.Empty<MyPortal.Core.Entities.Room>();
        public IReadOnlyList<SubjectRoom>                  RoomSubjects  = Array.Empty<SubjectRoom>();
        public IReadOnlyList<CurriculumBand>               Bands         = Array.Empty<CurriculumBand>();
        public IReadOnlyList<CurriculumBandBlockAssignment> BandBlocks   = Array.Empty<CurriculumBandBlockAssignment>();
        public IReadOnlyList<CurriculumBlock>              Blocks        = Array.Empty<CurriculumBlock>();
        public IReadOnlyList<CurriculumGroup>              Groups        = Array.Empty<CurriculumGroup>();
        public IReadOnlyList<CurriculumGroupSession>       GroupSessions = Array.Empty<CurriculumGroupSession>();
        public IReadOnlyList<SessionType>                  SessionTypes  = Array.Empty<SessionType>();
        public IReadOnlyList<Class>                        Classes       = Array.Empty<Class>();
        public IReadOnlyList<Course>                       Courses       = Array.Empty<Course>();
        public IReadOnlyList<TimetablePin>                 Pins          = Array.Empty<TimetablePin>();

        public Guid MathsSubjectId;

        public TimetableInputSources ToSources() => new(
            Periods, Teachers, StaffSubjects, Rooms, RoomSubjects,
            Bands, BandBlocks, Blocks, Groups, GroupSessions, SessionTypes,
            Classes, Courses, Pins);
    }

    /// One band, one block (one group, one class), one teacher (Maths-qualified, no PPA),
    /// one room (Maths-suitable). The smallest input that produces a valid TimetableInput.
    private static Fixture MinimalFixture()
    {
        var maths = Guid.NewGuid();
        var teacher = Staff("T1", isTeaching: true);
        var room = Room("R1");
        var single = new SessionType { Id = Guid.NewGuid(), Length = 1, Description = "Single" };
        var band = new CurriculumBand
        {
            Id = Guid.NewGuid(), AcademicYearId = Guid.NewGuid(),
            CurriculumYearGroupId = Guid.NewGuid(), StudentGroupId = Guid.NewGuid()
        };
        var block = new CurriculumBlock { Id = Guid.NewGuid(), Code = "B1" };
        var bandBlock = new CurriculumBandBlockAssignment
            { Id = Guid.NewGuid(), BandId = band.Id, BlockId = block.Id };
        var group = new CurriculumGroup { Id = Guid.NewGuid(), BlockId = block.Id, StudentGroupId = Guid.NewGuid() };
        var course = new Course { Id = Guid.NewGuid(), Name = "Maths", SubjectId = maths };
        var cls = new Class
        {
            Id = Guid.NewGuid(), Code = "C1",
            CourseId = course.Id, CurriculumGroupId = group.Id,
            DirectoryId = Guid.NewGuid(),
        };
        var groupSession = new CurriculumGroupSession
        {
            Id = Guid.NewGuid(), CurriculumGroupId = group.Id, SubjectId = maths,
            SessionTypeId = single.Id, SessionAmount = 1,
        };

        return new Fixture
        {
            MathsSubjectId = maths,
            Periods = new[] { Period("P1", DayOfWeek.Monday, "09:00", "09:55") },
            Teachers = new[] { teacher },
            StaffSubjects = new[]
            {
                new SubjectStaffMember
                    { Id = Guid.NewGuid(), StaffMemberId = teacher.Id, SubjectId = maths },
            },
            Rooms = new[] { room },
            RoomSubjects = new[]
            {
                new SubjectRoom { Id = Guid.NewGuid(), RoomId = room.Id, SubjectId = maths },
            },
            Bands = new[] { band },
            BandBlocks = new[] { bandBlock },
            Blocks = new[] { block },
            Groups = new[] { group },
            GroupSessions = new[] { groupSession },
            SessionTypes = new[] { single },
            Classes = new[] { cls },
            Courses = new[] { course },
        };
    }
}
