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

        // Phased solve: Phase 1 fixes the schedule (slot starts + class picks) with per-subject
        // and per-room-pool cumulatives; Phase 2 does per-slot teacher and room assignment as
        // a per-period bipartite matching. Spread objective in Phase 1.
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
        // Subject staffing — peak concurrency on a tight schedule equals (max sets per band) ×
        // (bands/year) which for setted blocks is 4 × ... per-subject limits. With *exactly*
        // teacherCount = peak concurrency, per-class teacher continuity creates a chromatic-
        // number problem (which classes share any period) that exceeds clique size. Adding
        // ~30% slack on the bottleneck subjects gives the matching room to breathe.
        var perSubjectCount = new Dictionary<string, int>
        {
            ["English"]     = 16,
            ["Maths"]       = 16,
            ["Science"]     = 12,
            ["French"]      = 6,
            ["History"]     = 8,
            ["Geography"]   = 8,
            ["DT"]          = 8,
            ["PE"]          = 8,
            ["Art"]         = 4,
            ["Citizenship"] = 4,
            ["Drama"]       = 4,
            ["ICT"]         = 4,
            ["Music"]       = 4,
            ["RE"]          = 4,
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

        // Sum each tutor group's curriculum into the block-level slot multiset (sorted for
        // multiset comparison). With every tutor group sharing the same 10-subject curriculum,
        // every group in the tutor block has the same session-size profile — which is exactly
        // what the adapter validates.
        var tutorBlockSlotSizes = TutorSubjects
            .SelectMany(s => SlotSizes[s])
            .OrderBy(s => s)
            .ToArray();

        for (var yg = 0; yg < YearGroupCount; yg++)
        {
            var year = yg + 7; // Y7..Y11
            for (var b = 0; b < BandsPerYear; b++)
            {
                var band = b + 1;
                var bandBlockIds = new List<string>();

                // Setted blocks at band level (English, Maths, Science, French — 4 sets each).
                foreach (var subject in SetSubjects)
                {
                    var blockId = $"BLK_Y{year}_B{band}_{subject}";
                    var groups = Enumerable.Range(1, SetsPerSubject).Select(set =>
                            new Group(
                                $"GRP_Y{year}_B{band}_{subject}_S{set}",
                                new[]
                                {
                                    new ClassDefinition(
                                        $"CLS_Y{year}_B{band}_{subject}_S{set}", subject,
                                        SlotSizes[subject])
                                }))
                        .ToArray();
                    blocks.Add(new Block(blockId, SlotSizes[subject], groups));
                    bandBlockIds.Add(blockId);
                }

                // ONE tutor block per band, containing every tutor group in that band-half.
                // Each tutor group gets one Class per subject; the block's slot-size multiset
                // is the union of one group's curriculum (sorted). This collapses the previous
                // "40 separate tutor blocks per band" structure into a single coherent unit.
                var tutorBlockId = $"BLK_Y{year}_B{band}_TUTOR";
                var tutorGroups = Enumerable.Range(0, TutorGroupsPerBand).Select(t =>
                {
                    var tutorLetter = (char)('A' + b * TutorGroupsPerBand + t);
                    var tutor = $"Y{year}{tutorLetter}";
                    var classes = TutorSubjects.Select(subject =>
                            new ClassDefinition(
                                $"CLS_{tutor}_{subject}", subject, SlotSizes[subject]))
                        .ToArray();
                    return new Group($"GRP_{tutor}", classes);
                }).ToArray();
                blocks.Add(new Block(tutorBlockId, tutorBlockSlotSizes, tutorGroups));
                bandBlockIds.Add(tutorBlockId);

                // No more sub-bands. One CurriculumBand per (year, band-half) — its students'
                // conflicts are now expressed via per-group NoOverlap *inside* the tutor block,
                // and the band-level NoOverlap on slot intervals across the band's blocks
                // continues to handle the cross-block (tutor↔setted) conflict.
                bands.Add(new Band($"BAND_Y{year}_B{band}", bandBlockIds));
            }
        }

        return (blocks, bands);
    }

    private static void AssertEveryBlockSlotClassAssigned(TimetableInput input, TimetableOutput result)
    {
        // One assignment per (block, slot, group) — the picked class for that group at that
        // slot. Multi-class groups still produce one row per slot, just with the class varying.
        var expected = input.Blocks.Sum(b => b.SlotSizes.Count * b.Groups.Count);
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
        // Each class's total scheduled periods must equal sum(class.SessionSizes). For
        // single-class-per-group this collapses to "class fills the block"; for multi-class
        // groups it's a per-class subset.
        var classExpected = input.Blocks
            .SelectMany(b => b.Groups.SelectMany(g => g.Classes))
            .ToDictionary(c => c.Id, c => c.SessionSizes.Sum());

        var byClass = result.Assignments.GroupBy(a => a.ClassId);
        foreach (var grp in byClass)
        {
            var totalPeriods = grp.Sum(a => a.PeriodIds.Count);
            var expected = classExpected[grp.Key];
            Assert.That(totalPeriods, Is.EqualTo(expected),
                $"Class {grp.Key} got {totalPeriods} periods, expected {expected}.");
        }
    }

    private static void AssertSpreadIsReasonable(TimetableInput input, TimetableOutput result)
    {
        // The per-class spread objective penalises every same-day pair of a class's sessions,
        // so "all sessions on one day" carries C(N,2) cost — pushed away from unless staffing
        // forces it. At full-school scale with 30% staffing slack we expect zero clumped
        // classes; any nonzero count means the objective lost a tradeoff it shouldn't have.
        var periodById = input.Periods.ToDictionary(p => p.Id);
        var classSessionCount = input.Blocks
            .SelectMany(b => b.Groups.SelectMany(g => g.Classes))
            .ToDictionary(c => c.Id, c => c.SessionSizes.Count);

        var multiSessionClasses = result.Assignments
            .GroupBy(a => a.ClassId)
            .Where(grp => classSessionCount[grp.Key] >= 2)
            .ToList();

        var clumped = multiSessionClasses.Count(grp =>
            grp.Select(a => periodById[a.PeriodIds[0]].Day).Distinct().Count() == 1);

        Assert.That(clumped, Is.EqualTo(0),
            $"{clumped} of {multiSessionClasses.Count} multi-session classes had all sessions on a single day — spread objective looks broken.");
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
