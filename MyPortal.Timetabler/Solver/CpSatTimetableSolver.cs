using Google.OrTools.Sat;
using Google.OrTools.Util;
using MyPortal.Timetabler.Models;

namespace MyPortal.Timetabler.Solver;

public class CpSatTimetableSolver : ITimetableSolver
{
    public TimetableOutput Solve(TimetableInput input, SolveOptions? options = null)
    {
        options ??= new SolveOptions();

        var validation = Validate(input);
        if (validation is not null)
        {
            return new TimetableOutput(SolveStatus.ModelInvalid, [], validation);
        }

        var ctx = BuildIndexes(input);

        // Solve in two phases — single-pass at full-school scale was intractable for CP-SAT
        // because the per-slot resource layer multiplies the search space prohibitively.
        // Splitting cleanly into "where do classes happen" and "who teaches them, and where"
        // gives each phase a much smaller problem.
        var phase1Budget = Math.Max(60.0, options.MaxSeconds * 0.6);
        var phase2Budget = Math.Max(60.0, options.MaxSeconds * 0.4);

        var schedule = SolveSchedulePhase(input, ctx, options with { MaxSeconds = phase1Budget });
        if (schedule.Status is not (SolveStatus.Feasible or SolveStatus.Optimal))
        {
            return new TimetableOutput(schedule.Status, [],
                $"Phase 1 (schedule): {schedule.Diagnostic}");
        }

        // Diagnostic: print Phase 1's peak concurrent classes per subject. If this exceeds
        // teacher counts, the cumulative isn't doing its job; if it doesn't, Phase 2 infeasibility
        // is a per-teacher matching issue not a peak-count issue.
        if (options.LogSearch)
        {
            var classSubjectByKey = new Dictionary<(string, string, string), string>();
            foreach (var block in input.Blocks)
            foreach (var group in block.Groups)
            foreach (var cls in group.Classes)
                classSubjectByKey[(block.Id, group.Id, cls.Id)] = cls.SubjectId;

            var concurrentBySubjectPeriod = new Dictionary<(string, int), int>();
            foreach (var ((blockId, slotIdx, groupId), classId) in schedule.SlotClassPick)
            {
                var subjectId = classSubjectByKey[(blockId, groupId, classId)];
                var size = ctx.BlockById[blockId].SlotSizes[slotIdx];
                var startPeriod = schedule.SlotStarts[(blockId, slotIdx)];
                for (var p = startPeriod; p < startPeriod + size; p++)
                {
                    var key = (subjectId, p);
                    concurrentBySubjectPeriod[key] = concurrentBySubjectPeriod.GetValueOrDefault(key) + 1;
                }
            }

            var perSubjectMax = concurrentBySubjectPeriod
                .GroupBy(kv => kv.Key.Item1)
                .Select(g => (Subject: g.Key, Max: g.Max(kv => kv.Value),
                              TeacherCount: ctx.TeachersForSubject[g.Key].Count))
                .OrderByDescending(x => x.Max);
            Console.WriteLine("Phase 1 peak concurrency per subject:");
            foreach (var (s, max, tc) in perSubjectMax)
            {
                Console.WriteLine($"  {s,-15} max-concurrent={max,3} teachers={tc,3} {(max > tc ? "[OVER]" : "")}");
            }
        }

        var assignment = SolveAssignmentPhase(input, ctx,
            options with { MaxSeconds = phase2Budget }, schedule);
        if (assignment.Status is not (SolveStatus.Feasible or SolveStatus.Optimal))
        {
            return new TimetableOutput(assignment.Status, [],
                $"Phase 2 (assignment): {assignment.Diagnostic}");
        }

        return assignment;
    }

    // ─── Phase 1: schedule (slotStart + classPicked) ──────────────────────────────────────

    private sealed record SchedulePhaseResult(
        SolveStatus Status,
        string? Diagnostic,
        // (block, slot) → period index in week
        Dictionary<(string, int), int> SlotStarts,
        // (block, slot, group) → picked class id
        Dictionary<(string, int, string), string> SlotClassPick);

