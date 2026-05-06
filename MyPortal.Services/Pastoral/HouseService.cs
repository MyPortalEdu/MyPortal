using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Pastoral;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces.Pastoral;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.Pastoral;

public class HouseService : BaseService, IHouseService
{
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IHouseRepository _houseRepository;
    private readonly IStudentGroupService _studentGroupService;

    public HouseService(IAuthorizationService authorizationService, ILogger<HouseService> logger,
        IUnitOfWorkFactory unitOfWorkFactory, IHouseRepository houseRepository,
        IStudentGroupService studentGroupService) : base(authorizationService, logger)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _houseRepository = houseRepository;
        _studentGroupService = studentGroupService;
    }

    public async Task<PageResult<HouseSummaryResponse>> GetSummariesAsync(Guid academicYearId,
        FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.ViewPastoralStructure,
            cancellationToken);

        return await _houseRepository.GetSummariesAsync(academicYearId, filter, sort, paging,
            cancellationToken);
    }

    public async Task<HouseDetailsResponse> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.ViewPastoralStructure,
            cancellationToken);

        var result = await _houseRepository.GetDetailsByIdAsync(id, cancellationToken)
                     ?? throw new NotFoundException("House not found.");

        result.Header.Supervisors = result.Supervisors;
        return result.Header;
    }

    public async Task<Guid> CreateHouseAsync(HouseUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.EditPastoralStructure,
            cancellationToken);

        return await _unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var studentGroupId = await _studentGroupService.CreateAsync(model.AcademicYearId,
                ToCore(model), model.Supervisors, cancellationToken, uow);

            var house = new House
            {
                Id = SqlConvention.SequentialGuid(),
                StudentGroupId = studentGroupId,
                ColourCode = model.ColourCode
            };

            await _houseRepository.InsertAsync(house, cancellationToken, uow.Transaction);

            return house.Id;
        }, cancellationToken);
    }

    public async Task UpdateHouseAsync(Guid id, HouseUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.EditPastoralStructure,
            cancellationToken);

        await _unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var house = await GetHouseAsync(id, cancellationToken);

            await _studentGroupService.UpdateAsync(house.StudentGroupId, ToCore(model), model.Supervisors,
                cancellationToken, uow);

            house.ColourCode = model.ColourCode;
            await _houseRepository.UpdateAsync(house, cancellationToken, uow.Transaction);
        }, cancellationToken);
    }

    public async Task DeleteHouseAsync(Guid id, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.School.EditPastoralStructure,
            cancellationToken);

        await _unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var house = await GetHouseAsync(id, cancellationToken);

            // Houses.StudentGroupId FKs to StudentGroups, so the House row must go before the
            // StudentGroup. StudentGroupService.DeleteAsync runs the lock + downstream-data
            // gates at the top, so a failure there rolls back the House delete via the UoW.
            await _houseRepository.DeleteAsync(house.Id, cancellationToken, transaction: uow.Transaction);

            await _studentGroupService.DeleteAsync(house.StudentGroupId, cancellationToken, uow);
        }, cancellationToken);
    }

    private async Task<House> GetHouseAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _houseRepository.GetByIdAsync(id, cancellationToken)
               ?? throw new NotFoundException("House not found.");
    }

    private static StudentGroupUpsertCore ToCore(HouseUpsertRequest model) => new()
    {
        Code = model.Code,
        Name = model.Name,
        Active = model.Active,
        Notes = model.Notes
    };
}
