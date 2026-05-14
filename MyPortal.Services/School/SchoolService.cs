using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Constants;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Agencies;
using MyPortal.Contracts.Models.School;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces.Agencies;
using MyPortal.Services.Interfaces.School;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using QueryKit.Sql;

namespace MyPortal.Services.School;

public class SchoolService : BaseService, ISchoolService
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IAgencyService _agencyService;

    public SchoolService(IAuthorizationService authorizationService, ILogger<BaseService> logger,
        ISchoolRepository schoolRepository, IUnitOfWorkFactory unitOfWorkFactory, IAgencyService agencyService) : base(
        authorizationService, logger)
    {
        _schoolRepository = schoolRepository;
        _unitOfWorkFactory = unitOfWorkFactory;
        _agencyService = agencyService;
    }

    public Task<PageResult<SchoolDetailsResponse>> GetListPagedAsync(FilterOptions? filter, SortOptions? sort, int page,
        int pageSize, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    // Anonymous endpoint (login page header) hits this — keep it un-gated. The
    // authenticated controller endpoint applies its own [Permission] attribute.
    public async Task<SchoolDetailsResponse?> GetLocalSchoolDetailsAsync(CancellationToken cancellationToken)
    {
        return await _schoolRepository.GetLocalSchoolAsync(cancellationToken);
    }

    public async Task<SchoolDetailsResponse?> GetSchoolByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Agencies.ViewAgencies, cancellationToken);

        return await _schoolRepository.GetDetailsByIdAsync(id, cancellationToken);
    }

    public async Task<Guid> CreateAsync(SchoolUpsertRequest model, CancellationToken cancellationToken,
        IUnitOfWork? uow = null)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Agencies.EditAgencies, cancellationToken);

        return await CreateInternalAsync(model, isLocal: false, uow, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, SchoolUpsertRequest model, CancellationToken cancellationToken,
        IUnitOfWork? uow = null)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Agencies.EditAgencies, cancellationToken);

        await _unitOfWorkFactory.RunInTransactionAsync(uow, async ownedUow =>
        {
            var school = await GetEntityByIdAsync(id, cancellationToken);

            await _agencyService.UpdateAsync(school.AgencyId, BuildAgencyRequest(model), cancellationToken, ownedUow);

            school.EstablishmentNumber = model.EstablishmentNumber;
            school.Urn = model.Urn;
            school.Uprn = model.Uprn;
            school.LocalAuthorityId = model.LocalAuthorityId;
            school.SchoolPhaseId = model.SchoolPhaseId;
            school.GovernanceTypeId = model.GovernanceTypeId;
            school.IntakeTypeId = model.IntakeTypeId;
            school.SchoolTypeId = model.SchoolTypeId;
            school.HeadTeacherId = model.HeadTeacherId;

            await _schoolRepository.UpdateAsync(school, cancellationToken, ownedUow.Transaction);
        }, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken, IUnitOfWork? uow = null)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Agencies.EditAgencies, cancellationToken);

        var school = await GetEntityByIdAsync(id, cancellationToken);

        if (school.IsLocal)
        {
            throw new InvalidOperationException("Cannot delete the local school.");
        }

        await _unitOfWorkFactory.RunInTransactionAsync(uow, async ownedUow =>
        {
            // School has an FK to Agency, so delete the school row first to release the
            // reference before the agency (and its directory) is torn down.
            await _schoolRepository.DeleteAsync(id, cancellationToken, false, ownedUow.Transaction);

            await _agencyService.DeleteAsync(school.AgencyId, cancellationToken, ownedUow);
        }, cancellationToken);
    }

    public async Task<Guid> CreateOrUpdateLocalSchoolAsync(SchoolUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Agencies.EditAgencies, cancellationToken);

        return await _unitOfWorkFactory.RunInTransactionAsync<Guid>(null, async ownedUow =>
        {
            var existing = await _schoolRepository.GetLocalSchoolAsync(cancellationToken);

            if (existing == null)
            {
                // Create with IsLocal=true in the single insert — no flip-update needed.
                return await CreateInternalAsync(model, isLocal: true, ownedUow, cancellationToken);
            }

            await UpdateAsync(existing.Id, model, cancellationToken, ownedUow);
            return existing.Id;
        }, cancellationToken);
    }

    private async Task<Guid> CreateInternalAsync(SchoolUpsertRequest model, bool isLocal, IUnitOfWork? uow,
        CancellationToken cancellationToken)
    {
        return await _unitOfWorkFactory.RunInTransactionAsync(uow, async ownedUow =>
        {
            var agencyId = await _agencyService.CreateAsync(BuildAgencyRequest(model), cancellationToken, ownedUow);

            var schoolId = SqlConvention.SequentialGuid();

            var school = new Core.Entities.School
            {
                Id = schoolId,
                AgencyId = agencyId,
                EstablishmentNumber = model.EstablishmentNumber,
                Urn = model.Urn,
                Uprn = model.Uprn,
                LocalAuthorityId = model.LocalAuthorityId,
                SchoolPhaseId = model.SchoolPhaseId,
                GovernanceTypeId = model.GovernanceTypeId,
                IntakeTypeId = model.IntakeTypeId,
                SchoolTypeId = model.SchoolTypeId,
                HeadTeacherId = model.HeadTeacherId,
                IsLocal = isLocal
            };

            await _schoolRepository.InsertAsync(school, cancellationToken, ownedUow.Transaction);

            return schoolId;
        }, cancellationToken);
    }

    // Schools are always backed by an agency of type "Educational Provider" — the
    // SPA doesn't pick a type, so the server fixes it here rather than trusting
    // an inbound AgencyTypeId.
    private static AgencyUpsertRequest BuildAgencyRequest(SchoolUpsertRequest model) => new()
    {
        AgencyTypeId = AgencyTypes.EducationalProvider,
        Name = model.Name,
        Website = model.Website,
        ExpectedVersion = model.ExpectedVersion
    };

    private async Task<Core.Entities.School> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var school = await _schoolRepository.GetByIdAsync(id, cancellationToken);

        if (school == null)
        {
            throw new NotFoundException("School not found.");
        }

        return school;
    }
}
