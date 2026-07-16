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
/// Staff-flavoured wrapper over <see cref="IPersonEqualityService"/>: enforces staff access under
/// <see cref="StaffArea.EqualityDetails"/> (HR-only edit, self/HR view — no Managed scope),
/// resolves the staff member to a person, delegates the person fields, and owns the staff-level
/// disability declaration (flag + free text) and the multi-select disability links.
/// </summary>
public class StaffEqualityService : BaseService, IStaffEqualityService
{
    private readonly IStaffMemberAccessService _accessService;
    private readonly IStaffMemberRepository _staffMemberRepository;
    private readonly IPersonEqualityService _personEqualityService;
    private readonly IDisabilityRepository _disabilityRepository;
    private readonly IStaffMemberDisabilityRepository _staffMemberDisabilityRepository;
    private readonly IValidationService _validationService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public StaffEqualityService(IAuthorizationService authorizationService, ILogger<StaffEqualityService> logger,
        IStaffMemberAccessService accessService, IStaffMemberRepository staffMemberRepository,
        IPersonEqualityService personEqualityService, IDisabilityRepository disabilityRepository,
        IStaffMemberDisabilityRepository staffMemberDisabilityRepository, IValidationService validationService,
        IUnitOfWorkFactory unitOfWorkFactory) : base(authorizationService, logger)
    {
        _accessService = accessService;
        _staffMemberRepository = staffMemberRepository;
        _personEqualityService = personEqualityService;
        _disabilityRepository = disabilityRepository;
        _staffMemberDisabilityRepository = staffMemberDisabilityRepository;
        _validationService = validationService;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<StaffEqualityDetailsResponse> GetEqualityDetailsAsync(Guid staffMemberId,
        CancellationToken cancellationToken)
    {
        // Special-category data: HR (All) or the person themselves (Own). No Managed scope.
        await _accessService.RequireAsync(staffMemberId, StaffArea.EqualityDetails,
            StaffAccess.ViewOwn | StaffAccess.ViewAll, cancellationToken);

        var staffMember = await _staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        var response = await _personEqualityService.GetEqualityDetailsAsync(staffMember.PersonId, cancellationToken);

        // Staff-level disability bits the person service doesn't own.
        response.HasDisability = staffMember.HasDisability;
        response.DisabilityDetails = staffMember.DisabilityDetails;

        var links = await _staffMemberDisabilityRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken);
        response.DisabilityIds = links.Select(l => l.DisabilityId).ToList();

        var disabilities = await _disabilityRepository.GetListAsync(cancellationToken: cancellationToken);
        response.Disabilities = disabilities.ToOrderedLookup();

        return response;
    }

    public async Task UpdateEqualityDetailsAsync(Guid staffMemberId, StaffEqualityDetailsUpsertRequest model,
        CancellationToken cancellationToken)
    {
        // Equality data is HR-edit-only — no Own/Managed edit scope.
        await _accessService.RequireAsync(staffMemberId, StaffArea.EqualityDetails, StaffAccess.EditAll,
            cancellationToken);

        await _validationService.ValidateAsync(model);

        var staffMember = await _staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        staffMember.HasDisability = model.HasDisability;
        staffMember.DisabilityDetails = model.DisabilityDetails;

        await _unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await _personEqualityService.UpdateEqualityDetailsAsync(staffMember.PersonId, model, cancellationToken, uow);
            await _staffMemberRepository.UpdateAsync(staffMember, cancellationToken, uow.Transaction);
            await ReconcileDisabilitiesAsync(staffMemberId, model.DisabilityIds, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    private async Task ReconcileDisabilitiesAsync(Guid staffMemberId, List<Guid> incoming, IDbTransaction? transaction,
        CancellationToken cancellationToken)
    {
        var wanted = incoming.Distinct().ToHashSet();

        var existing =
            await _staffMemberDisabilityRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken, transaction);
        var existingIds = existing.Select(e => e.DisabilityId).ToHashSet();

        // Hard-delete links the payload dropped (the join row carries no soft-delete column).
        foreach (var row in existing)
        {
            if (!wanted.Contains(row.DisabilityId))
            {
                await _staffMemberDisabilityRepository.DeleteAsync(row.Id, cancellationToken, false, transaction);
            }
        }

        foreach (var disabilityId in wanted)
        {
            if (!existingIds.Contains(disabilityId))
            {
                await _staffMemberDisabilityRepository.InsertAsync(new StaffMemberDisability
                {
                    Id = SqlConvention.SequentialGuid(),
                    StaffMemberId = staffMemberId,
                    DisabilityId = disabilityId
                }, cancellationToken, transaction);
            }
        }
    }
}
