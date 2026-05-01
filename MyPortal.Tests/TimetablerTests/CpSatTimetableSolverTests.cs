using MyPortal.Timetabler.Models;
using MyPortal.Timetabler.Solver;

namespace MyPortal.Tests.TimetablerTests;

[TestFixture]
public class CpSatTimetableSolverTests
{
    private const string MathsSubject = "MAT";
    private const string PeSubject    = "PE";

    private CpSatTimetableSolver _solver = null!;

    [SetUp]
    public void Setup()
    {
        _solver = new CpSatTimetableSolver();
    }

    // 5 days x 6 periods = 30 slots. NoDoubleAfter on P3 (lunch) and P5 (end-of-day-ish).
    private static List<PeriodSlot> BuildWeek()
    {
        var periods = new List<PeriodSlot>();
        for (var day = 0; day < 5; day++)
        for (var p = 1; p <= 6; p++)
        {
            var noDoubleAfter = p == 3 || p == 5;
            periods.Add(new PeriodSlot($"D{day}P{p}", day, p, noDoubleAfter));
        }
        return periods;
    }

    private TimetableInput BuildTinyTrust(IReadOnlyList<Pin>? pins = null)
    {
        var periods = BuildWeek();

        var teachers = new List<Teacher>
        {
            new("T_M1", new[] { MathsSubject }),
            new("T_M2", new[] { MathsSubject }),
            new("T_PE1", new[] { PeSubject }),
            new("T_PE2", new[] { PeSubject }),
        };

        var rooms = new List<Room>
        {
            new("R_M1", new[] { MathsSubject }),
            new("R_M2", new[] { MathsSubject }),
            new("R_PE1", new[] { PeSubject }),
        };

        // Y7 maths: two sets, structure [single, single, double] = 4 periods/week.
        var mathsBlock = new Block("BLK_MAT_Y7",
            SlotSizes: new[] { 1, 1, 2 },
            Groups: new[]
            {
                new Group("GRP_MA1", new[] { new ClassDefinition("CLS_MA1", MathsSubject) }),
                new Group("GRP_MA2", new[] { new ClassDefinition("CLS_MA2", MathsSubject) }),
            });

        // Y7 PE: two singles.
        var peBlock = new Block("BLK_PE_Y7",
            SlotSizes: new[] { 1, 1 },
            Groups: new[]
            {
                new Group("GRP_PE1", new[] { new ClassDefinition("CLS_PE1", PeSubject) }),
            });

        var bands = new List<Band>
        {
            new("BAND_Y7", new[] { mathsBlock.Id, peBlock.Id }),
        };

        return new TimetableInput(periods, teachers, rooms, bands,
            new[] { mathsBlock, peBlock },
            pins ?? Array.Empty<Pin>());
    }

    [Test]
    public void Solve_FeasibleSchedule_ProducesAssignmentForEverySlotAndClass()
    {
        var input = BuildTinyTrust();

        var result = _solver.Solve(input, new SolveOptions(MaxSeconds: 10, RandomSeed: 1));

        Assert.That(result.Status, Is.AnyOf(SolveStatus.Feasible, SolveStatus.Optimal),
            $"Expected a solution but got {result.Status} ({result.Diagnostic}).");

        // 3 maths slots × 2 maths classes + 2 PE slots × 1 PE class = 8 assignments.
        Assert.That(result.Assignments, Has.Count.EqualTo(8));
    }

    [Test]
    public void Solve_AssignedTeacher_IsQualifiedForSubject()
    {
        var input = BuildTinyTrust();

        var result = _solver.Solve(input, new SolveOptions(MaxSeconds: 10, RandomSeed: 1));

        Assert.That(result.Status, Is.AnyOf(SolveStatus.Feasible, SolveStatus.Optimal));

        var teacherSubjects = input.Teachers.ToDictionary(t => t.Id, t => t.SubjectIds);
        var classSubjects = input.Blocks.SelectMany(b => b.Groups).SelectMany(g => g.Classes)
            .ToDictionary(c => c.Id, c => c.SubjectId);

        foreach (var a in result.Assignments)
        {
            Assert.That(teacherSubjects[a.TeacherId], Does.Contain(classSubjects[a.ClassId]),
                $"Teacher {a.TeacherId} not qualified for {classSubjects[a.ClassId]}.");
        }
    }

