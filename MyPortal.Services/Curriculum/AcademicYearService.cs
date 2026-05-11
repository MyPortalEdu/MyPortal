using System.Data;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Constants;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Curriculum;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Curriculum;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.Curriculum;

public class AcademicYearService : BaseService, IAcademicYearService
{
    private readonly IAcademicYearRepository _academicYearRepository;
    private readonly IAcademicTermRepository _academicTermRepository;
    private readonly IAttendancePeriodRepository _attendancePeriodRepository;
    private readonly IAttendanceWeekRepository _attendanceWeekRepository;
    private readonly ISchoolHolidayRepository _schoolHolidayRepository;
    private readonly IDiaryEventRepository _diaryEventRepository;
    private readonly IStudentGroupRepository _studentGroupRepository;
    private readonly IStudentGroupSupervisorRepository _studentGroupSupervisorRepository;
    private readonly IYearGroupRepository _yearGroupRepository;
    private readonly IRegGroupRepository _regGroupRepository;
    private readonly IHouseRepository _houseRepository;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IValidationService _validationService;

    private IDictionary<SchoolHolidayType, Guid> _schoolHolidayMap = new Dictionary<SchoolHolidayType, Guid>
    {
        { SchoolHolidayType.HalfTerm, DiaryEventTypes.SchoolHoliday },
        { SchoolHolidayType.PublicHoliday, DiaryEventTypes.PublicHoliday },
        { SchoolHolidayType.TeacherTraining, DiaryEventTypes.TeacherTraining }
    };

    // Reverse of _schoolHolidayMap for surfacing holidays on read. Built in the
    // field initialiser so it stays automatically consistent with the forward map.
    private static readonly IReadOnlyDictionary<Guid, SchoolHolidayType> EventTypeToHolidayType
        = new Dictionary<Guid, SchoolHolidayType>
        {
            { DiaryEventTypes.SchoolHoliday,   SchoolHolidayType.HalfTerm },
            { DiaryEventTypes.PublicHoliday,   SchoolHolidayType.PublicHoliday },
            { DiaryEventTypes.TeacherTraining, SchoolHolidayType.TeacherTraining }
        };


    public AcademicYearService(IAuthorizationService authorizationService, ILogger<AcademicYearService> logger,
        IAcademicYearRepository academicYearRepository, IAcademicTermRepository academicTermRepository,
        IAttendancePeriodRepository attendancePeriodRepository, IAttendanceWeekRepository attendanceWeekRepository,
        ISchoolHolidayRepository schoolHolidayRepository, IDiaryEventRepository diaryEventRepository,
        IStudentGroupRepository studentGroupRepository,
        IStudentGroupSupervisorRepository studentGroupSupervisorRepository,
        IYearGroupRepository yearGroupRepository, IRegGroupRepository regGroupRepository,
        IHouseRepository houseRepository, IUnitOfWorkFactory unitOfWorkFactory,
        IValidationService validationService) : base(authorizationService, logger)
    {
        _academicYearRepository = academicYearRepository;
        _academicTermRepository = academicTermRepository;
        _attendancePeriodRepository = attendancePeriodRepository;
        _attendanceWeekRepository = attendanceWeekRepository;
        _schoolHolidayRepository = schoolHolidayRepository;
        _diaryEventRepository = diaryEventRepository;
        _studentGroupRepository = studentGroupRepository;
        _studentGroupSupervisorRepository = studentGroupSupervisorRepository;
        _yearGroupRepository = yearGroupRepository;
        _regGroupRepository = regGroupRepository;
        _houseRepository = houseRepository;
        _unitOfWorkFactory = unitOfWorkFactory;
        _validationService = validationService;
    }

    public async Task<IList<AcademicYearSummaryResponse>> ListAsync(CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Curriculum.ViewAcademicYears,
            cancellationToken);

