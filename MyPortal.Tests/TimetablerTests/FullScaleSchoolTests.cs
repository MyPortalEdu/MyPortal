using System.Diagnostics;
using MyPortal.Timetabler.Models;
using MyPortal.Timetabler.Solver;

namespace MyPortal.Tests.TimetablerTests;

/// One line: this is a full-school feasibility test — slow, opt-in via `--filter`.
[TestFixture]
[Category("Slow")]
public class FullScaleSchoolTests
{
    // Subjects taught within tutor-group blocks (one block per (tutorGroup, subject)).
    private static readonly string[] TutorSubjects =
    {
        "Art", "Citizenship", "DT", "Drama", "Geography", "History",
        "ICT", "Music", "PE", "RE"
    };

    // Subjects taught in setted blocks at band level (4 sets per block).
    private static readonly string[] SetSubjects = { "English", "Maths", "Science", "French" };

    // Per-week structure for each subject. DT and PE are doubles per the user's brief.
    private static readonly Dictionary<string, int[]> SlotSizes = new()
    {
        ["English"]     = new[] { 1, 1, 1, 1 },
        ["Maths"]       = new[] { 1, 1, 1, 1 },
        ["Science"]     = new[] { 1, 1, 1 },
        ["French"]      = new[] { 1, 1 },
        ["History"]     = new[] { 1, 1 },
        ["Geography"]   = new[] { 1, 1 },
        ["Art"]         = new[] { 1 },
        ["DT"]          = new[] { 2 },
        ["Drama"]       = new[] { 1 },
        ["ICT"]         = new[] { 1 },
        ["Music"]       = new[] { 1 },
        ["PE"]          = new[] { 2 },
        ["RE"]          = new[] { 1 },
        ["Citizenship"] = new[] { 1 },
    };

    // Generic classroom subjects — non-specialist rooms can host any of these.
    private static readonly HashSet<string> GenericRoomSubjects = new()
    {
        "English", "Maths", "French", "History", "Geography", "RE", "Citizenship"
    };

    private const int YearGroupCount = 5;
    private const int BandsPerYear = 2;
    private const int TutorGroupsPerBand = 4;
    private const int SetsPerSubject = 4;

    [Test]
    public void Solve_FullSchool_ProducesFeasibleSchedule()
    {
        var input = BuildSchool();

        Console.WriteLine($"School scale: " +
                          $"{input.Periods.Count} periods, " +
                          $"{input.Teachers.Count} teachers, " +
                          $"{input.Rooms.Count} rooms, " +
                          $"{input.Bands.Count} bands, " +
                          $"{input.Blocks.Count} blocks, " +
                          $"{input.Blocks.SelectMany(b => b.Groups).SelectMany(g => g.Classes).Count()} classes.");

        // Spread objective on — exercises the pair-penalty formulation at full scale.
        var sw = Stopwatch.StartNew();
        var result = new CpSatTimetableSolver().Solve(input,
            new SolveOptions(MaxSeconds: 600, RandomSeed: 1, MaximiseSpread: true));
        sw.Stop();

        Console.WriteLine($"Solve status: {result.Status} ({result.Diagnostic}) in {sw.Elapsed.TotalSeconds:F1}s. " +
                          $"Assignments: {result.Assignments.Count}.");

        Assert.That(result.Status, Is.AnyOf(SolveStatus.Feasible, SolveStatus.Optimal),
            $"Expected feasible solution, got {result.Status} ({result.Diagnostic}).");

        AssertEveryBlockSlotClassAssigned(input, result);
        AssertNoTeacherOrRoomDoubleBooked(result);
        AssertSessionCountsMatchSpec(input, result);
        AssertDoublesAreConsecutive(input, result);
        AssertSpreadIsReasonable(input, result);
    }

    // ─── builders ────────────────────────────────────────────────────────────

    private static TimetableInput BuildSchool()
    {
        var periods = BuildWeek();
        var teachers = BuildTeachers();
        var rooms = BuildRooms();
        var (blocks, bands) = BuildCurriculum();
        return new TimetableInput(periods, teachers, rooms, bands, blocks, Array.Empty<Pin>());
    }

