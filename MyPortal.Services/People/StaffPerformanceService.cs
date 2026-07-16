using System.Data;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.People;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

/// <summary>
/// Owns the Performance (appraisal) area: the review-cycle history plus the objective, observation
/// and training-record lists. Gated under <see cref="StaffArea.PerformanceDetails"/> — line-manager
/// (Managed) and HR (All) view/edit, no self scope. The save is a whole-area replace, each list
/// reconciled by id. Reviews and objectives are soft-deleted (audited history); observations and
/// training records are lean tables and hard-deleted.
/// </summary>
public class StaffPerformanceService : BaseService, IStaffPerformanceService
{
    private readonly IStaffMemberAccessService _accessService;
    private readonly IStaffMemberRepository _staffMemberRepository;
    private readonly IPerformanceReviewRepository _reviewRepository;
    private readonly IReviewStatusRepository _reviewStatusRepository;
    private readonly IStaffObjectiveRepository _objectiveRepository;
    private readonly IObjectiveStatusRepository _objectiveStatusRepository;
    private readonly IObjectiveCategoryRepository _objectiveCategoryRepository;
    private readonly IObservationRepository _observationRepository;
    private readonly IObservationOutcomeRepository _observationOutcomeRepository;
    private readonly ITrainingCertificateRepository _trainingRepository;
    private readonly ITrainingCourseRepository _trainingCourseRepository;
    private readonly ITrainingCertificateStatusRepository _trainingStatusRepository;
    private readonly IValidationService _validationService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public StaffPerformanceService(IAuthorizationService authorizationService,
        ILogger<StaffPerformanceService> logger, IStaffMemberAccessService accessService,
        IStaffMemberRepository staffMemberRepository, IPerformanceReviewRepository reviewRepository,
        IReviewStatusRepository reviewStatusRepository, IStaffObjectiveRepository objectiveRepository,
        IObjectiveStatusRepository objectiveStatusRepository, IObjectiveCategoryRepository objectiveCategoryRepository,
        IObservationRepository observationRepository, IObservationOutcomeRepository observationOutcomeRepository,
        ITrainingCertificateRepository trainingRepository, ITrainingCourseRepository trainingCourseRepository,
        ITrainingCertificateStatusRepository trainingStatusRepository, IValidationService validationService,
        IUnitOfWorkFactory unitOfWorkFactory) : base(authorizationService, logger)
    {
        _accessService = accessService;
        _staffMemberRepository = staffMemberRepository;
        _reviewRepository = reviewRepository;
        _reviewStatusRepository = reviewStatusRepository;
        _objectiveRepository = objectiveRepository;
        _objectiveStatusRepository = objectiveStatusRepository;
        _objectiveCategoryRepository = objectiveCategoryRepository;
        _observationRepository = observationRepository;
        _observationOutcomeRepository = observationOutcomeRepository;
        _trainingRepository = trainingRepository;
        _trainingCourseRepository = trainingCourseRepository;
        _trainingStatusRepository = trainingStatusRepository;
        _validationService = validationService;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<StaffPerformanceResponse> GetPerformanceAsync(Guid staffMemberId,
        CancellationToken cancellationToken)
    {
        // Line-manager (Managed) or HR (All) — no self scope.
        await _accessService.RequireAsync(staffMemberId, StaffArea.PerformanceDetails,
            StaffAccess.ViewManaged | StaffAccess.ViewAll, cancellationToken);

        var staffMember = await _staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        var reviews = await _reviewRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken);
        var objectives = await _objectiveRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken);
        var observations = await _observationRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken);
        var trainingRecords = await _trainingRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken);

        var reviewStatuses = await _reviewStatusRepository.GetListAsync(cancellationToken: cancellationToken);
        var objectiveStatuses = await _objectiveStatusRepository.GetListAsync(cancellationToken: cancellationToken);
        var objectiveCategories = await _objectiveCategoryRepository.GetListAsync(cancellationToken: cancellationToken);
        var outcomes = await _observationOutcomeRepository.GetListAsync(cancellationToken: cancellationToken);
        var trainingCourses = await _trainingCourseRepository.GetListAsync(cancellationToken: cancellationToken);
        var trainingStatuses = await _trainingStatusRepository.GetListAsync(cancellationToken: cancellationToken);
        var staff = await _staffMemberRepository.GetStaffLookupAsync(cancellationToken);

        return new StaffPerformanceResponse
        {
            Reviews = reviews.OrderByDescending(r => r.ReviewDate ?? DateTime.MinValue).Select(MapReview).ToList(),
            Objectives = objectives.OrderBy(o => o.DueDate ?? DateTime.MaxValue).Select(MapObjective).ToList(),
            Observations = observations.OrderByDescending(o => o.Date).Select(MapObservation).ToList(),
            TrainingRecords = trainingRecords.OrderByDescending(t => t.CompletedDate).Select(MapTraining).ToList(),
            ReviewStatuses = reviewStatuses.ToOrderedLookup(),
            ObjectiveStatuses = objectiveStatuses.ToOrderedLookup(),
            ObjectiveCategories = objectiveCategories.ToOrderedLookup(),
            Outcomes = outcomes.ToAlphabeticalLookup(),
            Staff = staff.ToList(),
            TrainingCourses = trainingCourses.ToAlphabeticalLookup(),
            TrainingStatuses = trainingStatuses.ToAlphabeticalLookup()
        };
    }

    public async Task UpdatePerformanceAsync(Guid staffMemberId, StaffPerformanceUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await _accessService.RequireAsync(staffMemberId, StaffArea.PerformanceDetails,
            StaffAccess.EditManaged | StaffAccess.EditAll, cancellationToken);

        await _validationService.ValidateAsync(model);

        var staffMember = await _staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        await _unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            // Reviews first: a new objective may reference an existing review by id.
            await ReconcileReviewsAsync(staffMemberId, model.Reviews, uow.Transaction, cancellationToken);
            await ReconcileObjectivesAsync(staffMemberId, model.Objectives, uow.Transaction, cancellationToken);
            await ReconcileObservationsAsync(staffMemberId, model.Observations, uow.Transaction, cancellationToken);
            await ReconcileTrainingAsync(staffMemberId, model.TrainingRecords, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    private async Task ReconcileReviewsAsync(Guid staffMemberId, List<PerformanceReviewUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing = (await _reviewRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken, transaction))
            .ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            // Audited — soft-delete to preserve appraisal history.
            await _reviewRepository.DeleteAsync(row.Id, cancellationToken, true, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplyReview(entity, item);
                await _reviewRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var created = new PerformanceReview
                {
                    Id = SqlConvention.SequentialGuid(),
                    StaffMemberId = staffMemberId
                };
                ApplyReview(created, item);
                await _reviewRepository.InsertAsync(created, cancellationToken, transaction);
            }
        }
    }

    private static void ApplyReview(PerformanceReview entity, PerformanceReviewUpsertItem item)
    {
        entity.CycleName = item.CycleName;
        entity.ReviewerId = item.ReviewerId;
        entity.StatusId = item.StatusId;
        entity.ReviewDate = item.ReviewDate;
        entity.NextReviewDate = item.NextReviewDate;
        entity.OverallOutcomeId = item.OverallRatingId;
        entity.Summary = item.Summary;
    }

    private async Task ReconcileObjectivesAsync(Guid staffMemberId, List<StaffObjectiveUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing = (await _objectiveRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken, transaction))
            .ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await _objectiveRepository.DeleteAsync(row.Id, cancellationToken, true, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplyObjective(entity, item);
                await _objectiveRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var created = new StaffObjective { Id = SqlConvention.SequentialGuid(), StaffMemberId = staffMemberId };
                ApplyObjective(created, item);
                await _objectiveRepository.InsertAsync(created, cancellationToken, transaction);
            }
        }
    }

    private static void ApplyObjective(StaffObjective entity, StaffObjectiveUpsertItem item)
    {
        entity.ReviewId = item.ReviewId;
        entity.CategoryId = item.CategoryId;
        entity.Title = item.Title;
        entity.Description = item.Description;
        entity.SuccessCriteria = item.SuccessCriteria;
        entity.DueDate = item.DueDate;
        entity.StatusId = item.StatusId;
        entity.ProgressNotes = item.ProgressNotes;
    }

    private async Task ReconcileObservationsAsync(Guid staffMemberId, List<StaffObservationUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing =
            (await _observationRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken, transaction))
            .ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            // Lean table — hard delete.
            await _observationRepository.DeleteAsync(row.Id, cancellationToken, false, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplyObservation(entity, item);
                await _observationRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var created = new Observation { Id = SqlConvention.SequentialGuid(), ObserveeId = staffMemberId };
                ApplyObservation(created, item);
                await _observationRepository.InsertAsync(created, cancellationToken, transaction);
            }
        }
    }

    private static void ApplyObservation(Observation entity, StaffObservationUpsertItem item)
    {
        entity.Date = item.Date;
        entity.ObserverId = item.ObserverId;
        entity.OutcomeId = item.OutcomeId;
        entity.Focus = item.Focus;
        entity.SubjectObserved = item.SubjectObserved;
        entity.Strengths = item.Strengths;
        entity.AreasForDevelopment = item.AreasForDevelopment;
        entity.Notes = item.Notes;
    }

    private async Task ReconcileTrainingAsync(Guid staffMemberId, List<StaffTrainingRecordUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing = (await _trainingRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken, transaction))
            .ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            // Lean table — hard delete.
            await _trainingRepository.DeleteAsync(row.Id, cancellationToken, false, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplyTraining(entity, item);
                await _trainingRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var created = new TrainingCertificate
                {
                    Id = SqlConvention.SequentialGuid(),
                    StaffMemberId = staffMemberId
                };
                ApplyTraining(created, item);
                await _trainingRepository.InsertAsync(created, cancellationToken, transaction);
            }
        }
    }

    private static void ApplyTraining(TrainingCertificate entity, StaffTrainingRecordUpsertItem item)
    {
        entity.TrainingCourseId = item.TrainingCourseId;
        entity.TrainingCertificateStatusId = item.StatusId;
        entity.CompletedDate = item.CompletedDate;
        entity.ExpiryDate = item.ExpiryDate;
        entity.Provider = item.Provider;
        entity.Hours = item.Hours;
        entity.CertificateReference = item.CertificateReference;
    }

    private static PerformanceReviewResponse MapReview(PerformanceReview r) => new()
    {
        Id = r.Id,
        CycleName = r.CycleName,
        ReviewerId = r.ReviewerId,
        StatusId = r.StatusId,
        ReviewDate = r.ReviewDate,
        NextReviewDate = r.NextReviewDate,
        OverallRatingId = r.OverallOutcomeId,
        Summary = r.Summary
    };

    private static StaffObjectiveResponse MapObjective(StaffObjective o) => new()
    {
        Id = o.Id,
        ReviewId = o.ReviewId,
        CategoryId = o.CategoryId,
        Title = o.Title,
        Description = o.Description,
        SuccessCriteria = o.SuccessCriteria,
        DueDate = o.DueDate,
        StatusId = o.StatusId,
        ProgressNotes = o.ProgressNotes
    };

    private static StaffObservationResponse MapObservation(Observation o) => new()
    {
        Id = o.Id,
        Date = o.Date,
        ObserverId = o.ObserverId,
        OutcomeId = o.OutcomeId,
        Focus = o.Focus,
        SubjectObserved = o.SubjectObserved,
        Strengths = o.Strengths,
        AreasForDevelopment = o.AreasForDevelopment,
        Notes = o.Notes
    };

    private static StaffTrainingRecordResponse MapTraining(TrainingCertificate t) => new()
    {
        Id = t.Id,
        TrainingCourseId = t.TrainingCourseId,
        StatusId = t.TrainingCertificateStatusId,
        CompletedDate = t.CompletedDate,
        ExpiryDate = t.ExpiryDate,
        Provider = t.Provider,
        Hours = t.Hours,
        CertificateReference = t.CertificateReference
    };
}