    private SchedulePhaseResult SolveSchedulePhase(TimetableInput input, SolverContext ctx,
        SolveOptions options)
    {
        var model = new CpModel();

        // Slot starts and intervals — same as today.
        var slotStart = new Dictionary<(string Block, int Slot), IntVar>();
        var slotInterval = new Dictionary<(string Block, int Slot), IntervalVar>();

        foreach (var block in input.Blocks)
        {
            for (var i = 0; i < block.SlotSizes.Count; i++)
            {
                var size = block.SlotSizes[i];
                var validStarts = ctx.ValidStartsBySize[size];
                var s = model.NewIntVarFromDomain(Domain.FromValues(validStarts.Select(x => (long)x).ToArray()),
                    $"start_{block.Id}_{i}");
                var iv = model.NewFixedSizeIntervalVar(s, size, $"int_{block.Id}_{i}");
                slotStart[(block.Id, i)] = s;
                slotInterval[(block.Id, i)] = iv;
            }
        }

        // Class-pick per (block, slot, group).
        var classPicked = new Dictionary<(string Block, int Slot, string Group, string Class), BoolVar>();

        foreach (var block in input.Blocks)
        {
            for (var i = 0; i < block.SlotSizes.Count; i++)
            {
                var size = block.SlotSizes[i];
                foreach (var group in block.Groups)
                {
                    var candidates = group.Classes.Where(c => c.SessionSizes.Contains(size)).ToArray();
                    if (candidates.Length == 0)
                        return new SchedulePhaseResult(SolveStatus.ModelInvalid,
                            $"Block '{block.Id}' has a slot of size {size} but group '{group.Id}' " +
                            $"has no class with a matching session size.",
                            new(), new());

                    var picks = new List<BoolVar>(candidates.Length);
                    foreach (var cls in candidates)
                    {
                        var pickBool = model.NewBoolVar($"pick_{block.Id}_{i}_{group.Id}_{cls.Id}");
                        classPicked[(block.Id, i, group.Id, cls.Id)] = pickBool;
                        picks.Add(pickBool);
                    }
                    model.AddExactlyOne(picks);
                }
            }
        }

        // Per-(group, class) cardinality.
        foreach (var block in input.Blocks)
        {
            var slotsBySize = new Dictionary<int, List<int>>();
            for (var i = 0; i < block.SlotSizes.Count; i++)
            {
                if (!slotsBySize.TryGetValue(block.SlotSizes[i], out var list))
                {
                    list = new List<int>();
                    slotsBySize[block.SlotSizes[i]] = list;
                }
                list.Add(i);
            }

            foreach (var group in block.Groups)
            {
                foreach (var cls in group.Classes)
                {
                    var sessionsBySize = cls.SessionSizes
                        .GroupBy(s => s)
                        .ToDictionary(g => g.Key, g => g.Count());

                    foreach (var (size, requiredCount) in sessionsBySize)
                    {
                        var matchingSlotIds = slotsBySize.TryGetValue(size, out var s) ? s : new List<int>();
                        var picks = matchingSlotIds
                            .Select(i => (LinearExpr)classPicked[(block.Id, i, group.Id, cls.Id)])
                            .ToArray();
                        if (picks.Length == 0)
                            return new SchedulePhaseResult(SolveStatus.ModelInvalid,
                                $"Class '{cls.Id}' needs {requiredCount} session(s) of size {size} " +
                                $"but block '{block.Id}' has no slots of that size.",
                                new(), new());
                        model.Add(LinearExpr.Sum(picks) == requiredCount);
                    }
                }
            }
        }

        // Block-internal slot NoOverlap.
        foreach (var block in input.Blocks)
        {
            if (block.SlotSizes.Count > 1)
            {
                model.AddNoOverlap(block.SlotSizes.Select((_, i) => slotInterval[(block.Id, i)]).ToArray());
            }
        }

        // Load balancing. Without this, Phase 1 can pile too many concurrent classes onto a
        // single period and Phase 2 has no way to find enough teachers/rooms. We need TWO
        // layers of cumulative:
        //   • per subject:  capacity = teacherCount for that subject (per-subject staff limit)
        //   • per room-pool: capacity = pool size (rooms shared across multiple subjects)
        // We build one optional interval per (block, slot, group, class) and reuse it in both
        // cumulatives so the model stays compact.
        var demandIntervalByClass = new Dictionary<(string Block, int Slot, string Group, string Class), IntervalVar>();
        foreach (var block in input.Blocks)
        {
            for (var i = 0; i < block.SlotSizes.Count; i++)
            {
                var size = block.SlotSizes[i];
                foreach (var group in block.Groups)
                {
                    foreach (var cls in group.Classes)
                    {
                        if (!cls.SessionSizes.Contains(size)) continue;

                        var pickIndicator = classPicked[(block.Id, i, group.Id, cls.Id)];
                        var oiv = model.NewOptionalFixedSizeIntervalVar(slotStart[(block.Id, i)],
                            size, pickIndicator, $"sd_{block.Id}_{i}_{group.Id}_{cls.Id}");
                        demandIntervalByClass[(block.Id, i, group.Id, cls.Id)] = oiv;
                    }
                }
            }
        }

        // Per-subject teacher cumulative.
        var classesBySubject = input.Blocks
            .SelectMany(b => b.Groups.SelectMany(g => g.Classes
                .Select(c => (b, g, c))))
            .GroupBy(x => x.c.SubjectId);

        foreach (var subjectGroup in classesBySubject)
        {
            var subjectId = subjectGroup.Key;
            var teacherCount = ctx.TeachersForSubject[subjectId].Count;
            if (teacherCount < 1) continue;

            var ivs = new List<IntervalVar>();
            foreach (var (block, group, cls) in subjectGroup)
            {
                for (var i = 0; i < block.SlotSizes.Count; i++)
                {
                    if (demandIntervalByClass.TryGetValue((block.Id, i, group.Id, cls.Id), out var iv))
                        ivs.Add(iv);
                }
            }
            if (ivs.Count == 0) continue;

            var cum = model.AddCumulative(teacherCount);
            foreach (var iv in ivs) cum.AddDemand(iv, 1);
        }

        // Per-room-pool cumulative. Group rooms by their (sorted) SubjectIds set — each
        // distinct set is a "pool". General classrooms with subjects {Eng, Maths, Fr, Hist,
        // Geo, RE, Cit} form one pool; specialist rooms (e.g., DT-only, PE-only) form their
        // own. A class uses any room in any pool whose subject set contains the class's
        // subject; we conservatively associate each class with the LARGEST pool that fits
        // (largest gives the loosest constraint while still preventing absolute overruns).
        // Capacity = number of rooms in the pool.
        var poolKey = (Func<IReadOnlyCollection<string>, string>)(
            subjects => string.Join("|", subjects.OrderBy(x => x)));
        var roomsByPool = input.Rooms
            .GroupBy(r => poolKey(r.SubjectIds))
            .ToDictionary(g => g.Key, g => g.ToArray());

        foreach (var (key, rooms) in roomsByPool)
        {
            var poolSubjects = rooms[0].SubjectIds.ToHashSet();
            var poolSize = rooms.Length;

            var ivs = new List<IntervalVar>();
            foreach (var block in input.Blocks)
            {
                for (var i = 0; i < block.SlotSizes.Count; i++)
                {
                    foreach (var group in block.Groups)
                    {
                        foreach (var cls in group.Classes)
                        {
                            if (!poolSubjects.Contains(cls.SubjectId)) continue;
                            if (!demandIntervalByClass.TryGetValue((block.Id, i, group.Id, cls.Id), out var iv))
                                continue;
                            ivs.Add(iv);
                        }
                    }
                }
            }
            if (ivs.Count == 0) continue;

            var cum = model.AddCumulative(poolSize);
            foreach (var iv in ivs) cum.AddDemand(iv, 1);
        }

        // Band-level NoOverlap.
        foreach (var band in input.Bands)
        {
            var rotationIntervals = band.BlockIds
                .Select(id => ctx.BlockById[id])
                .Where(b => !b.AllowsConcurrentScheduling)
                .SelectMany(b => b.SlotSizes.Select((_, i) => slotInterval[(b.Id, i)]))
                .ToList();
            if (rotationIntervals.Count > 1) model.AddNoOverlap(rotationIntervals);
        }

        // Period pins (StartPeriodId, no class).
        foreach (var pin in input.Pins)
        {
            if (pin.StartPeriodId is not null)
            {
                var idx = ctx.PeriodIndex[pin.StartPeriodId];
                model.Add(slotStart[(pin.BlockId, pin.SlotIndex)] == idx);
            }
        }

        // Spread objective: minimise same-day pairs of slot intervals within a block.
        if (options.MaximiseSpread)
        {
            var dayByPeriodIdx = ctx.Periods.Select(p => (long)p.Day).ToArray();
            var maxDay = (int)dayByPeriodIdx.Max();
            var penaltyTerms = new List<BoolVar>();

            foreach (var block in input.Blocks)
            {
                if (block.SlotSizes.Count < 2) continue;
                var slotDay = new IntVar[block.SlotSizes.Count];
                for (var i = 0; i < block.SlotSizes.Count; i++)
                {
                    var sd = model.NewIntVar(0, maxDay, $"sd_{block.Id}_{i}");
                    model.AddElement(slotStart[(block.Id, i)], dayByPeriodIdx, sd);
                    slotDay[i] = sd;
                }
                for (var i = 0; i < slotDay.Length; i++)
                for (var j = i + 1; j < slotDay.Length; j++)
                {
                    var same = model.NewBoolVar($"same_{block.Id}_{i}_{j}");
                    model.Add(slotDay[i] == slotDay[j]).OnlyEnforceIf(same);
                    model.Add(slotDay[i] != slotDay[j]).OnlyEnforceIf(same.Not());
                    penaltyTerms.Add(same);
                }
            }
            if (penaltyTerms.Count > 0) model.Minimize(LinearExpr.Sum(penaltyTerms.ToArray()));
        }

        var solver = new CpSolver
        {
            StringParameters = $"max_time_in_seconds:{options.MaxSeconds:0.###}" +
                               (options.RandomSeed is { } seed ? $" random_seed:{seed}" : "") +
                               (options.LogSearch ? " log_search_progress:true" : "")
        };
        var status = solver.Solve(model);
        var solveStatus = status switch
        {
            CpSolverStatus.Optimal => SolveStatus.Optimal,
            CpSolverStatus.Feasible => SolveStatus.Feasible,
            CpSolverStatus.Infeasible => SolveStatus.Infeasible,
            _ => SolveStatus.Unknown,
        };

        if (solveStatus is not (SolveStatus.Feasible or SolveStatus.Optimal))
        {
            return new SchedulePhaseResult(solveStatus, $"CpSolverStatus={status}", new(), new());
        }

        var slotStarts = new Dictionary<(string, int), int>();
        foreach (var (key, var) in slotStart)
        {
            slotStarts[key] = (int)solver.Value(var);
        }

        var picked = new Dictionary<(string, int, string), string>();
        foreach (var block in input.Blocks)
        {
            for (var i = 0; i < block.SlotSizes.Count; i++)
            {
                foreach (var group in block.Groups)
                {
                    var pickedCls = group.Classes.First(c =>
                        classPicked.TryGetValue((block.Id, i, group.Id, c.Id), out var b)
                        && solver.BooleanValue(b));
                    picked[(block.Id, i, group.Id)] = pickedCls.Id;
                }
            }
        }

        return new SchedulePhaseResult(solveStatus, $"CpSolverStatus={status}", slotStarts, picked);
    }

