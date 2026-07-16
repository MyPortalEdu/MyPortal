using MyPortal.Core.Entities;
using MyPortal.Data.Timetabler;
using MyPortal.Timetabler.Models;
using SolverRoom = MyPortal.Timetabler.Models.Room;
using SolverTeacher = MyPortal.Timetabler.Models.Teacher;

namespace MyPortal.Services.Curriculum.Timetable;

/// Pure-function adapter from domain entities to TimetableInput. The loading strategy
/// (Dapper, EF, cached) lives outside; this class only converts already-loaded entities so it
/// can be unit-tested without a database.
public class TimetableInputBuilder
{
    public TimetableInput Build(TimetableInputSources sources)
    {
        var periods = BuildPeriods(sources.Periods);
        var teachers = BuildTeachers(sources);
        var rooms = BuildRooms(sources);
        var (blocks, classSubjects) = BuildBlocks(sources);
        var bands = BuildBands(sources);
        var pins = BuildPins(sources);

        // Sanity: every class subject we've referenced must have ≥ 1 qualified teacher and
        // ≥ 1 suitable room, otherwise the solver will reject the model. Catch it here with a
        // clearer error than CP-SAT's diagnostic.
        var teacherSubjectCoverage = teachers.SelectMany(t => t.SubjectIds).ToHashSet();
        var roomSubjectCoverage    = rooms.SelectMany(r => r.SubjectIds).ToHashSet();
        foreach (var (classId, subjectId) in classSubjects)
        {
            if (!teacherSubjectCoverage.Contains(subjectId))
                throw new InvalidOperationException(
                    $"No teaching staff qualified for subject '{subjectId}' (class {classId}).");
            if (!roomSubjectCoverage.Contains(subjectId))
                throw new InvalidOperationException(
                    $"No room marked suitable for subject '{subjectId}' (class {classId}).");
        }

        return new TimetableInput(periods, teachers, rooms, bands, blocks, pins);
    }

    private static List<PeriodSlot> BuildPeriods(IReadOnlyList<AttendancePeriod> entities)
    {
        if (entities.Count == 0) throw new InvalidOperationException("No attendance periods supplied.");

        // Order chronologically so OrderInDay falls out of position; NoDoubleAfter is true
        // when the next period in the same day starts after this one ends (i.e., a gap —
        // break, lunch, or transit). Last period of each day is always NoDoubleAfter.
        var ordered = entities
            .OrderBy(p => p.CycleDayIndex)
            .ThenBy(p => p.StartTime)
            .ToArray();

        var result = new List<PeriodSlot>(ordered.Length);
        for (var i = 0; i < ordered.Length; i++)
        {
            var p = ordered[i];
            var sameDayIndex = ordered.Take(i).Count(x => x.CycleDayIndex == p.CycleDayIndex) + 1;

            var nextSameDay = i + 1 < ordered.Length && ordered[i + 1].CycleDayIndex == p.CycleDayIndex
                ? ordered[i + 1]
                : null;
            var noDoubleAfter = nextSameDay is null || nextSameDay.StartTime > p.EndTime;

            result.Add(new PeriodSlot(p.Id.ToString(), p.CycleDayIndex, sameDayIndex, noDoubleAfter));
        }
        return result;
    }

    private static List<SolverTeacher> BuildTeachers(TimetableInputSources sources)
    {
        var subjectsByStaff = sources.StaffSubjects
            .GroupBy(ss => ss.StaffMemberId)
            .ToDictionary(g => g.Key, g => (IReadOnlyCollection<string>)
                g.Select(ss => ss.SubjectId.ToString()).Distinct().ToArray());

        return sources.Teachers
            .Where(s => s.IsTeachingStaff)
            .Select(s => new SolverTeacher(
                s.Id.ToString(),
                subjectsByStaff.TryGetValue(s.Id, out var subj) ? subj : Array.Empty<string>(),
                s.PpaPeriodsPerWeek))
            .ToList();
    }

    private static List<SolverRoom> BuildRooms(TimetableInputSources sources)
    {
        var subjectsByRoom = sources.RoomSubjects
            .GroupBy(rs => rs.RoomId)
            .ToDictionary(g => g.Key, g => (IReadOnlyCollection<string>)
                g.Select(rs => rs.SubjectId.ToString()).Distinct().ToArray());

        return sources.Rooms
            .Select(r => new SolverRoom(
                r.Id.ToString(),
                subjectsByRoom.TryGetValue(r.Id, out var subj) ? subj : Array.Empty<string>()))
            .ToList();
    }