    private static List<PeriodSlot> BuildWeek()
    {
        // 5 days × 6 periods. Break between P2/P3 and lunch between P4/P5 — encoded as
        // NoDoubleAfter on P2 and P4 so doubles can never bridge those gaps.
        var periods = new List<PeriodSlot>(30);
        for (var day = 0; day < 5; day++)
        for (var p = 1; p <= 6; p++)
        {
            var noDoubleAfter = p == 2 || p == 4;
            periods.Add(new PeriodSlot($"D{day}P{p}", day, p, noDoubleAfter));
        }
        return periods;
    }

    private static List<Teacher> BuildTeachers()
    {
        // Counts roughly sized to ~50% slack over the minimum implied by lesson-period demand
        // for each subject, so the model has room to find a feasible packing rather than
        // teetering on a knife-edge.
        var perSubjectCount = new Dictionary<string, int>
        {
            ["English"]     = 12,
            ["Maths"]       = 12,
            ["Science"]     = 9,
            ["French"]      = 6,
            ["History"]     = 6,
            ["Geography"]   = 6,
            ["DT"]          = 6,
            ["PE"]          = 6,
            ["Art"]         = 3,
            ["Citizenship"] = 3,
            ["Drama"]       = 3,
            ["ICT"]         = 3,
            ["Music"]       = 3,
            ["RE"]          = 3,
        };

        var teachers = new List<Teacher>();
        foreach (var (subject, count) in perSubjectCount)
        {
            for (var i = 1; i <= count; i++)
            {
                teachers.Add(new Teacher($"T_{subject}_{i:D2}", new[] { subject }));
            }
        }
        return teachers;
    }

    private static List<Room> BuildRooms()
    {
        // Generic classrooms cover the seven non-specialist subjects. Specialist rooms are
        // dedicated to their subject. Counts sized so peak concurrent demand fits.
        var rooms = new List<Room>();

        var generic = GenericRoomSubjects.ToArray();
        for (var i = 1; i <= 50; i++) rooms.Add(new Room($"R_Gen{i:D2}", generic));

        for (var i = 1; i <= 8; i++) rooms.Add(new Room($"R_Sci{i}",   new[] { "Science" }));
        for (var i = 1; i <= 5; i++) rooms.Add(new Room($"R_DT{i}",    new[] { "DT" }));
        for (var i = 1; i <= 5; i++) rooms.Add(new Room($"R_PE{i}",    new[] { "PE" }));
        for (var i = 1; i <= 5; i++) rooms.Add(new Room($"R_ICT{i}",   new[] { "ICT" }));
        for (var i = 1; i <= 3; i++) rooms.Add(new Room($"R_Drama{i}", new[] { "Drama" }));
        for (var i = 1; i <= 3; i++) rooms.Add(new Room($"R_Music{i}", new[] { "Music" }));
        for (var i = 1; i <= 3; i++) rooms.Add(new Room($"R_Art{i}",   new[] { "Art" }));

        return rooms;
    }

    private static (List<Block>, List<Band>) BuildCurriculum()
    {
        var blocks = new List<Block>();
        var bands = new List<Band>();

        // Tutor-group letters across both bands of a year, e.g. Y7 has tutors A–H — bands split
        // them in half (A–D in band 1, E–H in band 2).
        for (var yg = 0; yg < YearGroupCount; yg++)
        {
            var year = yg + 7; // Y7..Y11
            for (var b = 0; b < BandsPerYear; b++)
            {
                var band = b + 1;

                // Setted blocks at band level (English, Maths, Science, French — 4 sets each).
                var settedBlockIds = new List<string>();
                foreach (var subject in SetSubjects)
                {
                    var blockId = $"BLK_Y{year}_B{band}_{subject}";
                    var groups = Enumerable.Range(1, SetsPerSubject).Select(set =>
                            new Group(
                                $"GRP_Y{year}_B{band}_{subject}_S{set}",
                                new[]
                                {
                                    new ClassDefinition(
                                        $"CLS_Y{year}_B{band}_{subject}_S{set}", subject)
                                }))
                        .ToArray();
                    blocks.Add(new Block(blockId, SlotSizes[subject], groups));
                    settedBlockIds.Add(blockId);
                }

                // Per-tutor-group blocks (one block per (tutorGroup, subject)).
                for (var t = 0; t < TutorGroupsPerBand; t++)
                {
                    var tutorLetter = (char)('A' + b * TutorGroupsPerBand + t);
                    var tutor = $"Y{year}{tutorLetter}";

                    var tutorBlockIds = new List<string>();
                    foreach (var subject in TutorSubjects)
                    {
                        var blockId = $"BLK_{tutor}_{subject}";
                        var group = new Group(
                            $"GRP_{tutor}_{subject}",
                            new[] { new ClassDefinition($"CLS_{tutor}_{subject}", subject) });
                        blocks.Add(new Block(blockId, SlotSizes[subject], new[] { group }));
                        tutorBlockIds.Add(blockId);
                    }

                    // The student-conflict-free unit is the *tutor group*, not the band:
                    // students in tutor 7A take 7A-Art, 7A-DT, etc., and one set in each of the
                    // band's setted blocks. Encode that as a sub-band per tutor group whose
                    // member blocks are the tutor's ten subject blocks plus the four shared
                    // setted blocks.
                    bands.Add(new Band(
                        $"BAND_{tutor}",
                        tutorBlockIds.Concat(settedBlockIds).ToList()));
                }
            }
        }

        return (blocks, bands);
    }