    // ─── Phase 2: resource assignment (classTeacher + classRoom) ──────────────────────────

    private TimetableOutput SolveAssignmentPhase(TimetableInput input, SolverContext ctx,
        SolveOptions options, SchedulePhaseResult phase1)
    {
        var model = new CpModel();

        // Index Phase 1's results for quick lookup. classByGroupSlot tells us which class is
        // picked at (block, slot, group) — fixed input to Phase 2.
        var classByGroupSlot = phase1.SlotClassPick;

        // For each (block, group, class) we need the list of slots where that class is picked.
        // Multi-class groups have classes appearing at *some* of the block's slots; single-class
        // groups have their one class at every slot.
        var classOccurrences = new Dictionary<(string Block, string Group, string Class), List<int>>();
        foreach (var ((blockId, slotIdx, groupId), classId) in classByGroupSlot)
        {
            var key = (blockId, groupId, classId);
            if (!classOccurrences.TryGetValue(key, out var list))
            {
                list = new List<int>();
                classOccurrences[key] = list;
            }
            list.Add(slotIdx);
        }

        // Per-slot (per-session) teacher and room decisions. Per-class continuity is too tight
        // for solver-time feasibility — schools that want it can pin a teacher per class. The
        // looser per-session model is just a per-period bipartite matching: at any period,
        // assign one teacher per concurrent class, drawn from the candidate pool.
        var teacherAssign = new Dictionary<(string Block, int Slot, string Group, string Class, string Teacher), BoolVar>();
        var roomAssign    = new Dictionary<(string Block, int Slot, string Group, string Class, string Room),    BoolVar>();

        var teacherIntervals = input.Teachers.ToDictionary(t => t.Id, _ => new List<IntervalVar>());
        var roomIntervals    = input.Rooms.ToDictionary(r => r.Id,    _ => new List<IntervalVar>());

        var classBySubjectLookup = new Dictionary<(string Block, string Group, string Class), string>();
        foreach (var block in input.Blocks)
        foreach (var group in block.Groups)
        foreach (var cls in group.Classes)
        {
            classBySubjectLookup[(block.Id, group.Id, cls.Id)] = cls.SubjectId;
        }

        var groupByBlockClass = new Dictionary<(string Block, string Class), string>();
        foreach (var block in input.Blocks)
        foreach (var group in block.Groups)
        foreach (var cls in group.Classes)
        {
            groupByBlockClass[(block.Id, cls.Id)] = group.Id;
        }

        foreach (var ((blockId, groupId, classId), occurrences) in classOccurrences)
        {
            var subjectId = classBySubjectLookup[(blockId, groupId, classId)];
            var eligibleTeachers = ctx.TeachersForSubject[subjectId];
            var eligibleRooms    = ctx.RoomsForSubject[subjectId];
            var size = ctx.BlockById[blockId].SlotSizes;

            foreach (var slotIdx in occurrences)
            {
                var slotSize = size[slotIdx];
                var startPeriod = phase1.SlotStarts[(blockId, slotIdx)];

                var tBools = new List<BoolVar>(eligibleTeachers.Count);
                foreach (var t in eligibleTeachers)
                {
                    var b = model.NewBoolVar($"t_{blockId}_{slotIdx}_{groupId}_{classId}_{t}");
                    teacherAssign[(blockId, slotIdx, groupId, classId, t)] = b;
                    tBools.Add(b);
                    var oiv = model.NewOptionalFixedSizeIntervalVar(model.NewConstant(startPeriod),
                        slotSize, b, $"toi_{blockId}_{slotIdx}_{groupId}_{classId}_{t}");
                    teacherIntervals[t].Add(oiv);
                }
                model.AddExactlyOne(tBools);

                var rBools = new List<BoolVar>(eligibleRooms.Count);
                foreach (var r in eligibleRooms)
                {
                    var b = model.NewBoolVar($"r_{blockId}_{slotIdx}_{groupId}_{classId}_{r}");
                    roomAssign[(blockId, slotIdx, groupId, classId, r)] = b;
                    rBools.Add(b);
                    var oiv = model.NewOptionalFixedSizeIntervalVar(model.NewConstant(startPeriod),
                        slotSize, b, $"roi_{blockId}_{slotIdx}_{groupId}_{classId}_{r}");
                    roomIntervals[r].Add(oiv);
                }
                model.AddExactlyOne(rBools);
            }
        }

        foreach (var (_, ivs) in teacherIntervals) if (ivs.Count > 1) model.AddNoOverlap(ivs);
        foreach (var (_, ivs) in roomIntervals)    if (ivs.Count > 1) model.AddNoOverlap(ivs);

        // PPA cap: per teacher, total assigned class-periods ≤ TotalPeriodsPerWeek - PpaPeriodsPerWeek.
        // Each teacherAssign[..., t] = 1 contributes (slot's size) periods.
        var totalPeriodsPerWeek = input.Periods.Count;
        var slotSizeByKey = new Dictionary<(string Block, int Slot), int>();
        foreach (var block in input.Blocks)
        for (var i = 0; i < block.SlotSizes.Count; i++)
            slotSizeByKey[(block.Id, i)] = block.SlotSizes[i];

        foreach (var teacher in input.Teachers)
        {
            if (teacher.PpaPeriodsPerWeek <= 0) continue;
            var teacherKeys = teacherAssign.Keys.Where(k => k.Teacher == teacher.Id).ToArray();
            if (teacherKeys.Length == 0) continue;
            var bools = teacherKeys.Select(k => teacherAssign[k]).ToArray();
            var weights = teacherKeys.Select(k => (long)slotSizeByKey[(k.Block, k.Slot)]).ToArray();
            model.Add(LinearExpr.WeightedSum(bools, weights) <=
                      totalPeriodsPerWeek - teacher.PpaPeriodsPerWeek);
        }

        // Teacher / room pins. With per-slot assignments, a pin without a SlotIndex applies
        // to every slot occurrence of the pinned class — modelled by forcing the per-slot
        // assignment indicator at each occurrence.
        foreach (var pin in input.Pins)
        {
            if (pin.ClassId is null) continue;
            var groupId = groupByBlockClass[(pin.BlockId, pin.ClassId)];
            if (!classOccurrences.TryGetValue((pin.BlockId, groupId, pin.ClassId), out var occurrences))
                continue;
            foreach (var slotIdx in occurrences)
            {
                if (pin.TeacherId is not null)
                    model.Add(teacherAssign[(pin.BlockId, slotIdx, groupId, pin.ClassId, pin.TeacherId)] == 1);
                if (pin.RoomId is not null)
                    model.Add(roomAssign[(pin.BlockId, slotIdx, groupId, pin.ClassId, pin.RoomId)] == 1);
            }
        }

        var solver = new CpSolver
        {
            StringParameters = $"max_time_in_seconds:{options.MaxSeconds:0.###}" +
                               (options.RandomSeed is { } seed ? $" random_seed:{seed}" : "") +
                               (options.LogSearch ? " log_search_progress:true" : "")
        };
        var status = solver.Solve(model);
        var solveStatus = status switch
        {
            CpSolverStatus.Optimal => SolveStatus.Optimal,
            CpSolverStatus.Feasible => SolveStatus.Feasible,
            CpSolverStatus.Infeasible => SolveStatus.Infeasible,
            _ => SolveStatus.Unknown,
        };

        if (solveStatus is not (SolveStatus.Feasible or SolveStatus.Optimal))
        {
            return new TimetableOutput(solveStatus, [], $"CpSolverStatus={status}");
        }

        // Compose final assignments by joining Phase 1's slot/class picks with Phase 2's
        // teacher/room picks.
        var assignments = new List<Assignment>();
        foreach (var block in input.Blocks)
        {
            for (var i = 0; i < block.SlotSizes.Count; i++)
            {
                var size = block.SlotSizes[i];
                var start = phase1.SlotStarts[(block.Id, i)];
                var periodIds = Enumerable.Range(start, size).Select(p => ctx.Periods[p].Id).ToArray();

                foreach (var group in block.Groups)
                {
                    var classId = phase1.SlotClassPick[(block.Id, i, group.Id)];
                    var subjectId = classBySubjectLookup[(block.Id, group.Id, classId)];
                    var teacher = ctx.TeachersForSubject[subjectId]
                        .First(t => solver.BooleanValue(teacherAssign[(block.Id, i, group.Id, classId, t)]));
                    var room = ctx.RoomsForSubject[subjectId]
                        .First(r => solver.BooleanValue(roomAssign[(block.Id, i, group.Id, classId, r)]));
                    assignments.Add(new Assignment(block.Id, i, classId, teacher, room, periodIds));
                }
            }
        }

        return new TimetableOutput(solveStatus, assignments, $"CpSolverStatus={status}");
    }

