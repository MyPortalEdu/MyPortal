using System.Data;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Constants;
using MyPortal.Common.Enums;
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


    public AcademicYearService(IAuthorizationService authorizationService, ILogger<BaseService> logger,
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

    public async Task<Guid> CreateAcademicYear(AcademicYearUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Curriculum.EditAcademicYears, cancellationToken);

        await _validationService.ValidateAsync(model);

        return await _unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var academicYearId = SqlConvention.SequentialGuid();
            var academicYearName =
                $"{model.AcademicTerms.Min(a => a.StartDate.Year)}/{model.AcademicTerms.Max(a => a.StartDate.Year)}";

            var academicYear = new AcademicYear
            {
                Id = academicYearId,
                Name = academicYearName,
                TimetableCycleLength = model.TimetableCycleLength,
                SchoolWeekLength = model.SchoolWeekLength
            };
            
            await _academicYearRepository.InsertAsync(academicYear, cancellationToken, uow.Transaction);

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

                await _academicTermRepository.InsertAsync(academicTerm, cancellationToken, uow.Transaction);

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

                    await _attendanceWeekRepository.InsertAsync(attendanceWeek, cancellationToken, uow.Transaction);
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
                    await _diaryEventRepository.InsertAsync(diaryEvent, cancellationToken, uow.Transaction);

                var schoolHoliday = new SchoolHoliday
                {
                    Id = SqlConvention.SequentialGuid(),
                    AcademicYearId = academicYearId,
                    EventId = diaryEventEntity.Id
                };
                
                await _schoolHolidayRepository.InsertAsync(schoolHoliday, cancellationToken, uow.Transaction);
            }

            if (model.CopyPeriodsFromAcademicYearId.HasValue)
            {
                var periodsToCopy =
                    await _attendancePeriodRepository.GetAttendancePeriodsByAcademicYear(
                        model.CopyPeriodsFromAcademicYearId.Value, cancellationToken);

                foreach (var periodModel in periodsToCopy)
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

                    await _attendancePeriodRepository.InsertAsync(attendancePeriod, cancellationToken, uow.Transaction);
                }
            }
            else
            {
                foreach (var periodModel in model.AttendancePeriods)
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

                    await _attendancePeriodRepository.InsertAsync(attendancePeriod, cancellationToken, uow.Transaction);
                }
            }

            if (model.CopyPastoralStructureFromAcademicYearId.HasValue)
            {
                await CopyPastoralStructure(model.CopyPastoralStructureFromAcademicYearId.Value, academicYearId,
                    uow.Transaction, cancellationToken);
            }

            return academicYear.Id;
        }, cancellationToken);
    }

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

    public Task<Guid> UpdateAcademicYear(Guid academicYearId, AcademicYearUpsertRequest model, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAcademicYear(Guid academicYearId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
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