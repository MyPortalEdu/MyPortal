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

        if (!options.MaximiseSpread)
        {
            return BuildAndSolve(input, ctx, options, withObjective: false, hint: null);
        }

        // Two-pass: feasibility first (no objective), then optimise spread warm-started from
        // the feasible solution. With an objective from the start, CP-SAT spends its budget
        // exploring the optimisation landscape and at full-school scale never reaches a
        // feasible point — staging avoids that.
        var halfBudget = Math.Max(60.0, options.MaxSeconds / 2.0);

        var pass1 = BuildAndSolve(input, ctx,
            options with { MaxSeconds = halfBudget },
            withObjective: false, hint: null);

        if (pass1.Status is not (SolveStatus.Feasible or SolveStatus.Optimal))
        {
            return pass1;
        }

        var pass2 = BuildAndSolve(input, ctx,
            options with { MaxSeconds = halfBudget },
            withObjective: true, hint: pass1);

        // If the optimisation pass timed out or somehow regressed to non-feasible, hand back
        // the feasible solution from pass 1 — better than failing because the objective took
        // too long to make progress.
        return pass2.Status is SolveStatus.Feasible or SolveStatus.Optimal ? pass2 : pass1;
    }

    private TimetableOutput BuildAndSolve(TimetableInput input, SolverContext ctx, SolveOptions options,
        bool withObjective, TimetableOutput? hint)
    {
        var model = new CpModel();

        // 1) Per-slot start variables and their interval representation.
        var slotStart    = new Dictionary<(string Block, int Slot), IntVar>();
        var slotInterval = new Dictionary<(string Block, int Slot), IntervalVar>();

        foreach (var block in input.Blocks)
        {
            for (var i = 0; i < block.SlotSizes.Count; i++)
            {
                var size = block.SlotSizes[i];
                var validStarts = ctx.ValidStartsBySize[size];

                var s  = model.NewIntVarFromDomain(Domain.FromValues(validStarts.Select(x => (long)x).ToArray()),
                    $"start_{block.Id}_{i}");
                var iv = model.NewFixedSizeIntervalVar(s, size, $"int_{block.Id}_{i}");

                slotStart[(block.Id, i)]    = s;
                slotInterval[(block.Id, i)] = iv;
            }
        }

        // 2) Resource-assignment booleans + optional intervals for teachers and rooms.
        // For each (block, slot, class), exactly one candidate teacher (and room) is "present" —
        // we encode that as a fan of BoolVars summing to 1, paired with optional intervals so
        // CP-SAT's NoOverlap can prevent any teacher/room being in two places at once.
        var teacherAssign = new Dictionary<(string Block, int Slot, string Class, string Teacher), BoolVar>();
        var roomAssign    = new Dictionary<(string Block, int Slot, string Class, string Room),    BoolVar>();

        var teacherIntervals = input.Teachers.ToDictionary(t => t.Id, _ => new List<IntervalVar>());
        var roomIntervals    = input.Rooms.ToDictionary(r => r.Id,    _ => new List<IntervalVar>());

        foreach (var block in input.Blocks)
        {
            for (var i = 0; i < block.SlotSizes.Count; i++)
            {
                var size  = block.SlotSizes[i];
                var start = slotStart[(block.Id, i)];

                foreach (var group in block.Groups)
                {
                    foreach (var cls in group.Classes)
                    {
                        var eligibleTeachers = ctx.TeachersForSubject[cls.SubjectId];
                        var eligibleRooms    = ctx.RoomsForSubject[cls.SubjectId];

                        var teacherBools = new List<BoolVar>(eligibleTeachers.Count);
                        foreach (var t in eligibleTeachers)
                        {
                            var b   = model.NewBoolVar($"t_{block.Id}_{i}_{cls.Id}_{t}");
                            var oiv = model.NewOptionalFixedSizeIntervalVar(start, size, b,
                                $"toi_{block.Id}_{i}_{cls.Id}_{t}");
                            teacherAssign[(block.Id, i, cls.Id, t)] = b;
                            teacherIntervals[t].Add(oiv);
                            teacherBools.Add(b);
                        }
                        model.AddExactlyOne(teacherBools);

                        var roomBools = new List<BoolVar>(eligibleRooms.Count);
                        foreach (var r in eligibleRooms)
                        {
                            var b   = model.NewBoolVar($"r_{block.Id}_{i}_{cls.Id}_{r}");
                            var oiv = model.NewOptionalFixedSizeIntervalVar(start, size, b,
                                $"roi_{block.Id}_{i}_{cls.Id}_{r}");
                            roomAssign[(block.Id, i, cls.Id, r)] = b;
                            roomIntervals[r].Add(oiv);
                            roomBools.Add(b);
                        }
                        model.AddExactlyOne(roomBools);
                    }
                }
            }
        }

        foreach (var (_, ivs) in teacherIntervals) if (ivs.Count > 1) model.AddNoOverlap(ivs);
        foreach (var (_, ivs) in roomIntervals)    if (ivs.Count > 1) model.AddNoOverlap(ivs);

        // 2a) PPA cap. A teacher's total weekly teaching load cannot exceed
        //     (TotalPeriodsPerWeek - PpaPeriodsPerWeek). Encoded as a weighted sum over the
        //     teacher's assignment indicators where the weight is the slot's size — assigning
        //     a double consumes 2 of the cap, a triple 3. Skipped when PpaPeriodsPerWeek=0
        //     (the default), so existing inputs see no behavioural change.
        var totalPeriodsPerWeek = input.Periods.Count;
        var slotSizeByKey = new Dictionary<(string Block, int Slot), int>();
        foreach (var block in input.Blocks)
        {
            for (var i = 0; i < block.SlotSizes.Count; i++)
            {
                slotSizeByKey[(block.Id, i)] = block.SlotSizes[i];
            }
        }

        foreach (var teacher in input.Teachers)
        {
            if (teacher.PpaPeriodsPerWeek <= 0) continue;

            var teacherKeys = teacherAssign.Keys.Where(k => k.Teacher == teacher.Id).ToArray();
            if (teacherKeys.Length == 0) continue;

            var bools   = teacherKeys.Select(k => teacherAssign[k]).ToArray();
            var weights = teacherKeys.Select(k => (long)slotSizeByKey[(k.Block, k.Slot)]).ToArray();

            model.Add(LinearExpr.WeightedSum(bools, weights) <=
                      totalPeriodsPerWeek - teacher.PpaPeriodsPerWeek);
        }

        // 3) A block's own slots must not overlap with each other (a block can't run two slots
        //    simultaneously; valid-starts already prevent intra-day collisions but this is the
        //    canonical guard).
        foreach (var block in input.Blocks)
        {
            if (block.SlotSizes.Count > 1)
            {
                model.AddNoOverlap(block.SlotSizes
                    .Select((_, i) => slotInterval[(block.Id, i)])
                    .ToArray());
            }
        }

        // 4) Within a band, all blocks that participate in the standard rotation must not
        //    overlap (students take exactly one class per band-block, so two such blocks can't
        //    run concurrently). Blocks with AllowsConcurrentScheduling=true (alt-curriculum
        //    pull-outs like Friday-P4 maths tutoring) are exempt.
        foreach (var band in input.Bands)
        {
            var rotationIntervals = band.BlockIds
                .Select(id => ctx.BlockById[id])
                .Where(b => !b.AllowsConcurrentScheduling)
                .SelectMany(b => b.SlotSizes.Select((_, i) => slotInterval[(b.Id, i)]))
                .ToList();
            if (rotationIntervals.Count > 1) model.AddNoOverlap(rotationIntervals);
        }

        // 5) Pinning. Each pin can fix any subset of {start period, teacher, room} for a slot.
        foreach (var pin in input.Pins)
        {
            if (pin.StartPeriodId is not null)
            {
                var idx = ctx.PeriodIndex[pin.StartPeriodId];
                model.Add(slotStart[(pin.BlockId, pin.SlotIndex)] == idx);
            }
            if (pin.ClassId is not null)
            {
                if (pin.TeacherId is not null)
                {
                    model.Add(teacherAssign[(pin.BlockId, pin.SlotIndex, pin.ClassId, pin.TeacherId)] == 1);
                }
                if (pin.RoomId is not null)
                {
                    model.Add(roomAssign[(pin.BlockId, pin.SlotIndex, pin.ClassId, pin.RoomId)] == 1);
                }
            }
        }

        // 6) Spread objective: minimise the count of slot-pairs *within the same block* that
        //    land on the same day. Pair-penalty form rather than distinct-day-count because
        //    it prefers (2,2) over (3,1) for a 4-slot block — it punishes clumping
        //    quadratically. Cheap encoding: one IntVar per slot via AddElement + one BoolVar
        //    per intra-block pair (no per-day reified vars, no OR aggregations). Skips blocks
        //    of size 1 — nothing to spread.
        if (withObjective)
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
            if (penaltyTerms.Count > 0)
            {
                model.Minimize(LinearExpr.Sum(penaltyTerms.ToArray()));
            }
        }

        // 7) Hints from a previous feasible solution warm-start the optimisation pass —
        //    CP-SAT uses hints as a starting point for the search, so the optimiser can skip
        //    the slow phase of finding any feasible point and go straight to improving it.
        if (hint is not null)
        {
            ApplyHint(hint, slotStart, teacherAssign, roomAssign, ctx, model);
        }

        // 8) Solve.
        var solver = new CpSolver
        {
            StringParameters = $"max_time_in_seconds:{options.MaxSeconds:0.###}" +
                               (options.RandomSeed is { } seed ? $" random_seed:{seed}" : "") +
                               (options.LogSearch ? " log_search_progress:true" : "")
        };

        var status = solver.Solve(model);
        var solveStatus = status switch
        {
            CpSolverStatus.Optimal    => SolveStatus.Optimal,
            CpSolverStatus.Feasible   => SolveStatus.Feasible,
            CpSolverStatus.Infeasible => SolveStatus.Infeasible,
            _                         => SolveStatus.Unknown
        };

        if (solveStatus is not (SolveStatus.Optimal or SolveStatus.Feasible))
        {
            return new TimetableOutput(solveStatus, [], $"CpSolverStatus={status}");
        }

        // 9) Extract assignments.
        var assignments = new List<Assignment>();
        foreach (var block in input.Blocks)
        {
            for (var i = 0; i < block.SlotSizes.Count; i++)
            {
                var size  = block.SlotSizes[i];
                var start = (int)solver.Value(slotStart[(block.Id, i)]);
                var periodIds = Enumerable.Range(start, size).Select(p => ctx.Periods[p].Id).ToArray();

                foreach (var group in block.Groups)
                {
                    foreach (var cls in group.Classes)
                    {
                        var teacher = ctx.TeachersForSubject[cls.SubjectId]
                            .First(t => solver.BooleanValue(teacherAssign[(block.Id, i, cls.Id, t)]));
                        var room = ctx.RoomsForSubject[cls.SubjectId]
                            .First(r => solver.BooleanValue(roomAssign[(block.Id, i, cls.Id, r)]));

                        assignments.Add(new Assignment(block.Id, i, cls.Id, teacher, room, periodIds));
                    }
                }
            }
        }

        return new TimetableOutput(solveStatus, assignments, $"CpSolverStatus={status}");
    }

    // --- helpers -------------------------------------------------------------------------

    private static void ApplyHint(TimetableOutput hint,
        Dictionary<(string, int), IntVar> slotStart,
        Dictionary<(string, int, string, string), BoolVar> teacherAssign,
        Dictionary<(string, int, string, string), BoolVar> roomAssign,
        SolverContext ctx, CpModel model)
    {
        var slotsHinted = new HashSet<(string, int)>();
        foreach (var a in hint.Assignments)
        {
            if (slotsHinted.Add((a.BlockId, a.SlotIndex)))
            {
                model.AddHint(slotStart[(a.BlockId, a.SlotIndex)], ctx.PeriodIndex[a.PeriodIds[0]]);
            }

            // Hint only the chosen teacher/room — CP-SAT's AddExactlyOne plus this single hint
            // is enough to imply the zeros for siblings, and skipping them keeps the hint set
            // small and the warm-start cheap.
            if (teacherAssign.TryGetValue((a.BlockId, a.SlotIndex, a.ClassId, a.TeacherId), out var tb))
            {
                model.AddHint(tb, 1);
            }
            if (roomAssign.TryGetValue((a.BlockId, a.SlotIndex, a.ClassId, a.RoomId), out var rb))
            {
                model.AddHint(rb, 1);
            }
        }
    }

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
        // Periods are flattened to integer indices; the chronology comes from the order the
        // caller supplies, but valid-start computation goes by (Day, OrderInDay).
        var periods = input.Periods.ToArray();
        var periodIndex = periods.Select((p, i) => (p, i)).ToDictionary(x => x.p.Id, x => x.i);

        // For each distinct slot size used anywhere, precompute the period indices where a
        // size-K run fits without crossing a day or a no-bridge gap.
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
