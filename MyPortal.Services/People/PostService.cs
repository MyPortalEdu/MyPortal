using System.Data;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
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

public class PostService(
    IAuthorizationService authorizationService,
    ILogger<PostService> logger,
    IPostRepository postRepository,
    IPostCategoryRepository postCategoryRepository,
    IVacancyRepository vacancyRepository,
    IServiceTermRepository serviceTermRepository,
    ISubjectRepository subjectRepository,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : BaseService(authorizationService, logger), IPostService
{
    public async Task<PostsResponse> GetPostsAsync(CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewStaffSetup, cancellationToken);

        var posts = (await postRepository.GetAllWithUsageAsync(cancellationToken)).ToList();

        var vacancies = await vacancyRepository.GetByPostIdsAsync(posts.Select(p => p.Id), cancellationToken);
        var vacanciesByPost = vacancies
            .GroupBy(v => v.PostId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var categories = await postCategoryRepository.GetListAsync(cancellationToken: cancellationToken);
        var serviceTerms = await serviceTermRepository.GetListAsync(cancellationToken: cancellationToken);
        var subjects = await subjectRepository.GetListAsync(cancellationToken: cancellationToken);

        var canEdit = await AuthorizationService.HasPermissionAsync(Permissions.Staff.EditStaffSetup,
            cancellationToken);

        return new PostsResponse
        {
            Posts = posts.Select(p =>
            {
                var rows = vacanciesByPost.TryGetValue(p.Id, out var v) ? v : [];

                return new PostResponse
                {
                    Id = p.Id,
                    Reference = p.Reference,
                    Description = p.Description,
                    PostCategoryId = p.PostCategoryId,
                    ServiceTermId = p.ServiceTermId,
                    SwrPostCode = p.SwrPostCode,
                    EstablishedFte = p.EstablishedFte,
                    ContractCount = p.ContractCount,
                    IsVacant = rows.Any(x => x.EndDate == null),
                    Vacancies = rows.Select(MapVacancy).ToList()
                };
            }).ToList(),
            PostCategories = categories.ToOrderedLookup(),
            ServiceTerms = serviceTerms.ToAlphabeticalLookup(),
            // Subject isn't a LookupEntity (Name/Code, not Description) — map by hand.
            Subjects = subjects
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Name)
                .Select(s => new LookupResponse { Id = s.Id, Description = s.Name })
                .ToList(),
            CanEdit = canEdit
        };
    }

    public async Task<Guid> CreatePostAsync(PostUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditStaffSetup, cancellationToken);
        await validationService.ValidateAsync(model);

        var postId = SqlConvention.SequentialGuid();

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await EnsureReferenceUniqueAsync(model.Reference, null, cancellationToken, uow.Transaction);

            var post = new Post { Id = postId };
            Apply(post, model);
            await postRepository.InsertAsync(post, cancellationToken, uow.Transaction);

            await ReconcileVacanciesAsync(postId, model.Vacancies, uow.Transaction, cancellationToken);
        }, cancellationToken);

        return postId;
    }

    public async Task UpdatePostAsync(Guid id, PostUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditStaffSetup, cancellationToken);
        await validationService.ValidateAsync(model);

        var post = await postRepository.GetByIdAsync(id, cancellationToken)
                   ?? throw new NotFoundException("Post not found.");

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await EnsureReferenceUniqueAsync(model.Reference, id, cancellationToken, uow.Transaction);

            Apply(post, model);
            await postRepository.UpdateAsync(post, cancellationToken, uow.Transaction);

            await ReconcileVacanciesAsync(id, model.Vacancies, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    public async Task DeletePostAsync(Guid id, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditStaffSetup, cancellationToken);

        var post = await postRepository.GetByIdAsync(id, cancellationToken)
                   ?? throw new NotFoundException("Post not found.");

        // A post still carrying contracts is part of the live establishment — deleting it would
        // orphan them, so block rather than cascade.
        var rows = await postRepository.GetAllWithUsageAsync(cancellationToken);
        var usage = rows.FirstOrDefault(r => r.Id == id);

        if (usage is { ContractCount: > 0 })
        {
            throw new EntityInUseException(
                $"This post is held by {usage.ContractCount} contract(s) and cannot be deleted.");
        }

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var vacancies = await vacancyRepository.GetByPostIdsAsync(new[] { id }, cancellationToken,
                uow.Transaction);

            foreach (var vacancy in vacancies)
            {
                await vacancyRepository.DeleteAsync(vacancy.Id, cancellationToken, true, uow.Transaction);
            }

            await postRepository.DeleteAsync(post.Id, cancellationToken, true, uow.Transaction);
        }, cancellationToken);
    }

    private async Task EnsureReferenceUniqueAsync(string reference, Guid? excludePostId,
        CancellationToken cancellationToken, IDbTransaction? transaction)
    {
        if (await postRepository.ReferenceExistsAsync(reference, excludePostId, cancellationToken, transaction))
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(PostUpsertRequest.Reference),
                    $"Post reference '{reference}' is already in use.")]);
        }
    }

    private async Task ReconcileVacanciesAsync(Guid postId, List<VacancyUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing = (await vacancyRepository.GetByPostIdsAsync(new[] { postId }, cancellationToken, transaction))
            .ToList();
        var existingById = existing.ToDictionary(v => v.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await vacancyRepository.DeleteAsync(row.Id, cancellationToken, true, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplyVacancy(entity, item);
                await vacancyRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var vacancy = new Vacancy { Id = SqlConvention.SequentialGuid(), PostId = postId };
                ApplyVacancy(vacancy, item);
                await vacancyRepository.InsertAsync(vacancy, cancellationToken, transaction);
            }
        }
    }

    private static void Apply(Post entity, PostUpsertRequest model)
    {
        entity.Reference = model.Reference.Trim();
        entity.Description = model.Description.Trim();
        entity.PostCategoryId = model.PostCategoryId;
        entity.ServiceTermId = model.ServiceTermId;
        entity.SwrPostCode = model.SwrPostCode;
        entity.EstablishedFte = model.EstablishedFte;
    }

    private static void ApplyVacancy(Vacancy entity, VacancyUpsertItem item)
    {
        entity.StartDate = item.StartDate;
        entity.EndDate = item.EndDate;
        entity.IsAdvertised = item.IsAdvertised;
        entity.IsTemporarilyFilled = item.IsTemporarilyFilled;
        entity.SubjectId = item.SubjectId;
        entity.Notes = item.Notes;
    }

    private static VacancyResponse MapVacancy(Vacancy v) => new()
    {
        Id = v.Id,
        StartDate = v.StartDate,
        EndDate = v.EndDate,
        IsAdvertised = v.IsAdvertised,
        IsTemporarilyFilled = v.IsTemporarilyFilled,
        SubjectId = v.SubjectId,
        Notes = v.Notes
    };
}
