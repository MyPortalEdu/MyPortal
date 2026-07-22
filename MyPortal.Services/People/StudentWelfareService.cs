using System.Data;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models;
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
/// The welfare / safeguarding area of the student profile. Maintains the looked-after care episodes,
/// Personal Education Plans (each with its own contributor set) and child protection plans — each
/// reconciled in one transaction — plus the flat welfare indicators held on the Student. Authorization
/// is enforced at the controller (Student.{View|Edit}StudentWelfare).
/// </summary>
public class StudentWelfareService(
    IAuthorizationService authorizationService,
    ILogger<StudentWelfareService> logger,
    IStudentRepository studentRepository,
    IStudentCareEpisodeRepository careEpisodeRepository,
    IStudentPepRepository pepRepository,
    IStudentPepContributorRepository pepContributorRepository,
    IStudentChildProtectionPlanRepository childProtectionPlanRepository,
    ILivingArrangementRepository livingArrangementRepository,
    ILocalAuthorityRepository localAuthorityRepository,
    IPostLookedAfterArrangementRepository postLookedAfterArrangementRepository,
    IServiceChildIndicatorRepository serviceChildIndicatorRepository,
    IYoungCarerIndicatorRepository youngCarerIndicatorRepository,
    IKinshipCareIndicatorRepository kinshipCareIndicatorRepository,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : BaseService(authorizationService, logger), IStudentWelfareService
{
    public async Task<StudentWelfareDetailsResponse> GetWelfareDetailsAsync(Guid studentId,
        CancellationToken cancellationToken)
    {
        var student = await LoadAsync(studentId, cancellationToken);

        var careEpisodes = await careEpisodeRepository.GetByStudentIdAsync(studentId, cancellationToken);
        var peps = (await pepRepository.GetByStudentIdAsync(studentId, cancellationToken)).ToList();
        var childProtectionPlans =
            await childProtectionPlanRepository.GetByStudentIdAsync(studentId, cancellationToken);

        var pepResponses = new List<PepResponse>();
        foreach (var pep in peps.OrderByDescending(p => p.StartDate))
        {
            var contributors = await pepContributorRepository.GetByPepIdAsync(pep.Id, cancellationToken);
            pepResponses.Add(MapPep(pep, contributors));
        }

        var livingArrangements = await livingArrangementRepository.GetListAsync(cancellationToken: cancellationToken);
        var localAuthorities = await localAuthorityRepository.GetListAsync(cancellationToken: cancellationToken);
        var postLookedAfterArrangements =
            await postLookedAfterArrangementRepository.GetListAsync(cancellationToken: cancellationToken);
        var serviceChildIndicators =
            await serviceChildIndicatorRepository.GetListAsync(cancellationToken: cancellationToken);
        var youngCarerIndicators =
            await youngCarerIndicatorRepository.GetListAsync(cancellationToken: cancellationToken);
        var kinshipCareIndicators =
            await kinshipCareIndicatorRepository.GetListAsync(cancellationToken: cancellationToken);

        return new StudentWelfareDetailsResponse
        {
            StudentId = studentId,
            PostLookedAfterArrangementId = student.PostLookedAfterArrangementId,
            ServiceChildIndicatorId = student.ServiceChildIndicatorId,
            YoungCarerIndicatorId = student.YoungCarerIndicatorId,
            KinshipCareIndicatorId = student.KinshipCareIndicatorId,
            CareEpisodes = careEpisodes
                .OrderByDescending(e => e.EndDate == null)
                .ThenByDescending(e => e.StartDate)
                .Select(MapCareEpisode)
                .ToList(),
            Peps = pepResponses,
            ChildProtectionPlans = childProtectionPlans
                .OrderByDescending(p => p.EndDate == null)
                .ThenByDescending(p => p.StartDate)
                .Select(MapChildProtectionPlan)
                .ToList(),
            LivingArrangements = livingArrangements.ToAlphabeticalLookup(),
            CaringAuthorities = MapLocalAuthorities(localAuthorities),
            PostLookedAfterArrangements = postLookedAfterArrangements.ToOrderedLookup(),
            ServiceChildIndicators = serviceChildIndicators.ToOrderedLookup(),
            YoungCarerIndicators = youngCarerIndicators.ToOrderedLookup(),
            KinshipCareIndicators = kinshipCareIndicators.ToOrderedLookup()
        };
    }

    public async Task UpdateWelfareIndicatorsAsync(Guid studentId, WelfareIndicatorsUpsertRequest model,
        CancellationToken cancellationToken)
    {
        var student = await LoadAsync(studentId, cancellationToken);

        // Only the welfare indicator FKs are touched; the rest of the Student row is preserved by
        // load-modify-save.
        student.PostLookedAfterArrangementId = model.PostLookedAfterArrangementId;
        student.ServiceChildIndicatorId = model.ServiceChildIndicatorId;
        student.YoungCarerIndicatorId = model.YoungCarerIndicatorId;
        student.KinshipCareIndicatorId = model.KinshipCareIndicatorId;

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await studentRepository.UpdateAsync(student, cancellationToken, uow.Transaction);
        }, cancellationToken);
    }

    public async Task UpdateCareEpisodesAsync(Guid studentId, IEnumerable<CareEpisodeUpsertRequest> model,
        CancellationToken cancellationToken)
    {
        var incoming = model.ToList();

        foreach (var episode in incoming)
        {
            await validationService.ValidateAsync(episode);
        }

        await LoadAsync(studentId, cancellationToken);

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await ReconcileCareEpisodesAsync(studentId, incoming, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    public async Task UpdatePepsAsync(Guid studentId, IEnumerable<PepUpsertRequest> model,
        CancellationToken cancellationToken)
    {
        var incoming = model.ToList();

        foreach (var pep in incoming)
        {
            await validationService.ValidateAsync(pep);
        }

        await LoadAsync(studentId, cancellationToken);

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await ReconcilePepsAsync(studentId, incoming, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    public async Task UpdateChildProtectionPlansAsync(Guid studentId,
        IEnumerable<ChildProtectionPlanUpsertRequest> model, CancellationToken cancellationToken)
    {
        var incoming = model.ToList();

        foreach (var plan in incoming)
        {
            await validationService.ValidateAsync(plan);
        }

        await LoadAsync(studentId, cancellationToken);

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await ReconcileChildProtectionPlansAsync(studentId, incoming, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    private async Task ReconcileCareEpisodesAsync(Guid studentId, List<CareEpisodeUpsertRequest> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing =
            (await careEpisodeRepository.GetByStudentIdAsync(studentId, cancellationToken, transaction)).ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await careEpisodeRepository.DeleteAsync(row.Id, cancellationToken, false, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplyCareEpisode(entity, item);
                await careEpisodeRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var created = new StudentCareEpisode { Id = SqlConvention.SequentialGuid(), StudentId = studentId };
                ApplyCareEpisode(created, item);
                await careEpisodeRepository.InsertAsync(created, cancellationToken, transaction);
            }
        }
    }

    private async Task ReconcilePepsAsync(Guid studentId, List<PepUpsertRequest> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing = (await pepRepository.GetByStudentIdAsync(studentId, cancellationToken, transaction)).ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            // Contributors of a removed PEP go with it.
            await ReconcilePepContributorsAsync(row.Id, [], transaction, cancellationToken);
            await pepRepository.DeleteAsync(row.Id, cancellationToken, false, transaction);
        }

        foreach (var item in incoming)
        {
            Guid pepId;

            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplyPep(entity, item);
                await pepRepository.UpdateAsync(entity, cancellationToken, transaction);
                pepId = entity.Id;
            }
            else
            {
                var created = new StudentPep { Id = SqlConvention.SequentialGuid(), StudentId = studentId };
                ApplyPep(created, item);
                await pepRepository.InsertAsync(created, cancellationToken, transaction);
                pepId = created.Id;
            }

            await ReconcilePepContributorsAsync(pepId, item.ContributorPersonIds, transaction, cancellationToken);
        }
    }

    private async Task ReconcilePepContributorsAsync(Guid pepId, IEnumerable<Guid> incomingPersonIds,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var desired = incomingPersonIds.ToHashSet();
        var existing = (await pepContributorRepository.GetByPepIdAsync(pepId, cancellationToken, transaction))
            .ToList();
        var existingPersonIds = existing.Select(c => c.PersonId).ToHashSet();

        foreach (var row in existing.Where(c => !desired.Contains(c.PersonId)))
        {
            await pepContributorRepository.DeleteAsync(row.Id, cancellationToken, false, transaction);
        }

        foreach (var personId in desired.Where(id => !existingPersonIds.Contains(id)))
        {
            await pepContributorRepository.InsertAsync(new StudentPepContributor
            {
                Id = SqlConvention.SequentialGuid(),
                StudentPepId = pepId,
                PersonId = personId
            }, cancellationToken, transaction);
        }
    }

    private async Task ReconcileChildProtectionPlansAsync(Guid studentId,
        List<ChildProtectionPlanUpsertRequest> incoming, IDbTransaction? transaction,
        CancellationToken cancellationToken)
    {
        var existing =
            (await childProtectionPlanRepository.GetByStudentIdAsync(studentId, cancellationToken, transaction))
            .ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await childProtectionPlanRepository.DeleteAsync(row.Id, cancellationToken, false, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplyChildProtectionPlan(entity, item);
                await childProtectionPlanRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var created = new StudentChildProtectionPlan
                {
                    Id = SqlConvention.SequentialGuid(), StudentId = studentId
                };
                ApplyChildProtectionPlan(created, item);
                await childProtectionPlanRepository.InsertAsync(created, cancellationToken, transaction);
            }
        }
    }

    private static void ApplyCareEpisode(StudentCareEpisode row, CareEpisodeUpsertRequest item)
    {
        row.CaringAuthorityId = item.CaringAuthorityId;
        row.LivingArrangementId = item.LivingArrangementId;
        row.StartDate = item.StartDate;
        row.EndDate = item.EndDate;
        row.Comment = string.IsNullOrWhiteSpace(item.Comment) ? null : item.Comment.Trim();
    }

    private static void ApplyPep(StudentPep row, PepUpsertRequest item)
    {
        row.StartDate = item.StartDate;
        row.EndDate = item.EndDate;
        row.Comment = string.IsNullOrWhiteSpace(item.Comment) ? null : item.Comment.Trim();
    }

    private static void ApplyChildProtectionPlan(StudentChildProtectionPlan row,
        ChildProtectionPlanUpsertRequest item)
    {
        row.LocalAuthorityId = item.LocalAuthorityId;
        row.StartDate = item.StartDate;
        row.EndDate = item.EndDate;
        row.Comment = string.IsNullOrWhiteSpace(item.Comment) ? null : item.Comment.Trim();
    }

    private static CareEpisodeResponse MapCareEpisode(StudentCareEpisode e) => new()
    {
        Id = e.Id,
        CaringAuthorityId = e.CaringAuthorityId,
        LivingArrangementId = e.LivingArrangementId,
        StartDate = e.StartDate,
        EndDate = e.EndDate,
        Comment = e.Comment
    };

    private static PepResponse MapPep(StudentPep p, IEnumerable<PepContributorResponse> contributors) => new()
    {
        Id = p.Id,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        Comment = p.Comment,
        Contributors = contributors.ToList()
    };

    private static ChildProtectionPlanResponse MapChildProtectionPlan(StudentChildProtectionPlan p) => new()
    {
        Id = p.Id,
        LocalAuthorityId = p.LocalAuthorityId,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        Comment = p.Comment
    };

    // LocalAuthority is a plain entity (Name, not a LookupEntity Description), so it can't use the
    // ToAlphabeticalLookup helper — project it into the shared LookupResponse shape by hand.
    private static List<LookupResponse> MapLocalAuthorities(IEnumerable<LocalAuthority> localAuthorities) =>
        localAuthorities
            .OrderBy(la => la.Name)
            .Select(la => new LookupResponse { Id = la.Id, Description = la.Name })
            .ToList();

    private async Task<Core.Entities.Student> LoadAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByIdAsync(studentId, cancellationToken);

        if (student == null)
        {
            throw new NotFoundException("Student not found.");
        }

        return student;
    }
}
