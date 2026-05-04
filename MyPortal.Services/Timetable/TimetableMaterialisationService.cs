using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Constants;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.Services;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.Timetable;

public class TimetableMaterialisationService : BaseService, ITimetableMaterialisationService
{
    private readonly ITimetableRepository _timetableRepository;

    public TimetableMaterialisationService(IAuthorizationService authorizationService,
        ILogger<TimetableMaterialisationService> logger, ITimetableRepository timetableRepository)
        : base(authorizationService, logger)
    {
        _timetableRepository = timetableRepository;
    }

    public async Task MaterialiseAsync(Guid timetableId, DateTime startDate, DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var assignments = await _timetableRepository.ListAssignmentsAsync(timetableId, cancellationToken);
        if (assignments.Count == 0)
        {
            Logger.LogInformation("No assignments to materialise for timetable {timetableId}.", timetableId);
            return;
        }

        var periods = await _timetableRepository.GetAttendancePeriodsForAssignmentsAsync(
            timetableId, cancellationToken);

        // For each (AcademicYear, CycleDayIndex), build a chain so we can walk a slot of size N
        // from its start period to N consecutive same-day periods. Solver-side guarantees the
        // start period has enough successors, but the materialiser still throws defensively if
        // a size-N slot runs off the end of the day.
        var nextByPeriod = BuildNextPeriodMap(periods);

        var resolvedEndDate = endDate ?? startDate.AddYears(1);
        var sessions = new List<Session>(assignments.Count);
        var sessionPeriods = new List<SessionPeriod>();

        // Track which periods each teacher ends up teaching, so we can compute their non-
        // contact slots in one pass without a second join.
        var teachingPeriodsByTeacher = new Dictionary<Guid, HashSet<Guid>>();

        foreach (var a in assignments)
        {
            var sessionId = SqlConvention.SequentialGuid();
            sessions.Add(new Session
            {
                Id = sessionId,
                ClassId = a.ClassId,
                TeacherId = a.TeacherId,
                RoomId = a.RoomId,
                TimetableId = timetableId,
                StartDate = startDate,
                EndDate = resolvedEndDate,
            });

            if (!teachingPeriodsByTeacher.TryGetValue(a.TeacherId, out var teaching))
            {
                teaching = new HashSet<Guid>();
                teachingPeriodsByTeacher[a.TeacherId] = teaching;
            }

            var currentPeriodId = a.StartAttendancePeriodId;
            for (var i = 0; i < a.Size; i++)
            {
                sessionPeriods.Add(new SessionPeriod
                {
                    Id = SqlConvention.SequentialGuid(),
                    SessionId = sessionId,
                    PeriodId = currentPeriodId,
                });
                teaching.Add(currentPeriodId);

                if (i < a.Size - 1)
                {
                    if (!nextByPeriod.TryGetValue(currentPeriodId, out var next) || next is null)
                    {
                        throw new InvalidOperationException(
                            $"Assignment {a.Id} requested {a.Size} periods but ran off the end of " +
                            $"the day from period {currentPeriodId}.");
                    }
                    currentPeriodId = next.Value;
                }
            }
        }

        await _timetableRepository.BulkInsertSessionsAsync(sessions, cancellationToken);
        await _timetableRepository.BulkInsertSessionPeriodsAsync(sessionPeriods, cancellationToken);

        var ppaAllocations = await BuildPpaAllocationsAsync(timetableId, teachingPeriodsByTeacher,
            periods, startDate, resolvedEndDate, cancellationToken);
        if (ppaAllocations.Count > 0)
        {
            await _timetableRepository.BulkInsertNonContactAllocationsAsync(ppaAllocations,
                cancellationToken);
        }

        Logger.LogInformation(
            "Materialised timetable {timetableId}: {sessionCount} sessions, " +
            "{sessionPeriodCount} session periods, {ppaCount} PPA allocations.",
            timetableId, sessions.Count, sessionPeriods.Count, ppaAllocations.Count);
    }

    private async Task<List<StaffNonContactAllocation>> BuildPpaAllocationsAsync(Guid timetableId,
        Dictionary<Guid, HashSet<Guid>> teachingPeriodsByTeacher, IList<AttendancePeriod> periods,
        DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var teachers = await _timetableRepository.GetAssignedTeachersAsync(timetableId, cancellationToken);
        var teachersWithPpa = teachers.Where(t => t.PpaPeriodsPerWeek > 0).ToArray();
        if (teachersWithPpa.Length == 0) return new List<StaffNonContactAllocation>();

        // Take the first N free periods in chronological order. Deterministic; if a school
        // wants PPA on different days, an admin can reassign.
        var orderedPeriods = periods
            .OrderBy(p => p.CycleDayIndex)
            .ThenBy(p => p.StartTime)
            .ToArray();

        var allocations = new List<StaffNonContactAllocation>();
        foreach (var teacher in teachersWithPpa)
        {
            var teaching = teachingPeriodsByTeacher.TryGetValue(teacher.Id, out var t)
                ? t : new HashSet<Guid>();

            var emitted = 0;
            foreach (var p in orderedPeriods)
            {
                if (emitted >= teacher.PpaPeriodsPerWeek) break;
                if (teaching.Contains(p.Id)) continue;

                allocations.Add(new StaffNonContactAllocation
                {
                    Id = SqlConvention.SequentialGuid(),
                    StaffMemberId = teacher.Id,
                    TimetableId = timetableId,
                    AttendancePeriodId = p.Id,
                    Code = StaffNonContactCodes.Ppa,
                    StartDate = startDate,
                    EndDate = endDate,
                });
                emitted++;
            }
        }

        return allocations;
    }

    private static Dictionary<Guid, Guid?> BuildNextPeriodMap(IList<AttendancePeriod> periods)
    {
        var next = new Dictionary<Guid, Guid?>(periods.Count);
        foreach (var dayGroup in periods.GroupBy(p => (p.AcademicYearId, p.CycleDayIndex)))
        {
            var ordered = dayGroup.OrderBy(p => p.StartTime).ToArray();
            for (var i = 0; i < ordered.Length; i++)
            {
                next[ordered[i].Id] = i + 1 < ordered.Length ? ordered[i + 1].Id : null;
            }
        }
        return next;
    }
}
