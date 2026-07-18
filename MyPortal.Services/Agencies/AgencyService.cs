using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Agencies;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Documents;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Agencies;
using MyPortal.Services.Interfaces.Documents;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.Agencies;

public class AgencyService(
    IAuthorizationService authorizationService,
    ILogger<DirectoryEntityService<Agency>> logger,
    IDirectoryService directoryService,
    IDocumentService documentService,
    IValidationService validationService,
    IAgencyRepository agencyRepository,
    IUnitOfWorkFactory unitOfWorkFactory)
    : DirectoryEntityService<Agency>(authorizationService, logger,
        directoryService, documentService, validationService), IAgencyService
{
    public Task<PageResult<AgencySummaryResponse>> GetListPagedAsync(FilterOptions? filter, SortOptions? sort, int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<AgencyDetailsResponse> GetDetailsByIdAsync(int id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Guid> CreateAsync(AgencyUpsertRequest model, CancellationToken cancellationToken,
        IUnitOfWork? uow = null)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Agencies.EditAgencies, cancellationToken);

        var agencyId = SqlConvention.SequentialGuid();

        var directoryRequest = new DirectoryUpsertRequest
        {
            Name = $"agency-{agencyId:N}",
            IsPrivate = true,
            UploadPolicy = DirectoryUploadPolicy.StaffOnly
        };

        return await unitOfWorkFactory.RunInTransactionAsync(uow, async ownedUow =>
        {
            var directory = await DirectoryService.CreateAsync(directoryRequest, cancellationToken, ownedUow);

            var agency = new Agency
            {
                Id = agencyId,
                AgencyTypeId = model.AgencyTypeId,
                DirectoryId = directory.Id,
                Name = model.Name,
                Website = model.Website,
                IsDeleted = false
            };

            await agencyRepository.InsertAsync(agency, cancellationToken, ownedUow.Transaction);

            return agencyId;
        }, cancellationToken);
    }

    public async Task UpdateAsync(Guid agencyId, AgencyUpsertRequest model,
        CancellationToken cancellationToken, IUnitOfWork? uow = null)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Agencies.EditAgencies, cancellationToken);

        var agency = await GetByIdAsync(agencyId, cancellationToken);

        agency.AgencyTypeId = model.AgencyTypeId;
        agency.Name = model.Name;
        agency.Website = model.Website;

        await unitOfWorkFactory.RunInTransactionAsync(uow, async ownedUow =>
        {
            await agencyRepository.UpdateAsync(agency, cancellationToken, ownedUow.Transaction);
        }, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken, IUnitOfWork? uow = null)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Agencies.EditAgencies, cancellationToken);

        var agency = await GetByIdAsync(id, cancellationToken);

        await unitOfWorkFactory.RunInTransactionAsync(uow, async ownedUow =>
        {
            // Order matches BulletinService: the agency row holds an FK to its directory,
            // so deleting the directory first throws a REFERENCE-constraint error once
            // the directory has content. Drop the agency row to release the FK, then
            // hard-delete the directory (and its subtree) in the same transaction.
            await agencyRepository.DeleteAsync(id, cancellationToken, softDelete: false, ownedUow.Transaction);

            await DirectoryService.DeleteAsync(agency.DirectoryId, cancellationToken, ownedUow, softDelete: false);
        }, cancellationToken);
    }

    public override async Task<Agency> GetByIdAsync(Guid entityId, CancellationToken cancellationToken)
    {
        var agency = await agencyRepository.GetByIdAsync(entityId, cancellationToken);

        if (agency == null)
        {
            throw new NotFoundException("Agency not found");
        }

        return agency;
    }

    public override async Task<bool> CanViewDirectoryAsync(Guid entityId, Guid directoryId, CancellationToken ct)
    {
        // Bool-returning gate: return false on missing permission rather than throwing,
        // so callers using this as a predicate (DirectoryEntityService.GetDirectoryByIdAsync
        // etc.) see a 403 mapped from their own ForbiddenException — not an exception
        // from inside the gate.
        if (!await AuthorizationService.HasPermissionAsync(Permissions.Agencies.ViewAgencies, ct))
        {
            return false;
        }

        return await CanStructurallyViewDirectoryAsync(entityId, directoryId, ct);
    }

    public override async Task<bool> CanEditDirectoryAsync(Guid entityId, Guid directoryId, CancellationToken ct)
    {
        if (!await AuthorizationService.HasPermissionAsync(Permissions.Agencies.EditAgencies, ct))
        {
            return false;
        }

        return await CanStructurallyEditDirectoryAsync(entityId, directoryId, ct);
    }

    public override async Task<bool> CanUploadToDirectoryAsync(Guid entityId, Guid directoryId, CancellationToken ct)
    {
        if (!await AuthorizationService.HasPermissionAsync(Permissions.Agencies.EditAgencies, ct))
        {
            return false;
        }

        return await CanStructurallyUploadToDirectoryAsync(entityId, directoryId, ct);
    }
}