    // ─── shared helpers ───────────────────────────────────────────────────────────────────

    private static string? Validate(TimetableInput input)
    {
        if (input.Periods.Count == 0)   return "No periods supplied.";
        if (input.Blocks.Count == 0)    return "No blocks supplied.";
        if (input.Teachers.Count == 0)  return "No teachers supplied.";
        if (input.Rooms.Count == 0)     return "No rooms supplied.";

        var periodIds = input.Periods.Select(p => p.Id).ToHashSet();
        if (periodIds.Count != input.Periods.Count) return "Duplicate period ids.";

        var blockIds = input.Blocks.Select(b => b.Id).ToHashSet();
        if (blockIds.Count != input.Blocks.Count) return "Duplicate block ids.";

        foreach (var band in input.Bands)
        {
            foreach (var bid in band.BlockIds)
                if (!blockIds.Contains(bid))
                    return $"Band '{band.Id}' references missing block '{bid}'.";
        }

        foreach (var block in input.Blocks)
        {
            if (block.SlotSizes.Count == 0)             return $"Block '{block.Id}' has no slots.";
            if (block.SlotSizes.Any(s => s < 1))        return $"Block '{block.Id}' has a slot with size < 1.";
            if (block.Groups.Count == 0)                return $"Block '{block.Id}' has no groups.";
            foreach (var g in block.Groups)
                if (g.Classes.Count == 0)               return $"Group '{g.Id}' has no classes.";
        }

        var subjectIds = input.Blocks.SelectMany(b => b.Groups).SelectMany(g => g.Classes)
            .Select(c => c.SubjectId).ToHashSet();
        foreach (var subj in subjectIds)
        {
            if (!input.Teachers.Any(t => t.SubjectIds.Contains(subj)))
                return $"No teacher qualified for subject '{subj}'.";
            if (!input.Rooms.Any(r => r.SubjectIds.Contains(subj)))
                return $"No room suitable for subject '{subj}'.";
        }

        return null;
    }

