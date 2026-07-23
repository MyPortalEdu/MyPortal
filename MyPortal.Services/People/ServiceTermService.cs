using System.Data;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
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

public class ServiceTermService(
    IAuthorizationService authorizationService,
    ILogger<ServiceTermService> logger,
    IServiceTermRepository serviceTermRepository,
    IServiceTermSuperannuationSchemeRepository serviceTermSchemeRepository,
    ISuperannuationSchemeRepository superannuationSchemeRepository,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : BaseService(authorizationService, logger), IServiceTermService
{
    public async Task<ServiceTermsResponse> GetServiceTermsAsync(CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewStaffSetup, cancellationToken);

        var terms = (await serviceTermRepository.GetAllWithUsageAsync(cancellationToken)).ToList();

        var links = await serviceTermSchemeRepository.GetByServiceTermIdsAsync(terms.Select(t => t.Id),
            cancellationToken);
        var linksByTerm = links
            .GroupBy(l => l.ServiceTermId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var schemes = await superannuationSchemeRepository.GetListAsync(cancellationToken: cancellationToken);

        var canEdit = await AuthorizationService.HasPermissionAsync(Permissions.Staff.EditStaffSetup,
            cancellationToken);

        return new ServiceTermsResponse
        {
            ServiceTerms = terms.Select(t => new ServiceTermResponse
            {
                Id = t.Id,
                Code = t.Code,
                Description = t.Description,
                Active = t.Active,
                IsTeacher = t.IsTeacher,
                Salaried = t.Salaried,
                SpinalProgression = t.SpinalProgression,
                SinglePaySpine = t.SinglePaySpine,
                TermTimeOnlyPossible = t.TermTimeOnlyPossible,
                IncrementMonth = t.IncrementMonth,
                IncrementDay = t.IncrementDay,
                MinimumPoint = t.MinimumPoint,
                MaximumPoint = t.MaximumPoint,
                PointInterval = t.PointInterval,
                HoursPerWeek = t.HoursPerWeek,
                WeeksPerYear = t.WeeksPerYear,
                ContractCount = t.ContractCount,
                PostCount = t.PostCount,
                SuperannuationSchemes = (linksByTerm.TryGetValue(t.Id, out var rows) ? rows : [])
                    .Select(l => new ServiceTermSchemeItem
                    {
                        SuperannuationSchemeId = l.SuperannuationSchemeId,
                        IsMain = l.IsMain
                    })
                    .ToList()
            }).ToList(),
            SuperannuationSchemes = schemes.ToOrderedLookup(),
            CanEdit = canEdit
        };
    }

    public async Task<Guid> CreateServiceTermAsync(ServiceTermUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditStaffSetup, cancellationToken);
        await validationService.ValidateAsync(model);

        var serviceTermId = SqlConvention.SequentialGuid();

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await EnsureCodeUniqueAsync(model.Code, null, cancellationToken, uow.Transaction);

            var term = new ServiceTerm { Id = serviceTermId };
            Apply(term, model);
            await serviceTermRepository.InsertAsync(term, cancellationToken, uow.Transaction);

            await ReconcileSchemesAsync(serviceTermId, model.SuperannuationSchemes, uow.Transaction,
                cancellationToken);
        }, cancellationToken);

        return serviceTermId;
    }

    public async Task UpdateServiceTermAsync(Guid id, ServiceTermUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditStaffSetup, cancellationToken);
        await validationService.ValidateAsync(model);

        var term = await serviceTermRepository.GetByIdAsync(id, cancellationToken)
                   ?? throw new NotFoundException("Service term not found.");

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await EnsureCodeUniqueAsync(model.Code, id, cancellationToken, uow.Transaction);

            Apply(term, model);
            await serviceTermRepository.UpdateAsync(term, cancellationToken, uow.Transaction);

            await ReconcileSchemesAsync(id, model.SuperannuationSchemes, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    public async Task DeleteServiceTermAsync(Guid id, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditStaffSetup, cancellationToken);

        var term = await serviceTermRepository.GetByIdAsync(id, cancellationToken)
                   ?? throw new NotFoundException("Service term not found.");

        // Contracts and posts both point at a service term; removing one still in use would strand
        // them, so block and let the user deactivate instead.
        var rows = await serviceTermRepository.GetAllWithUsageAsync(cancellationToken);
        var usage = rows.FirstOrDefault(r => r.Id == id);

        if (usage is { ContractCount: > 0 })
        {
            throw new EntityInUseException(
                $"This service term is used by {usage.ContractCount} contract(s) and cannot be deleted.");
        }

        if (usage is { PostCount: > 0 })
        {
            throw new EntityInUseException(
                $"This service term is used by {usage.PostCount} post(s) and cannot be deleted.");
        }

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var links = await serviceTermSchemeRepository.GetByServiceTermIdsAsync(new[] { id }, cancellationToken,
                uow.Transaction);

            foreach (var link in links)
            {
                await serviceTermSchemeRepository.DeleteAsync(link.Id, cancellationToken, true, uow.Transaction);
            }

            await serviceTermRepository.DeleteAsync(term.Id, cancellationToken, true, uow.Transaction);
        }, cancellationToken);
    }

    private async Task EnsureCodeUniqueAsync(string code, Guid? excludeServiceTermId,
        CancellationToken cancellationToken, IDbTransaction? transaction)
    {
        if (await serviceTermRepository.CodeExistsAsync(code, excludeServiceTermId, cancellationToken, transaction))
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(ServiceTermUpsertRequest.Code),
                    $"Service term code '{code}' is already in use.")]);
        }
    }

    private async Task ReconcileSchemesAsync(Guid serviceTermId, List<ServiceTermSchemeItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing =
            (await serviceTermSchemeRepository.GetByServiceTermIdsAsync(new[] { serviceTermId }, cancellationToken,
                transaction))
            .ToList();
        var incomingBySchemeId = incoming.ToDictionary(i => i.SuperannuationSchemeId);

        // Drop removed links first: the one-main-per-term index would reject an insert that
        // collides with a row still due for deletion.
        foreach (var row in existing.Where(row => !incomingBySchemeId.ContainsKey(row.SuperannuationSchemeId)))
        {
            await serviceTermSchemeRepository.DeleteAsync(row.Id, cancellationToken, true, transaction);
        }

        // Clear main flags before setting the new one, for the same reason.
        foreach (var row in existing.Where(row => row.IsMain))
        {
            if (incomingBySchemeId.TryGetValue(row.SuperannuationSchemeId, out var stillMain) && stillMain.IsMain)
            {
                continue;
            }

            row.IsMain = false;
            await serviceTermSchemeRepository.UpdateAsync(row, cancellationToken, transaction);
        }

        var existingBySchemeId = existing.ToDictionary(e => e.SuperannuationSchemeId);

        foreach (var item in incoming)
        {
            if (existingBySchemeId.TryGetValue(item.SuperannuationSchemeId, out var entity))
            {
                if (entity.IsMain == item.IsMain)
                {
                    continue;
                }

                entity.IsMain = item.IsMain;
                await serviceTermSchemeRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                await serviceTermSchemeRepository.InsertAsync(new ServiceTermSuperannuationScheme
                {
                    Id = SqlConvention.SequentialGuid(),
                    ServiceTermId = serviceTermId,
                    SuperannuationSchemeId = item.SuperannuationSchemeId,
                    IsMain = item.IsMain
                }, cancellationToken, transaction);
            }
        }
    }

    private static void Apply(ServiceTerm entity, ServiceTermUpsertRequest model)
    {
        entity.Code = model.Code.Trim().ToUpperInvariant();
        entity.Description = model.Description.Trim();
        entity.Active = model.Active;
        entity.IsTeacher = model.IsTeacher;
        entity.Salaried = model.Salaried;
        entity.SpinalProgression = model.SpinalProgression;
        entity.TermTimeOnlyPossible = model.TermTimeOnlyPossible;
        // Increment timing only means anything with spinal progression on.
        entity.IncrementMonth = model.SpinalProgression ? model.IncrementMonth : null;
        entity.IncrementDay = model.SpinalProgression ? model.IncrementDay : null;
        // SinglePaySpine and the point range belong to the Pay Spine panel: changing them moves
        // points between owners, which only that save can reconcile atomically.
        entity.HoursPerWeek = model.HoursPerWeek;
        entity.WeeksPerYear = model.WeeksPerYear;
    }
}