    // ─── invariants ──────────────────────────────────────────────────────────

    private static void AssertEveryBlockSlotClassAssigned(TimetableInput input, TimetableOutput result)
    {
        var expected = input.Blocks.Sum(b => b.SlotSizes.Count *
                                             b.Groups.Sum(g => g.Classes.Count));
        Assert.That(result.Assignments, Has.Count.EqualTo(expected));
    }

    private static void AssertNoTeacherOrRoomDoubleBooked(TimetableOutput result)
    {
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

    private static void AssertSessionCountsMatchSpec(TimetableInput input, TimetableOutput result)
    {
        // For each class, sum of period counts across all assignments must equal sum of its
        // block's slot sizes.
        var blockBySlotSizes = input.Blocks.ToDictionary(b => b.Id, b => b.SlotSizes);
        var byClass = result.Assignments.GroupBy(a => (a.BlockId, a.ClassId));

        foreach (var grp in byClass)
        {
            var (blockId, classId) = grp.Key;
            var totalPeriods = grp.Sum(a => a.PeriodIds.Count);
            var expected = blockBySlotSizes[blockId].Sum();
            Assert.That(totalPeriods, Is.EqualTo(expected),
                $"Class {classId} got {totalPeriods} periods, expected {expected}.");
        }
    }

    private static void AssertSpreadIsReasonable(TimetableInput input, TimetableOutput result)
    {
        // Sanity-check the spread objective actually did something: no class with multiple
        // slots is allowed to have *all* of them on a single day. (Stronger checks would be
        // brittle — the solver may not reach a perfectly-spread optimum within the budget.)
        var periodById = input.Periods.ToDictionary(p => p.Id);
        var classToBlock = input.Blocks.SelectMany(b => b.Groups.SelectMany(g => g.Classes)
            .Select(c => (c.Id, BlockSlotCount: b.SlotSizes.Count))).ToDictionary(x => x.Id, x => x.BlockSlotCount);

        foreach (var grp in result.Assignments.GroupBy(a => a.ClassId))
        {
            if (classToBlock[grp.Key] < 2) continue; // single-slot classes can't spread

            var daysUsed = grp.Select(a => periodById[a.PeriodIds[0]].Day).Distinct().Count();
            Assert.That(daysUsed, Is.GreaterThan(1),
                $"Class {grp.Key} has all {grp.Count()} slots on a single day — spread objective failed.");
        }
    }

    private static void AssertDoublesAreConsecutive(TimetableInput input, TimetableOutput result)
    {
        var periodById = input.Periods.ToDictionary(p => p.Id);
        foreach (var a in result.Assignments.Where(x => x.PeriodIds.Count > 1))
        {
            for (var i = 0; i < a.PeriodIds.Count - 1; i++)
            {
                var p1 = periodById[a.PeriodIds[i]];
                var p2 = periodById[a.PeriodIds[i + 1]];
                Assert.That(p1.Day, Is.EqualTo(p2.Day),
                    $"Multi-period slot for {a.ClassId} crosses days.");
                Assert.That(p2.OrderInDay, Is.EqualTo(p1.OrderInDay + 1),
                    $"Multi-period slot for {a.ClassId} not consecutive.");
                Assert.That(p1.NoDoubleAfter, Is.False,
                    $"Multi-period slot for {a.ClassId} bridges a no-bridge gap.");
            }
        }
    }
}