    [Test]
    public void Solve_TeachersAndRooms_AreNeverDoubleBooked()
    {
        var input = BuildTinyTrust();

        var result = _solver.Solve(input, new SolveOptions(MaxSeconds: 10, RandomSeed: 1));

        Assert.That(result.Status, Is.AnyOf(SolveStatus.Feasible, SolveStatus.Optimal));

        var teacherUse = new Dictionary<(string Teacher, string Period), Assignment>();
        var roomUse    = new Dictionary<(string Room,    string Period), Assignment>();

        foreach (var a in result.Assignments)
        foreach (var p in a.PeriodIds)
        {
            Assert.That(teacherUse.ContainsKey((a.TeacherId, p)), Is.False,
                $"Teacher {a.TeacherId} double-booked at {p}.");
            Assert.That(roomUse.ContainsKey((a.RoomId, p)), Is.False,
                $"Room {a.RoomId} double-booked at {p}.");
            teacherUse[(a.TeacherId, p)] = a;
            roomUse[(a.RoomId, p)]    = a;
        }
    }

    [Test]
    public void Solve_DoubleSlots_OccupyConsecutiveSameDayPeriods_AndDoNotBridgeNoBridgeGaps()
    {
        var input = BuildTinyTrust();
        var periodById = input.Periods.ToDictionary(p => p.Id);

        var result = _solver.Solve(input, new SolveOptions(MaxSeconds: 10, RandomSeed: 1));

        Assert.That(result.Status, Is.AnyOf(SolveStatus.Feasible, SolveStatus.Optimal));

        // The maths block's slot index 2 is the size-2 double. Verify both periods in the
        // assignment land on the same day, are consecutive, and the first doesn't carry
        // NoDoubleAfter (which would mean the double bridges a break).
        var doubles = result.Assignments
            .Where(a => a.BlockId == "BLK_MAT_Y7" && a.SlotIndex == 2)
            .ToList();

        Assert.That(doubles, Is.Not.Empty);

        foreach (var a in doubles)
        {
            Assert.That(a.PeriodIds, Has.Count.EqualTo(2));
            var p1 = periodById[a.PeriodIds[0]];
            var p2 = periodById[a.PeriodIds[1]];
            Assert.That(p1.Day, Is.EqualTo(p2.Day));
            Assert.That(p2.OrderInDay, Is.EqualTo(p1.OrderInDay + 1));
            Assert.That(p1.NoDoubleAfter, Is.False);
        }
    }

    [Test]
    public void Solve_BlocksWithinBand_DoNotOverlapInTime()
    {
        var input = BuildTinyTrust();

        var result = _solver.Solve(input, new SolveOptions(MaxSeconds: 10, RandomSeed: 1));

        Assert.That(result.Status, Is.AnyOf(SolveStatus.Feasible, SolveStatus.Optimal));

        // Two blocks under BAND_Y7: maths and PE. A student in Y7 takes one class in each, so
        // the periods occupied by the two blocks must be disjoint across the week.
        var mathsPeriods = result.Assignments.Where(a => a.BlockId == "BLK_MAT_Y7")
            .SelectMany(a => a.PeriodIds).ToHashSet();
        var pePeriods = result.Assignments.Where(a => a.BlockId == "BLK_PE_Y7")
            .SelectMany(a => a.PeriodIds).ToHashSet();

        Assert.That(mathsPeriods.Intersect(pePeriods), Is.Empty);
    }

    [Test]
    public void Solve_HonoursPinnedTeacherAndStartPeriod()
    {
        var pin = new Pin(
            BlockId: "BLK_PE_Y7",
            SlotIndex: 0,
            ClassId: "CLS_PE1",
            TeacherId: "T_PE2",
            StartPeriodId: "D2P4");

        var input = BuildTinyTrust(new[] { pin });

        var result = _solver.Solve(input, new SolveOptions(MaxSeconds: 10, RandomSeed: 1));

        Assert.That(result.Status, Is.AnyOf(SolveStatus.Feasible, SolveStatus.Optimal));

        var pinned = result.Assignments.Single(a => a.BlockId == "BLK_PE_Y7" && a.SlotIndex == 0);
        Assert.That(pinned.TeacherId, Is.EqualTo("T_PE2"));
        Assert.That(pinned.PeriodIds, Is.EqualTo(new[] { "D2P4" }));
    }

