using System.Transactions;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.Timetabler;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Services;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.Curriculum.Timetable;

public class TimetableService : BaseService, ITimetableService
{
    private readonly ITimetableRepository _repository;
    private readonly ITimetableRunRepository _runRepository;
    private readonly ITimetableMaterialisationService _materialisationService;
    private readonly IValidationService _validationService;

    public TimetableService(IAuthorizationService authorizationService,
        ILogger<TimetableService> logger, ITimetableRepository repository,
        ITimetableRunRepository runRepository,
        ITimetableMaterialisationService materialisationService,
        IValidationService validationService)
        : base(authorizationService, logger)
    {
        _repository = repository;
        _runRepository = runRepository;
        _materialisationService = materialisationService;
        _validationService = validationService;
    }

    public async Task<Guid> CreateDraftAsync(TimetableUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Timetable.EditTimetables,
            cancellationToken);

        var entity = new Core.Entities.Timetable
        {
            Id = SqlConvention.SequentialGuid(),
            AcademicYearId = model.AcademicYearId,
            Name = model.Name,
            Status = TimetableStatus.Draft,
            EffectiveFrom = null,
            EffectiveTo = null,
        };

        await _repository.InsertAsync(entity, cancellationToken);

        Logger.LogInformation("Timetable draft created: {timetableId}", entity.Id);
        return entity.Id;
    }

    public async Task<TimetableDetailsResponse> GetByIdAsync(Guid timetableId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Timetable.ViewTimetables,
            cancellationToken);

        var entity = await _repository.GetByIdAsync(timetableId, cancellationToken)
                     ?? throw new NotFoundException("Timetable not found.");

        return ToDetails(entity);
    }

    public async Task<IList<TimetableSummaryResponse>> ListByAcademicYearAsync(Guid academicYearId,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Timetable.ViewTimetables,
            cancellationToken);

        var entities = await _repository.ListByAcademicYearAsync(academicYearId, cancellationToken);
        return entities.Select(ToSummary).ToList();
    }

    public async Task<IList<TimetableRunResponse>> ListRunsAsync(Guid timetableId,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Timetable.ViewTimetables,
            cancellationToken);

        var runs = await _repository.ListRunsAsync(timetableId, cancellationToken);
        return runs.Select(ToRunResponse).ToList();
    }

    public async Task<TimetableRunResponse> GetRunAsync(Guid runId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Timetable.ViewTimetables,
            cancellationToken);

        var run = await _runRepository.GetRunAsync(runId, cancellationToken)
                  ?? throw new NotFoundException("Run not found.");

        return ToRunResponse(run);
    }

    public async Task<IList<TimetableAssignmentResponse>> ListAssignmentsAsync(Guid timetableId,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Timetable.ViewTimetables,
            cancellationToken);

        var assignments = await _repository.ListAssignmentsAsync(timetableId, cancellationToken);
        return assignments.Select(ToAssignmentResponse).ToList();
    }

    public async Task ApplyAsync(Guid timetableId, TimetableApplyRequest model,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Timetable.EditTimetables,
            cancellationToken);

        var timetable = await _repository.GetByIdAsync(timetableId, cancellationToken)
                        ?? throw new NotFoundException("Timetable not found.");

        if (timetable.Status != TimetableStatus.Draft)
            throw new InvalidOperationException(
                $"Only Draft timetables can be applied; this one is {timetable.Status}.");

        if (model.EffectiveTo.HasValue && model.EffectiveTo.Value < model.EffectiveFrom)
            throw new ArgumentException("EffectiveTo must be on or after EffectiveFrom.");

        // Wrap repository status flip + Session materialisation in one ambient transaction so
        // the timetable can never be marked Active without the corresponding Sessions, or vice
        // versa. The repo's ApplyAsync uses TransactionScope.Required and will enlist.
        using var tx = new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = global::System.Transactions.IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        await _repository.ApplyAsync(timetable.Id, timetable.AcademicYearId,
            model.EffectiveFrom, model.EffectiveTo, cancellationToken);

        await _materialisationService.MaterialiseAsync(timetable.Id,
            model.EffectiveFrom, model.EffectiveTo, cancellationToken);

        tx.Complete();

        Logger.LogInformation("Timetable applied: {timetableId} (effective {from}..{to}).",
            timetable.Id, model.EffectiveFrom, model.EffectiveTo);
    }

    public async Task DiscardAsync(Guid timetableId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Timetable.EditTimetables,
            cancellationToken);

        var timetable = await _repository.GetByIdAsync(timetableId, cancellationToken)
                        ?? throw new NotFoundException("Timetable not found.");

        if (timetable.Status == TimetableStatus.Active)
            throw new InvalidOperationException("Cannot discard an Active timetable — apply a replacement first.");

        await _repository.UpdateStatusAsync(timetableId, TimetableStatus.Discarded, cancellationToken);

        Logger.LogInformation("Timetable discarded: {timetableId}", timetableId);
    }

    public async Task<Guid> AddPinAsync(Guid timetableId, TimetablePinUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Timetable.EditTimetables,
            cancellationToken);

        await _validationService.ValidateAsync(model);

        var timetable = await _repository.GetByIdAsync(timetableId, cancellationToken)
                        ?? throw new NotFoundException("Timetable not found.");

        // Pinning a Superseded/Discarded timetable changes nothing meaningful (no future runs);
        // only Draft / Active accept new pins. Active pins protect a re-solve from breaking the
        // currently-applied schedule.
        if (timetable.Status is TimetableStatus.Superseded or TimetableStatus.Discarded)
            throw new InvalidOperationException(
                $"Cannot add pins to a {timetable.Status} timetable.");

        var userId = AuthorizationService.GetCurrentUserId()
                     ?? throw new ForbiddenException("Not authenticated.");

        var pin = new TimetablePin
        {
            Id = SqlConvention.SequentialGuid(),
            TimetableId = timetableId,
            CurriculumBlockId = model.CurriculumBlockId,
            SlotIndex = model.SlotIndex,
            ClassId = model.ClassId,
            TeacherId = model.TeacherId,
            RoomId = model.RoomId,
            StartAttendancePeriodId = model.StartAttendancePeriodId,
            CreatedById = userId,
            CreatedByIpAddress = AuthorizationService.GetCurrentUserIpAddress() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
        };

        await _repository.InsertPinAsync(pin, cancellationToken);

        Logger.LogInformation("Pin added: {pinId} to timetable {timetableId}", pin.Id, timetableId);
        return pin.Id;
    }

    public async Task<IList<TimetablePinResponse>> ListPinsAsync(Guid timetableId,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Timetable.ViewTimetables,
            cancellationToken);

        var pins = await _repository.ListPinsAsync(timetableId, cancellationToken);
        return pins.Select(ToPinResponse).ToList();
    }

    public async Task RemovePinAsync(Guid timetableId, Guid pinId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Timetable.EditTimetables,
            cancellationToken);

        var pin = await _repository.GetPinAsync(pinId, cancellationToken)
                  ?? throw new NotFoundException("Pin not found.");

        // Defence-in-depth: an attacker can't delete pins from a different timetable by guessing
        // pin ids — the URL's timetableId must match the pin's parent.
        if (pin.TimetableId != timetableId)
            throw new NotFoundException("Pin not found.");

        await _repository.DeletePinAsync(pinId, cancellationToken);

        Logger.LogInformation("Pin removed: {pinId} from timetable {timetableId}", pinId, timetableId);
    }

    // --- mapping ----------------------------------------------------------------------

    private static TimetableSummaryResponse ToSummary(Core.Entities.Timetable t) => new()
    {
        Id = t.Id, AcademicYearId = t.AcademicYearId, Name = t.Name,
        Status = (int)t.Status,
        EffectiveFrom = t.EffectiveFrom, EffectiveTo = t.EffectiveTo,
        CreatedAt = t.CreatedAt,
    };

    private static TimetableDetailsResponse ToDetails(Core.Entities.Timetable t) => new()
    {
        Id = t.Id, AcademicYearId = t.AcademicYearId, Name = t.Name,
        Status = (int)t.Status,
        EffectiveFrom = t.EffectiveFrom, EffectiveTo = t.EffectiveTo,
        CreatedById = t.CreatedById, CreatedAt = t.CreatedAt,
        LastModifiedAt = t.LastModifiedAt, Version = t.Version,
    };

    private static TimetableRunResponse ToRunResponse(TimetableRun r) => new()
    {
        Id = r.Id, TimetableId = r.TimetableId,
        Status = (int)r.Status,
        StartedAt = r.StartedAt, CompletedAt = r.CompletedAt,
        SolverDiagnostic = r.SolverDiagnostic,
        TriggeredById = r.TriggeredById,
    };

    private static TimetableAssignmentResponse ToAssignmentResponse(TimetableAssignment a) => new()
    {
        Id = a.Id, TimetableId = a.TimetableId,
        CurriculumBlockId = a.CurriculumBlockId, SlotIndex = a.SlotIndex,
        ClassId = a.ClassId, TeacherId = a.TeacherId, RoomId = a.RoomId,
        StartAttendancePeriodId = a.StartAttendancePeriodId, Size = a.Size,
    };

    private static TimetablePinResponse ToPinResponse(TimetablePin p) => new()
    {
        Id = p.Id, TimetableId = p.TimetableId,
        CurriculumBlockId = p.CurriculumBlockId, SlotIndex = p.SlotIndex,
        ClassId = p.ClassId, TeacherId = p.TeacherId, RoomId = p.RoomId,
        StartAttendancePeriodId = p.StartAttendancePeriodId,
        CreatedById = p.CreatedById, CreatedAt = p.CreatedAt,
    };
}