    private sealed record SolverContext(
        IReadOnlyList<PeriodSlot> Periods,
        Dictionary<string, int> PeriodIndex,
        Dictionary<int, int[]> ValidStartsBySize,
        Dictionary<string, Block> BlockById,
        Dictionary<string, IReadOnlyList<string>> TeachersForSubject,
        Dictionary<string, IReadOnlyList<string>> RoomsForSubject);

    private static SolverContext BuildIndexes(TimetableInput input)
    {
        var periods = input.Periods.ToArray();
        var periodIndex = periods.Select((p, i) => (p, i)).ToDictionary(x => x.p.Id, x => x.i);

        var sizes = input.Blocks.SelectMany(b => b.SlotSizes).Distinct().ToArray();
        var validStartsBySize = sizes.ToDictionary(k => k, k => ComputeValidStarts(periods, k));

        var blockById = input.Blocks.ToDictionary(b => b.Id);

        var teachersForSubject = new Dictionary<string, IReadOnlyList<string>>();
        var roomsForSubject    = new Dictionary<string, IReadOnlyList<string>>();
        foreach (var subj in input.Blocks.SelectMany(b => b.Groups).SelectMany(g => g.Classes)
                     .Select(c => c.SubjectId).Distinct())
        {
            teachersForSubject[subj] = input.Teachers.Where(t => t.SubjectIds.Contains(subj))
                .Select(t => t.Id).ToArray();
            roomsForSubject[subj] = input.Rooms.Where(r => r.SubjectIds.Contains(subj))
                .Select(r => r.Id).ToArray();
        }

        return new SolverContext(periods, periodIndex, validStartsBySize, blockById,
            teachersForSubject, roomsForSubject);
    }

    private static int[] ComputeValidStarts(PeriodSlot[] periods, int size)
    {
        var valid = new List<int>();
        for (var p = 0; p + size <= periods.Length; p++)
        {
            var ok = true;
            for (var k = 0; k < size - 1; k++)
            {
                var a = periods[p + k];
                var b = periods[p + k + 1];
                if (a.Day != b.Day || a.OrderInDay + 1 != b.OrderInDay || a.NoDoubleAfter)
                {
                    ok = false;
                    break;
                }
            }
            if (ok) valid.Add(p);
        }
        return valid.ToArray();
    }
}