    [Test]
    public void Solve_AltCurriculumBlock_MayShareTimeWithSiblingBlock()
    {
        // Add a maths-tutoring alt-curriculum block under the same band — it should be allowed
        // to schedule alongside the standard maths/PE blocks because the students opting in are
        // pulled out of their normal class for that period.
        var periods = BuildWeek();

        var teachers = new List<Teacher>
        {
            new("T_M1", new[] { MathsSubject }),
            new("T_M2", new[] { MathsSubject }),
            new("T_PE1", new[] { PeSubject }),
        };

        var rooms = new List<Room>
        {
            new("R_M1", new[] { MathsSubject }),
            new("R_M2", new[] { MathsSubject }),
            new("R_PE1", new[] { PeSubject }),
        };

        var mathsBlock = new Block("BLK_MAT_Y7",
            SlotSizes: new[] { 1 },
            Groups: new[]
            {
                new Group("GRP_MA1", new[] { new ClassDefinition("CLS_MA1", MathsSubject) }),
            });

        var peBlock = new Block("BLK_PE_Y7",
            SlotSizes: new[] { 1 },
            Groups: new[]
            {
                new Group("GRP_PE1", new[] { new ClassDefinition("CLS_PE1", PeSubject) }),
            });

        var altBlock = new Block("BLK_TUTOR",
            SlotSizes: new[] { 1 },
            Groups: new[]
            {
                new Group("GRP_TUTOR", new[] { new ClassDefinition("CLS_TUTOR", MathsSubject) }),
            },
            AllowsConcurrentScheduling: true);

        // Pin the maths block and the alt-curriculum block to the *same* period — only feasible
        // because the alt-curriculum block is exempt from the band-level NoOverlap. PE is left
        // unpinned and the solver will land it on a non-conflicting slot.
        var pins = new[]
        {
            new Pin("BLK_MAT_Y7", 0, StartPeriodId: "D4P4"),
            new Pin("BLK_TUTOR",  0, StartPeriodId: "D4P4"),
        };

        var input = new TimetableInput(
            periods, teachers, rooms,
            new[] { new Band("BAND_Y7", new[] { mathsBlock.Id, peBlock.Id, altBlock.Id }) },
            new[] { mathsBlock, peBlock, altBlock },
            pins);

        var result = _solver.Solve(input, new SolveOptions(MaxSeconds: 10, RandomSeed: 1));

        Assert.That(result.Status, Is.AnyOf(SolveStatus.Feasible, SolveStatus.Optimal),
            $"Alt-curriculum block should be allowed to overlap with siblings ({result.Diagnostic}).");

        var mathsAtD4P4 = result.Assignments.Single(a => a.BlockId == "BLK_MAT_Y7");
        var tutorAtD4P4 = result.Assignments.Single(a => a.BlockId == "BLK_TUTOR");
        Assert.That(mathsAtD4P4.PeriodIds.Single(), Is.EqualTo("D4P4"));
        Assert.That(tutorAtD4P4.PeriodIds.Single(), Is.EqualTo("D4P4"));
    }

    [Test]
    public void Solve_RejectsModel_WhenSubjectHasNoQualifiedTeacher()
    {
        // PE block but no PE-qualified teacher — solver should reject the model upfront with a
        // diagnostic, not return Infeasible after a wasted search.
        var periods = BuildWeek();
        var teachers = new List<Teacher> { new("T_M1", new[] { MathsSubject }) };
        var rooms = new List<Room> { new("R_PE1", new[] { PeSubject }), new("R_M1", new[] { MathsSubject }) };

        var peBlock = new Block("BLK_PE_Y7",
            SlotSizes: new[] { 1 },
            Groups: new[]
            {
                new Group("GRP_PE1", new[] { new ClassDefinition("CLS_PE1", PeSubject) })
            });

        var input = new TimetableInput(
            periods, teachers, rooms,
            new[] { new Band("BAND_Y7", new[] { peBlock.Id }) },
            new[] { peBlock },
            Array.Empty<Pin>());

        var result = _solver.Solve(input, new SolveOptions(MaxSeconds: 10, RandomSeed: 1));

        Assert.That(result.Status, Is.EqualTo(SolveStatus.ModelInvalid));
        Assert.That(result.Diagnostic, Does.Contain("PE"));
    }
}