    private static (List<Block> Blocks, Dictionary<string, string> ClassSubjects)
        BuildBlocks(TimetableInputSources sources)
    {
        var sessionTypeLength = sources.SessionTypes.ToDictionary(st => st.Id, st => st.Length);
        // CurriculumGroupSession is keyed by (GroupId, SubjectId), so per (group, subject) we
        // know the session breakdown — directly attributable to the Class for that subject.
        var groupSessionsByGroupAndSubject = sources.GroupSessions
            .GroupBy(gs => (gs.CurriculumGroupId, gs.SubjectId))
            .ToDictionary(g => g.Key, g => g.ToArray());
        var groupsByBlock = sources.Groups
            .GroupBy(cg => cg.BlockId)
            .ToDictionary(g => g.Key, g => g.ToArray());
        var classesByGroup = sources.Classes
            .GroupBy(c => c.CurriculumGroupId)
            .ToDictionary(g => g.Key, g => g.ToArray());
        var subjectByCourse = sources.Courses.ToDictionary(c => c.Id, c => c.SubjectId);

        var blocks = new List<Block>(sources.Blocks.Count);
        var classSubjects = new Dictionary<string, string>();

        foreach (var blockEntity in sources.Blocks)
        {
            if (!groupsByBlock.TryGetValue(blockEntity.Id, out var blockGroups) || blockGroups.Length == 0)
                throw new InvalidOperationException(
                    $"CurriculumBlock '{blockEntity.Code}' has no curriculum groups.");

            // Build solver groups. Each Class derives its own SessionSizes from CurriculumGroupSession
            // rows for (group, class.subject). The block's slot multiset is the union of one group's
            // class session sizes; we validate that every group in the block produces the same
            // multiset so slots can be lockstep across groups.
            int[]? canonicalSlotSizes = null;
            string? canonicalGroupId = null;

            var solverGroups = blockGroups.Select(g =>
            {
                if (!classesByGroup.TryGetValue(g.Id, out var groupClasses) || groupClasses.Length == 0)
                    throw new InvalidOperationException(
                        $"CurriculumGroup '{g.Id}' has no Classes.");

                var classDefs = groupClasses.Select(c =>
                {
                    if (!subjectByCourse.TryGetValue(c.CourseId, out var subjectId))
                        throw new InvalidOperationException(
                            $"Class '{c.Code}' references missing Course '{c.CourseId}'.");

                    if (!groupSessionsByGroupAndSubject.TryGetValue((g.Id, subjectId), out var sessions)
                        || sessions.Length == 0)
                        throw new InvalidOperationException(
                            $"Class '{c.Code}' has no CurriculumGroupSessions configured " +
                            $"for group '{g.Id}', subject '{subjectId}'.");

                    var classSessionSizes = sessions
                        .SelectMany(gs => Enumerable.Repeat(sessionTypeLength[gs.SessionTypeId], gs.SessionAmount))
                        .ToArray();

                    var classKey = c.Id.ToString();
                    var subjectKey = subjectId.ToString();
                    classSubjects[classKey] = subjectKey;
                    return new ClassDefinition(classKey, subjectKey, classSessionSizes);
                }).ToArray();

                // Aggregate this group's class-level sessions into the group's slot multiset.
                // Sorted so multiset comparison across groups is order-independent.
                var groupSlotSizes = classDefs
                    .SelectMany(c => c.SessionSizes)
                    .OrderBy(s => s)
                    .ToArray();

                if (canonicalSlotSizes is null)
                {
                    canonicalSlotSizes = groupSlotSizes;
                    canonicalGroupId = g.Id.ToString();
                }
                else if (!canonicalSlotSizes.SequenceEqual(groupSlotSizes))
                {
                    throw new InvalidOperationException(
                        $"Block '{blockEntity.Code}' groups have mismatched session-size profiles. " +
                        $"Group '{canonicalGroupId}' has [{string.Join(",", canonicalSlotSizes)}], " +
                        $"group '{g.Id}' has [{string.Join(",", groupSlotSizes)}]. " +
                        $"All groups in a block must share the same total session-size multiset.");
                }

                return new Group(g.Id.ToString(), classDefs);
            }).ToArray();

            blocks.Add(new Block(blockEntity.Id.ToString(), canonicalSlotSizes!, solverGroups));
        }

        return (blocks, classSubjects);
    }

    private static List<Band> BuildBands(TimetableInputSources sources)
    {
        var blocksByBand = sources.BandBlocks
            .GroupBy(bba => bba.BandId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)
                g.Select(bba => bba.BlockId.ToString()).ToArray());

        return sources.Bands
            .Select(b => new Band(
                b.Id.ToString(),
                blocksByBand.TryGetValue(b.Id, out var ids) ? ids : Array.Empty<string>()))
            .ToList();
    }

    private static List<Pin> BuildPins(TimetableInputSources sources)
        => sources.Pins.Select(p => new Pin(
            p.CurriculumBlockId.ToString(),
            p.SlotIndex,
            ClassId:        p.ClassId?.ToString(),
            TeacherId:      p.TeacherId?.ToString(),
            RoomId:         p.RoomId?.ToString(),
            StartPeriodId:  p.StartAttendancePeriodId?.ToString())).ToList();
}