        return await _academicYearRepository.GetSummariesAsync(cancellationToken);
    }

    public async Task<AcademicYearDetailsResponse> GetByIdAsync(Guid academicYearId,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Curriculum.ViewAcademicYears,
            cancellationToken);

        var result = await _academicYearRepository.GetDetailsByIdAsync(academicYearId, cancellationToken)
                     ?? throw new NotFoundException("Academic year not found.");

        result.Header.Terms = result.Terms;
        result.Header.AttendancePeriods = result.AttendancePeriods;
        result.Header.SchoolHolidays = result.Holidays.Select(h => new SchoolHolidayResponse
        {
            Id = h.Id,
            Name = h.Name,
            // Holidays were always created via _schoolHolidayMap, so every EventTypeId
            // here should round-trip cleanly. If a row ever shows up with an unknown
            // type (e.g. someone hand-inserts a SchoolHoliday with an unrelated
            // DiaryEventType), defaulting to HalfTerm is the least-surprising fallback.
            Type = EventTypeToHolidayType.TryGetValue(h.EventTypeId, out var t) ? t : SchoolHolidayType.HalfTerm,
            StartDate = h.StartDate,
            EndDate = h.EndDate
        }).ToList();

        return result.Header;
    }

    public async Task<AcademicYearSummaryResponse?> GetCurrentAsync(CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Curriculum.ViewAcademicYears,
            cancellationToken);

        return await _academicYearRepository.GetCurrentAsync(cancellationToken);
    }

    public async Task<Guid> CreateAcademicYear(AcademicYearUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Curriculum.EditAcademicYears, cancellationToken);

        await _validationService.ValidateAsync(model);

        return await _unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await EnsureNoOverlapAsync(excludeAcademicYearId: null, model, uow.Transaction, cancellationToken);

            var academicYearId = SqlConvention.SequentialGuid();

            var academicYear = new AcademicYear
            {
                Id = academicYearId,
                Name = BuildAcademicYearName(model),
                TimetableCycleLength = model.TimetableCycleLength,
                SchoolWeekLength = model.SchoolWeekLength
            };

            await _academicYearRepository.InsertAsync(academicYear, cancellationToken, uow.Transaction);

            await InsertCalendarArtefactsAsync(academicYearId, model, uow.Transaction, cancellationToken);

            if (model.CopyPastoralStructureFromAcademicYearId.HasValue)
            {
                await CopyPastoralStructure(model.CopyPastoralStructureFromAcademicYearId.Value, academicYearId,
                    uow.Transaction, cancellationToken);
            }

            return academicYear.Id;
        }, cancellationToken);
    }

    private async Task EnsureNoOverlapAsync(Guid? excludeAcademicYearId, AcademicYearUpsertRequest model,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var rangeStart = model.AcademicTerms.Min(t => t.StartDate);
        var rangeEnd = model.AcademicTerms.Max(t => t.EndDate);

        if (await _academicYearRepository.HasOverlapAsync(excludeAcademicYearId, rangeStart, rangeEnd,
                cancellationToken, transaction))
        {
            throw new ArgumentException(
                "The academic year's term dates overlap with another existing academic year.");
        }
    }

    private async Task InsertCalendarArtefactsAsync(Guid academicYearId, AcademicYearUpsertRequest model,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        // Sort terms chronologically so weekIndex (calendar weeks since the year's first
        // Monday) is deterministic regardless of caller input order. The cycle counter ticks
        // continuously across terms — including holiday weeks that get skipped below — so
        // anchoring it on the year's first Monday is what gives us "continuous" semantics.
        var orderedTerms = model.AcademicTerms.OrderBy(t => t.StartDate).ToArray();
        var yearStartMonday = MondayOf(orderedTerms[0].StartDate);

        foreach (var termModel in orderedTerms)
        {
            var academicTerm = new AcademicTerm
            {
                Id = SqlConvention.SequentialGuid(),
                AcademicYearId = academicYearId,
                Name = termModel.Name,
                StartDate = termModel.StartDate,
                EndDate = termModel.EndDate
            };

            await _academicTermRepository.InsertAsync(academicTerm, cancellationToken, transaction);

            // Walk every calendar week that touches the term, anchored on Monday. Every week
            // gets a row — weeks where the entire Mon-Fri block is covered by SchoolHoliday
            // rows are flagged IsNonTimetable so the attendance flow knows to write '#' marks
            // instead of expecting real codes. Single mid-week holidays (May Day etc.) leave
            // IsNonTimetable false and are resolved per-day at the attendance level. The
            // cycleOffset is computed for every week regardless so downstream cycle-aware
            // code doesn't need a special case.
            for (var monday = MondayOf(termModel.StartDate);
                 monday <= termModel.EndDate;
                 monday = monday.AddDays(7))
            {
                var weekIndex = (monday - yearStartMonday).Days / 7;
                var cycleOffset =
                    (model.FirstWeekOffset + weekIndex * model.SchoolWeekLength) % model.TimetableCycleLength;

                var attendanceWeek = new AttendanceWeek
                {
                    Id = SqlConvention.SequentialGuid(),
                    AcademicTermId = academicTerm.Id,
                    Beginning = monday,
                    CycleOffset = cycleOffset,
                    IsNonTimetable = IsFullHolidayWeek(monday, model.SchoolWeekLength, model.SchoolHolidays)
                };

                await _attendanceWeekRepository.InsertAsync(attendanceWeek, cancellationToken, transaction);
            }
        }

        foreach (var holidayModel in model.SchoolHolidays)
        {
            var diaryEvent = new DiaryEvent
            {
                Id = SqlConvention.SequentialGuid(),
                Subject = holidayModel.Name,
                StartTime = holidayModel.StartDate,
                EndTime = holidayModel.EndDate,
                // IsAllDay=true tells fn_diary_event_get_overlapping to extend the end
                // by a day so the last calendar day of the holiday is covered. Without
                // this, EndTime defaults to midnight at the start of EndDate and lessons
                // on that final day would slip past the holiday filter.
                IsAllDay = true,
                IsPublic = true,
                IsSystem = true,
                EventTypeId = _schoolHolidayMap[holidayModel.Type]
            };

            var diaryEventEntity =
                await _diaryEventRepository.InsertAsync(diaryEvent, cancellationToken, transaction);

            var schoolHoliday = new SchoolHoliday
            {
                Id = SqlConvention.SequentialGuid(),
                AcademicYearId = academicYearId,
                EventId = diaryEventEntity.Id
            };

            await _schoolHolidayRepository.InsertAsync(schoolHoliday, cancellationToken, transaction);
        }

        IEnumerable<AttendancePeriodUpsertRequest> periodSource;
        if (model.CopyPeriodsFromAcademicYearId.HasValue)
        {
            var periodsToCopy = await _attendancePeriodRepository.GetAttendancePeriodsByAcademicYear(
                model.CopyPeriodsFromAcademicYearId.Value, cancellationToken);

            periodSource = periodsToCopy.Select(p => new AttendancePeriodUpsertRequest
            {
                Name = p.Name,
                CycleDayIndex = p.CycleDayIndex,
                StartTime = p.StartTime,
                EndTime = p.EndTime,
                IsAmReg = p.IsAmReg,
                IsPmReg = p.IsPmReg
            });
        }
        else
        {
            periodSource = model.AttendancePeriods;
        }

        foreach (var periodModel in periodSource)
        {
            var attendancePeriod = new AttendancePeriod
            {
                Id = SqlConvention.SequentialGuid(),
                AcademicYearId = academicYearId,
                Name = periodModel.Name,
                CycleDayIndex = periodModel.CycleDayIndex,
                StartTime = periodModel.StartTime,
                EndTime = periodModel.EndTime,
                IsAmReg = periodModel.IsAmReg,
                IsPmReg = periodModel.IsPmReg
            };

            await _attendancePeriodRepository.InsertAsync(attendancePeriod, cancellationToken, transaction);
        }
    }

    private static string BuildAcademicYearName(AcademicYearUpsertRequest model) =>
        $"{model.AcademicTerms.Min(a => a.StartDate.Year)}/{model.AcademicTerms.Max(a => a.StartDate.Year)}";

    private async Task CopyPastoralStructure(Guid sourceAcademicYearId, Guid targetAcademicYearId,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var sourceStudentGroups =
            await _studentGroupRepository.GetStudentGroupsByAcademicYear(sourceAcademicYearId, cancellationToken);
        var sourceSupervisors =
            await _studentGroupSupervisorRepository.GetStudentGroupSupervisorsByAcademicYear(sourceAcademicYearId,
                cancellationToken);
        var sourceYearGroups =
            await _yearGroupRepository.GetYearGroupsByAcademicYear(sourceAcademicYearId, cancellationToken);
        var sourceRegGroups =
            await _regGroupRepository.GetRegGroupsByAcademicYear(sourceAcademicYearId, cancellationToken);
        var sourceHouses =
            await _houseRepository.GetHousesByAcademicYear(sourceAcademicYearId, cancellationToken);

        // Pre-allocate IDs so cross-row references can be remapped without ordering hacks.
        var studentGroupIdMap = sourceStudentGroups.ToDictionary(sg => sg.Id, _ => SqlConvention.SequentialGuid());
        var supervisorIdMap = sourceSupervisors.ToDictionary(s => s.Id, _ => SqlConvention.SequentialGuid());
        var yearGroupIdMap = sourceYearGroups.ToDictionary(yg => yg.Id, _ => SqlConvention.SequentialGuid());

        // StudentGroup.MainSupervisorId points at StudentGroupSupervisor.Id, but
        // StudentGroupSupervisor.StudentGroupId points back at StudentGroup.Id — a circular
        // FK that we break by inserting groups with MainSupervisorId=null first, then
        // backfilling once supervisors exist. PromoteToGroupId is also left null here:
        // it gets set by the end-of-year promotion routine, not by structure copy.
        var insertedStudentGroups = new Dictionary<Guid, StudentGroup>();
        foreach (var src in sourceStudentGroups)
        {
            var studentGroup = new StudentGroup
            {
                Id = studentGroupIdMap[src.Id],
                Description = src.Description,
                Active = src.Active,
                Code = src.Code,
                AcademicYearId = targetAcademicYearId,
                PromoteToGroupId = null,
                MainSupervisorId = null,
                MaxMembers = src.MaxMembers,
                Notes = src.Notes,
                IsSystem = src.IsSystem
            };

            var inserted = await _studentGroupRepository.InsertAsync(studentGroup, cancellationToken, transaction);
            insertedStudentGroups[src.Id] = inserted;
        }

        foreach (var src in sourceSupervisors)
        {
            var supervisor = new StudentGroupSupervisor
            {
                Id = supervisorIdMap[src.Id],
                StudentGroupId = studentGroupIdMap[src.StudentGroupId],
                SupervisorId = src.SupervisorId,
                Title = src.Title
            };

            await _studentGroupSupervisorRepository.InsertAsync(supervisor, cancellationToken, transaction);
        }

        foreach (var src in sourceStudentGroups.Where(sg => sg.MainSupervisorId.HasValue))
        {
            var inserted = insertedStudentGroups[src.Id];
            inserted.MainSupervisorId = supervisorIdMap[src.MainSupervisorId!.Value];

            await _studentGroupRepository.UpdateAsync(inserted, cancellationToken, transaction);
        }

        foreach (var src in sourceYearGroups)
        {
            var yearGroup = new YearGroup
            {
                Id = yearGroupIdMap[src.Id],
                StudentGroupId = studentGroupIdMap[src.StudentGroupId],
                CurriculumYearGroupId = src.CurriculumYearGroupId
            };

            await _yearGroupRepository.InsertAsync(yearGroup, cancellationToken, transaction);
        }

        foreach (var src in sourceRegGroups)
        {
            var regGroup = new RegGroup
            {
                Id = SqlConvention.SequentialGuid(),
                StudentGroupId = studentGroupIdMap[src.StudentGroupId],
                YearGroupId = yearGroupIdMap[src.YearGroupId],
                RoomId = src.RoomId
            };

            await _regGroupRepository.InsertAsync(regGroup, cancellationToken, transaction);
        }

        foreach (var src in sourceHouses)
        {
            var house = new House
            {
                Id = SqlConvention.SequentialGuid(),
                StudentGroupId = studentGroupIdMap[src.StudentGroupId],
                ColourCode = src.ColourCode
            };

            await _houseRepository.InsertAsync(house, cancellationToken, transaction);
        }
    }

    public async Task<Guid> UpdateAcademicYear(Guid academicYearId, AcademicYearUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Curriculum.EditAcademicYears, cancellationToken);

        // The Copy* fields seed structure at create time only — re-applying them on update is
        // ambiguous (would we wipe the existing pastoral hierarchy? merge?). Reject up-front so
        // the caller gets a precise error instead of the validator's "either Copy or inline"
        // message, which would be misleading here.
        if (model.CopyPeriodsFromAcademicYearId.HasValue ||
            model.CopyPastoralStructureFromAcademicYearId.HasValue)
        {
            throw new ArgumentException(
                "Copy fields are only valid when creating a new academic year. " +
                "Delete and recreate the year if you want to re-copy structure.");
        }

        await _validationService.ValidateAsync(model);

        return await _unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var academicYear = await _academicYearRepository.GetByIdAsync(academicYearId, cancellationToken,
                                   uow.Transaction)
                               ?? throw new NotFoundException("Academic year not found.");

            if (academicYear.IsLocked)
            {
                throw new AcademicYearLockedException(
                    "This academic year is locked and cannot be edited.");
            }

            // Once the earliest term has started (or starts today) the calendar is live —
            // attendance weeks are being read by the register flow, sessions reference
            // periods, etc. Blocking updates from this point keeps regen safe even if no
            // marks have been written yet.
            var earliestStart = await _academicYearRepository.GetEarliestTermStartDateAsync(
                academicYearId, cancellationToken, uow.Transaction);

            if (earliestStart.HasValue && earliestStart.Value.Date <= DateTime.UtcNow.Date)
            {
                throw new AcademicYearLockedException(
                    "This academic year has already started and cannot be edited.");
            }

            if (await _academicYearRepository.HasDownstreamDataAsync(academicYearId, cancellationToken,
                    uow.Transaction))
            {
                throw new AcademicYearLockedException(
                    "This academic year has data attached to it and cannot be edited. " +
                    "Delete the dependent data first, or create a new year.");
            }

            await EnsureNoOverlapAsync(excludeAcademicYearId: academicYearId, model, uow.Transaction,
                cancellationToken);

            // FK-safe wipe order: periods (no children — marks already proven absent),
            // weeks (parent of marks), terms (parent of weeks), holidays + their backing
            // diary events. Pastoral hierarchy is intentionally left in place.
            await _attendancePeriodRepository.DeleteByAcademicYearAsync(academicYearId, cancellationToken,
                uow.Transaction);
            await _attendanceWeekRepository.DeleteByAcademicYearAsync(academicYearId, cancellationToken,
                uow.Transaction);
            await _academicTermRepository.DeleteByAcademicYearAsync(academicYearId, cancellationToken,
                uow.Transaction);
            await _schoolHolidayRepository.DeleteByAcademicYearAsync(academicYearId, cancellationToken,
                uow.Transaction);

            academicYear.Name = BuildAcademicYearName(model);
            academicYear.TimetableCycleLength = model.TimetableCycleLength;
            academicYear.SchoolWeekLength = model.SchoolWeekLength;

            await _academicYearRepository.UpdateAsync(academicYear, cancellationToken, uow.Transaction);

            await InsertCalendarArtefactsAsync(academicYearId, model, uow.Transaction, cancellationToken);

            return academicYear.Id;
        }, cancellationToken);
    }

    public async Task DeleteAcademicYear(Guid academicYearId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Curriculum.EditAcademicYears, cancellationToken);

        await _unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var academicYear = await _academicYearRepository.GetByIdAsync(academicYearId, cancellationToken,
                                   uow.Transaction)
                               ?? throw new NotFoundException("Academic year not found.");

            if (academicYear.IsLocked)
            {
                throw new AcademicYearLockedException(
                    "This academic year is locked and cannot be deleted.");
            }

            // Same gate as update: once the calendar is live other systems read from it,
            // and once any user data is attached we can't tear the AY down without
            // orphaning that data.
            var earliestStart = await _academicYearRepository.GetEarliestTermStartDateAsync(
                academicYearId, cancellationToken, uow.Transaction);

            if (earliestStart.HasValue && earliestStart.Value.Date <= DateTime.UtcNow.Date)
            {
                throw new AcademicYearLockedException(
                    "This academic year has already started and cannot be deleted.");
            }

            if (await _academicYearRepository.HasDownstreamDataAsync(academicYearId, cancellationToken,
                    uow.Transaction))
            {
                throw new AcademicYearLockedException(
                    "This academic year has data attached to it and cannot be deleted. " +
                    "Delete the dependent data first.");
            }

            // Same FK-safe wipe as update, then add the pastoral hierarchy on top (which
            // update leaves alone) and finally the AY row itself.
            await _attendancePeriodRepository.DeleteByAcademicYearAsync(academicYearId, cancellationToken,
                uow.Transaction);
            await _attendanceWeekRepository.DeleteByAcademicYearAsync(academicYearId, cancellationToken,
                uow.Transaction);
            await _academicTermRepository.DeleteByAcademicYearAsync(academicYearId, cancellationToken,
                uow.Transaction);
            await _schoolHolidayRepository.DeleteByAcademicYearAsync(academicYearId, cancellationToken,
                uow.Transaction);
            await _studentGroupRepository.DeleteByAcademicYearAsync(academicYearId, cancellationToken,
                uow.Transaction);

            await _academicYearRepository.DeleteAsync(academicYearId, cancellationToken,
                transaction: uow.Transaction);
        }, cancellationToken);
    }

    private static DateTime MondayOf(DateTime date)
    {
        // DayOfWeek.Sunday is 0, Monday is 1, …; the (+ 7) % 7 wraps Sunday back to a 6-day
        // step instead of -1 so we land on the most recent Monday at or before the date.
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.Date.AddDays(-diff);
    }

    private static bool IsFullHolidayWeek(DateTime monday, int schoolWeekLength,
        IEnumerable<SchoolHolidayUpsertRequest> holidays)
    {
        var holidayList = holidays as IList<SchoolHolidayUpsertRequest> ?? holidays.ToList();
        for (var i = 0; i < schoolWeekLength; i++)
        {
            var day = monday.AddDays(i).Date;
            if (!holidayList.Any(h => day >= h.StartDate.Date && day <= h.EndDate.Date))
            {
                return false;
            }
        }
        return true;
    }
}