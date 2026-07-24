using System.Data;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Models;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.People;
using MyPortal.Services.Interfaces.Providers;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

/// <summary>
/// Owns the Employment Details area: the bank / NI fields on the staff member plus the
/// employment-spell → contract hierarchy. Gating is enforced under
/// <see cref="StaffArea.EmploymentDetails"/> — self / HR view, HR-only edit. The save is a
/// whole-graph replace reconciled two levels deep (spells, then each spell's contracts).
/// </summary>
public class StaffEmploymentService(
    IAuthorizationService authorizationService,
    ILogger<StaffEmploymentService> logger,
    IStaffMemberAccessService accessService,
    IStaffMemberRepository staffMemberRepository,
    IStaffEmploymentRepository employmentRepository,
    IStaffContractRepository contractRepository,
    IStaffContractAllowanceRepository allowanceRepository,
    IAdditionalPaymentTypeRepository additionalPaymentTypeRepository,
    IStaffContractSuspensionRepository suspensionRepository,
    IStaffContractSalaryChangeRepository salaryChangeRepository,
    IPostRepository postRepository,
    ISuperannuationSchemeRepository superannuationSchemeRepository,
    ISuperannuationSchemeRateRepository superannuationSchemeRateRepository,
    ILeavingReasonRepository leavingReasonRepository,
    IStaffOriginRepository originRepository,
    IStaffDestinationRepository destinationRepository,
    IContractTypeRepository contractTypeRepository,
    IStaffRoleRepository staffRoleRepository,
    IServiceTermRepository serviceTermRepository,
    IServiceTermSuperannuationSchemeRepository serviceTermSchemeRepository,
    IDepartmentRepository departmentRepository,
    IPayScaleRepository payScaleRepository,
    IPayScalePointRepository payScalePointRepository,
    IPayScalePointRateRepository payScalePointRateRepository,
    IPayZoneRepository payZoneRepository,
    ISchoolRepository schoolRepository,
    IDateTimeProvider dateTimeProvider,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : BaseService(authorizationService, logger), IStaffEmploymentService
{
    public async Task<StaffEmploymentDetailsResponse> GetEmploymentDetailsAsync(Guid staffMemberId,
        CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(staffMemberId, StaffArea.EmploymentDetails,
            StaffAccess.ViewOwn | StaffAccess.ViewAll, cancellationToken);

        var staffMember = await staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        var employments =
            (await employmentRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken)).ToList();
        var contracts = await contractRepository.GetByEmploymentIdsAsync(employments.Select(e => e.Id),
            cancellationToken);
        var contractsByEmployment = contracts
            .GroupBy(c => c.StaffEmploymentId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var contractIds = contracts.Select(c => c.Id).ToList();

        var allowances = await allowanceRepository.GetByContractIdsAsync(contractIds, cancellationToken);
        var allowancesByContract = allowances
            .GroupBy(a => a.StaffContractId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var suspensions = await suspensionRepository.GetByContractIdsAsync(contractIds, cancellationToken);
        var suspensionsByContract = suspensions
            .GroupBy(s => s.StaffContractId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var salaryChanges = await salaryChangeRepository.GetByContractIdsAsync(contractIds, cancellationToken);
        var salaryChangesByContract = salaryChanges
            .GroupBy(s => s.StaffContractId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var leavingReasons = await leavingReasonRepository.GetListAsync(cancellationToken: cancellationToken);
        var origins = await originRepository.GetListAsync(cancellationToken: cancellationToken);
        var destinations = await destinationRepository.GetListAsync(cancellationToken: cancellationToken);
        var contractTypes = await contractTypeRepository.GetListAsync(cancellationToken: cancellationToken);
        var staffRoles = await staffRoleRepository.GetListAsync(cancellationToken: cancellationToken);
        var serviceTerms = await serviceTermRepository.GetListAsync(cancellationToken: cancellationToken);
        var departments = await departmentRepository.GetListAsync(cancellationToken: cancellationToken);
        var payScales = await payScaleRepository.GetListAsync(cancellationToken: cancellationToken);
        var payScalePoints = await payScalePointRepository.GetListAsync(cancellationToken: cancellationToken);
        var additionalPaymentTypes =
            await additionalPaymentTypeRepository.GetListAsync(cancellationToken: cancellationToken);

        // Pension schemes carry the employer rate in effect today, so the editor can show the
        // indicative employer cost next to the salary.
        var posts = await postRepository.GetAllWithUsageAsync(cancellationToken);

        var schemes = await superannuationSchemeRepository.GetListAsync(cancellationToken: cancellationToken);
        var schemeRates =
            await superannuationSchemeRateRepository.GetCurrentAsync(dateTimeProvider.UtcNow.Date, cancellationToken);
        var employerRateByScheme = schemeRates
            .OrderBy(r => r.EffectiveFrom)
            .GroupBy(r => r.SuperannuationSchemeId)
            .ToDictionary(g => g.Key, g => g.Last().EmployerRate);

        // Resolve the school's pay zone and the statutory salary in effect today for each spine
        // point, so the editor can pre-fill a contract's salary as (full-time rate × FTE).
        var payZoneId = await schoolRepository.GetLocalSchoolPayZoneIdAsync(cancellationToken);
        var fullTimeSalaryByPoint = new Dictionary<Guid, decimal>();
        string? payZoneName = null;

        if (payZoneId.HasValue)
        {
            var payZone = await payZoneRepository.GetByIdAsync(payZoneId.Value, cancellationToken);
            payZoneName = payZone?.Description;

            var rates = await payScalePointRateRepository.GetCurrentByZoneAsync(payZoneId.Value,
                dateTimeProvider.UtcNow.Date, cancellationToken);

            // Latest effective rate wins if more than one row qualifies.
            foreach (var rate in rates.OrderBy(r => r.EffectiveFrom))
            {
                fullTimeSalaryByPoint[rate.PayScalePointId] = rate.AnnualSalary;
            }
        }

        return new StaffEmploymentDetailsResponse
        {
            PayZoneId = payZoneId,
            PayZoneName = payZoneName,
            BankName = staffMember.BankName,
            BankAccount = staffMember.BankAccount,
            BankSortCode = staffMember.BankSortCode,
            NiNumber = staffMember.NiNumber,
            Employments = employments
                .OrderByDescending(e => e.StartDate)
                .Select(e => new StaffEmploymentResponse
                {
                    Id = e.Id,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    ContinuousServiceStartDate = e.ContinuousServiceStartDate,
                    LocalAuthorityStartDate = e.LocalAuthorityStartDate,
                    LeavingReasonId = e.LeavingReasonId,
                    OriginId = e.OriginId,
                    DestinationId = e.DestinationId,
                    PreviousEmployer = e.PreviousEmployer,
                    NextEmployer = e.NextEmployer,
                    Notes = e.Notes,
                    Contracts = (contractsByEmployment.TryGetValue(e.Id, out var rows) ? rows : new List<StaffContract>())
                        .OrderByDescending(c => c.StartDate)
                        .Select(c => MapContract(c, allowancesByContract, suspensionsByContract,
                            salaryChangesByContract))
                        .ToList()
                })
                .ToList(),
            LeavingReasons = leavingReasons.ToAlphabeticalLookup(),
            Origins = origins.ToOrderedLookup(),
            Destinations = destinations.ToOrderedLookup(),
            ContractTypes = contractTypes.ToOrderedLookup(),
            StaffRoles = staffRoles.ToOrderedLookup(),
            ServiceTerms = serviceTerms.ToAlphabeticalLookup(),
            Departments = departments.ToAlphabeticalLookup(),
            PayScales = payScales
                .WhereActive()
                .OrderBy(s => s.Description)
                .Select(s => new PayScaleResponse
                {
                    Id = s.Id,
                    ServiceTermId = s.ServiceTermId,
                    Description = s.Description
                })
                .ToList(),
            ServiceTermSchemes = (await serviceTermSchemeRepository
                    .GetByServiceTermIdsAsync(serviceTerms.Select(t => t.Id), cancellationToken))
                .Select(l => new ServiceTermSchemeLink
                {
                    ServiceTermId = l.ServiceTermId,
                    SuperannuationSchemeId = l.SuperannuationSchemeId,
                    IsMain = l.IsMain
                })
                .ToList(),
            ServiceTermDefaults = serviceTerms
                .Select(t => new ServiceTermDefaultsItem
                {
                    ServiceTermId = t.Id,
                    HoursPerWeek = t.HoursPerWeek,
                    WeeksPerYear = t.WeeksPerYear
                })
                .ToList(),
            AdditionalPaymentTypes = additionalPaymentTypes.ToOrderedLookup(),
            // Reference + title so the picker disambiguates similarly-titled posts.
            Posts = posts
                .Select(p => new LookupResponse { Id = p.Id, Description = $"{p.Reference} — {p.Description}" })
                .ToList(),
            SuperannuationSchemes = schemes
                .WhereActive()
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Description)
                .Select(s => new SuperannuationSchemeResponse
                {
                    Id = s.Id,
                    Description = s.Description,
                    EmployerRate = employerRateByScheme.TryGetValue(s.Id, out var rate) ? rate : null
                })
                .ToList(),
            PayScalePoints = BuildPointOptions(payScales, payScalePoints, fullTimeSalaryByPoint)
        };
    }

    /// <summary>
    /// The points selectable under each pay scale. A scale that owns its points offers those; on a
    /// term running a single spine the scale is a window, so it offers the term's points that fall
    /// inside it — which means an overlapping grade legitimately offers the same point as its
    /// neighbour.
    /// </summary>
    private static List<PayScalePointResponse> BuildPointOptions(IEnumerable<PayScale> payScales,
        IEnumerable<PayScalePoint> payScalePoints, Dictionary<Guid, decimal> fullTimeSalaryByPoint)
    {
        var points = payScalePoints.WhereActive().ToList();
        var scales = payScales.WhereActive().ToList();

        var spineByTerm = points
            .Where(p => p.ServiceTermId.HasValue)
            .GroupBy(p => p.ServiceTermId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var options = points
            .Where(p => p.PayScaleId.HasValue)
            .Select(p => (ScaleId: p.PayScaleId!.Value, Point: p))
            .ToList();

        foreach (var scale in scales)
        {
            if (!spineByTerm.TryGetValue(scale.ServiceTermId, out var spine))
            {
                continue;
            }

            options.AddRange(spine
                .Where(p => (scale.MinimumPoint is null || p.PointValue >= scale.MinimumPoint)
                            && (scale.MaximumPoint is null || p.PointValue <= scale.MaximumPoint))
                .Select(p => (ScaleId: scale.Id, Point: p)));
        }

        return options
            .OrderBy(o => o.Point.PointValue)
            .ThenBy(o => o.Point.Description)
            .Select(o => new PayScalePointResponse
            {
                Id = o.Point.Id,
                PayScaleId = o.ScaleId,
                PointValue = o.Point.PointValue,
                Description = o.Point.Description,
                FullTimeSalary = fullTimeSalaryByPoint.TryGetValue(o.Point.Id, out var salary) ? salary : null
            })
            .ToList();
    }

    public async Task UpdateEmploymentDetailsAsync(Guid staffMemberId, StaffEmploymentDetailsUpsertRequest model,
        CancellationToken cancellationToken)
    {
        // Employment data is the "crown jewels" — HR-edit only (All scope).
        await accessService.RequireAsync(staffMemberId, StaffArea.EmploymentDetails, StaffAccess.EditAll,
            cancellationToken);

        await validationService.ValidateAsync(model);

        var staffMember = await staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        await EnsureContractsMatchTheirServiceTermAsync(model, cancellationToken);

        staffMember.BankName = model.BankName;
        staffMember.BankAccount = model.BankAccount;
        staffMember.BankSortCode = model.BankSortCode;
        staffMember.NiNumber = model.NiNumber;

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await staffMemberRepository.UpdateAsync(staffMember, cancellationToken, uow.Transaction);
            await ReconcileEmploymentsAsync(staffMemberId, model.Employments, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    /// <summary>
    /// A contract's pay scale, point and pension scheme all hang off its service term. The editor
    /// cascades them, but nothing stops a caller posting a Burgundy contract on an NJC scale, so
    /// the combination is re-checked here rather than trusted from the client.
    /// </summary>
    private async Task EnsureContractsMatchTheirServiceTermAsync(StaffEmploymentDetailsUpsertRequest model,
        CancellationToken cancellationToken)
    {
        var contracts = model.Employments
            .SelectMany(e => e.Contracts)
            .Where(c => c.PayScaleId.HasValue || c.SuperannuationSchemeId.HasValue
                                              || c.PayScalePointId.HasValue)
            .ToList();

        if (contracts.Count == 0)
        {
            return;
        }

        var scales = (await payScaleRepository.GetListAsync(cancellationToken: cancellationToken))
            .ToDictionary(s => s.Id);
        var points = (await payScalePointRepository.GetListAsync(cancellationToken: cancellationToken))
            .ToDictionary(p => p.Id);
        var schemesByTerm = (await serviceTermSchemeRepository
                .GetByServiceTermIdsAsync(model.Employments
                    .SelectMany(e => e.Contracts)
                    .Where(c => c.ServiceTermId.HasValue)
                    .Select(c => c.ServiceTermId!.Value)
                    .Distinct(), cancellationToken))
            .GroupBy(l => l.ServiceTermId)
            .ToDictionary(g => g.Key, g => g.Select(l => l.SuperannuationSchemeId).ToHashSet());

        var failures = new List<ValidationFailure>();

        foreach (var contract in contracts)
        {
            if (contract.ServiceTermId is not { } serviceTermId)
            {
                failures.Add(new ValidationFailure(nameof(StaffContractUpsertItem.ServiceTermId),
                    "Choose a service term before a pay scale or pension scheme."));
                continue;
            }

            if (contract.PayScaleId is { } payScaleId
                && scales.TryGetValue(payScaleId, out var scale)
                && scale.ServiceTermId != serviceTermId)
            {
                failures.Add(new ValidationFailure(nameof(StaffContractUpsertItem.PayScaleId),
                    $"'{scale.Description}' does not belong to the contract's service term."));
            }

            // The point must sit under the chosen scale, or on the term's own spine when it runs one.
            if (contract.PayScalePointId is { } pointId && points.TryGetValue(pointId, out var point))
            {
                var belongs = point.PayScaleId is { } owner
                    ? owner == contract.PayScaleId
                    : point.ServiceTermId == serviceTermId;

                if (!belongs)
                {
                    failures.Add(new ValidationFailure(nameof(StaffContractUpsertItem.PayScalePointId),
                        $"Point '{point.Code}' is not on the contract's pay scale."));
                }
            }

            if (contract.SuperannuationSchemeId is { } schemeId
                && !(schemesByTerm.TryGetValue(serviceTermId, out var offered) && offered.Contains(schemeId)))
            {
                failures.Add(new ValidationFailure(nameof(StaffContractUpsertItem.SuperannuationSchemeId),
                    "That pension scheme is not offered on the contract's service term."));
            }
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }
    }

    private async Task ReconcileEmploymentsAsync(Guid staffMemberId, List<StaffEmploymentUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing =
            (await employmentRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken, transaction))
            .ToList();
        var existingById = existing.ToDictionary(e => e.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        // Soft-delete dropped spells — and their contracts first, so nothing is orphaned.
        var droppedEmploymentIds = existing.Where(e => !keptIds.Contains(e.Id)).Select(e => e.Id).ToList();

        if (droppedEmploymentIds.Count > 0)
        {
            var orphanedContracts =
                (await contractRepository.GetByEmploymentIdsAsync(droppedEmploymentIds, cancellationToken, transaction))
                .ToList();

            // Allowances hang off the contract, so they go first — nothing is left orphaned.
            await SoftDeleteAllowancesForContractsAsync(orphanedContracts.Select(c => c.Id), transaction,
                cancellationToken);
            await SoftDeleteSuspensionsForContractsAsync(orphanedContracts.Select(c => c.Id), transaction,
                cancellationToken);

            foreach (var contract in orphanedContracts)
            {
                await contractRepository.DeleteAsync(contract.Id, cancellationToken, true, transaction);
            }

            foreach (var employmentId in droppedEmploymentIds)
            {
                await employmentRepository.DeleteAsync(employmentId, cancellationToken, true, transaction);
            }
        }

        foreach (var item in incoming)
        {
            Guid employmentId;

            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                entity.StartDate = item.StartDate;
                entity.EndDate = item.EndDate;
                entity.ContinuousServiceStartDate = item.ContinuousServiceStartDate;
                entity.LocalAuthorityStartDate = item.LocalAuthorityStartDate;
                entity.LeavingReasonId = item.LeavingReasonId;
                entity.OriginId = item.OriginId;
                entity.DestinationId = item.DestinationId;
                entity.PreviousEmployer = item.PreviousEmployer;
                entity.NextEmployer = item.NextEmployer;
                entity.Notes = item.Notes;
                await employmentRepository.UpdateAsync(entity, cancellationToken, transaction);
                employmentId = entity.Id;
            }
            else
            {
                employmentId = SqlConvention.SequentialGuid();
                await employmentRepository.InsertAsync(new StaffEmployment
                {
                    Id = employmentId,
                    StaffMemberId = staffMemberId,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    ContinuousServiceStartDate = item.ContinuousServiceStartDate,
                    LocalAuthorityStartDate = item.LocalAuthorityStartDate,
                    LeavingReasonId = item.LeavingReasonId,
                    OriginId = item.OriginId,
                    DestinationId = item.DestinationId,
                    PreviousEmployer = item.PreviousEmployer,
                    NextEmployer = item.NextEmployer,
                    Notes = item.Notes
                }, cancellationToken, transaction);
            }

            await ReconcileContractsAsync(employmentId, item.Contracts, transaction, cancellationToken);
        }
    }

    private async Task ReconcileContractsAsync(Guid employmentId, List<StaffContractUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing =
            (await contractRepository.GetByEmploymentIdsAsync(new[] { employmentId }, cancellationToken, transaction))
            .ToList();
        var existingById = existing.ToDictionary(c => c.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        var droppedContractIds = existing.Where(row => !keptIds.Contains(row.Id)).Select(row => row.Id).ToList();

        if (droppedContractIds.Count > 0)
        {
            await SoftDeleteAllowancesForContractsAsync(droppedContractIds, transaction, cancellationToken);
            await SoftDeleteSuspensionsForContractsAsync(droppedContractIds, transaction, cancellationToken);

            foreach (var contractId in droppedContractIds)
            {
                await contractRepository.DeleteAsync(contractId, cancellationToken, true, transaction);
            }
        }

        foreach (var item in incoming)
        {
            Guid contractId;

            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                // Capture the pay position before the update so a movement can be logged.
                var previousPointId = entity.PayScalePointId;
                var previousSalary = entity.AnnualSalary;

                ApplyContract(entity, item);
                await contractRepository.UpdateAsync(entity, cancellationToken, transaction);
                contractId = entity.Id;

                await RecordSalaryChangeAsync(entity, previousPointId, previousSalary, transaction,
                    cancellationToken);
            }
            else
            {
                var contract = new StaffContract
                {
                    Id = SqlConvention.SequentialGuid(),
                    StaffEmploymentId = employmentId
                };
                ApplyContract(contract, item);
                await contractRepository.InsertAsync(contract, cancellationToken, transaction);
                contractId = contract.Id;
            }

            await ReconcileAllowancesAsync(contractId, item.Allowances, transaction, cancellationToken);
            await ReconcileSuspensionsAsync(contractId, item.Suspensions, transaction, cancellationToken);
        }
    }

    // Append-only: only written when the point or the salary actually moved. The repository stamps
    // CreatedById / CreatedAt, which is the changed-by / changed-on.
    private async Task RecordSalaryChangeAsync(StaffContract contract, Guid? previousPointId,
        decimal? previousSalary, IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        if (previousPointId == contract.PayScalePointId && previousSalary == contract.AnnualSalary)
        {
            return;
        }

        await salaryChangeRepository.InsertAsync(new StaffContractSalaryChange
        {
            Id = SqlConvention.SequentialGuid(),
            StaffContractId = contract.Id,
            OldPayScalePointId = previousPointId,
            NewPayScalePointId = contract.PayScalePointId,
            OldAnnualSalary = previousSalary,
            NewAnnualSalary = contract.AnnualSalary
        }, cancellationToken, transaction);
    }

    private async Task ReconcileSuspensionsAsync(Guid contractId, List<StaffContractSuspensionUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing =
            (await suspensionRepository.GetByContractIdsAsync(new[] { contractId }, cancellationToken, transaction))
            .ToList();
        var existingById = existing.ToDictionary(s => s.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await suspensionRepository.DeleteAsync(row.Id, cancellationToken, true, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplySuspension(entity, item);
                await suspensionRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var suspension = new StaffContractSuspension
                {
                    Id = SqlConvention.SequentialGuid(),
                    StaffContractId = contractId
                };
                ApplySuspension(suspension, item);
                await suspensionRepository.InsertAsync(suspension, cancellationToken, transaction);
            }
        }
    }

    private async Task SoftDeleteSuspensionsForContractsAsync(IEnumerable<Guid> contractIds,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var orphaned = await suspensionRepository.GetByContractIdsAsync(contractIds, cancellationToken, transaction);

        foreach (var suspension in orphaned)
        {
            await suspensionRepository.DeleteAsync(suspension.Id, cancellationToken, true, transaction);
        }
    }

    private static void ApplySuspension(StaffContractSuspension entity, StaffContractSuspensionUpsertItem item)
    {
        entity.StartDate = item.StartDate;
        entity.EndDate = item.EndDate;
        entity.Reason = item.Reason;
    }

    private async Task ReconcileAllowancesAsync(Guid contractId, List<StaffContractAllowanceUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing =
            (await allowanceRepository.GetByContractIdsAsync(new[] { contractId }, cancellationToken, transaction))
            .ToList();
        var existingById = existing.ToDictionary(a => a.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await allowanceRepository.DeleteAsync(row.Id, cancellationToken, true, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplyAllowance(entity, item);
                await allowanceRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var allowance = new StaffContractAllowance
                {
                    Id = SqlConvention.SequentialGuid(),
                    StaffContractId = contractId
                };
                ApplyAllowance(allowance, item);
                await allowanceRepository.InsertAsync(allowance, cancellationToken, transaction);
            }
        }
    }

    private async Task SoftDeleteAllowancesForContractsAsync(IEnumerable<Guid> contractIds,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var orphaned = await allowanceRepository.GetByContractIdsAsync(contractIds, cancellationToken, transaction);

        foreach (var allowance in orphaned)
        {
            await allowanceRepository.DeleteAsync(allowance.Id, cancellationToken, true, transaction);
        }
    }

    private static void ApplyAllowance(StaffContractAllowance entity, StaffContractAllowanceUpsertItem item)
    {
        entity.AdditionalPaymentTypeId = item.AdditionalPaymentTypeId;
        entity.Amount = item.Amount;
        entity.PayFactor = item.PayFactor;
        entity.StartDate = item.StartDate;
        entity.EndDate = item.EndDate;
        entity.IsSuperannuable = item.IsSuperannuable;
        entity.IsSubjectToNi = item.IsSubjectToNi;
        entity.IsBenefitInKind = item.IsBenefitInKind;
        entity.Reason = item.Reason;
    }

    private static void ApplyContract(StaffContract entity, StaffContractUpsertItem item)
    {
        entity.ContractTypeId = item.ContractTypeId;
        entity.StaffRoleId = item.StaffRoleId;
        entity.ServiceTermId = item.ServiceTermId;
        entity.DepartmentId = item.DepartmentId;
        entity.PayScaleId = item.PayScaleId;
        entity.PayScalePointId = item.PayScalePointId;
        entity.PostId = item.PostId;
        entity.SuperannuationSchemeId = item.SuperannuationSchemeId;
        entity.NiContractedOut = item.NiContractedOut;
        entity.PostTitle = item.PostTitle;
        entity.StartDate = item.StartDate;
        entity.EndDate = item.EndDate;
        entity.Fte = item.Fte;
        entity.HoursPerWeek = item.HoursPerWeek;
        entity.WeeksPerYear = item.WeeksPerYear;
        entity.AnnualSalary = item.AnnualSalary;
        entity.IsAgencySupply = item.IsAgencySupply;
        entity.SafeguardedSalary = item.SafeguardedSalary;
        entity.DailyRate = item.DailyRate;
    }

    private static StaffContractResponse MapContract(StaffContract c,
        IReadOnlyDictionary<Guid, List<StaffContractAllowance>> allowancesByContract,
        IReadOnlyDictionary<Guid, List<StaffContractSuspension>> suspensionsByContract,
        IReadOnlyDictionary<Guid, List<StaffContractSalaryChangeRow>> salaryChangesByContract) => new()
    {
        Allowances = (allowancesByContract.TryGetValue(c.Id, out var rows) ? rows : [])
            .OrderByDescending(a => a.StartDate)
            .Select(MapAllowance)
            .ToList(),
        Suspensions = (suspensionsByContract.TryGetValue(c.Id, out var suspensions) ? suspensions : [])
            .OrderByDescending(s => s.StartDate)
            .Select(MapSuspension)
            .ToList(),
        SalaryChanges = (salaryChangesByContract.TryGetValue(c.Id, out var changes) ? changes : [])
            .Select(MapSalaryChange)
            .ToList(),
        PostId = c.PostId,
        SuperannuationSchemeId = c.SuperannuationSchemeId,
        NiContractedOut = c.NiContractedOut,
        Id = c.Id,
        ContractTypeId = c.ContractTypeId,
        StaffRoleId = c.StaffRoleId,
        ServiceTermId = c.ServiceTermId,
        DepartmentId = c.DepartmentId,
        PayScaleId = c.PayScaleId,
        PayScalePointId = c.PayScalePointId,
        PostTitle = c.PostTitle,
        StartDate = c.StartDate,
        EndDate = c.EndDate,
        Fte = c.Fte,
        HoursPerWeek = c.HoursPerWeek,
        WeeksPerYear = c.WeeksPerYear,
        AnnualSalary = c.AnnualSalary,
        IsAgencySupply = c.IsAgencySupply,
        SafeguardedSalary = c.SafeguardedSalary,
        DailyRate = c.DailyRate
    };

    private static StaffContractAllowanceResponse MapAllowance(StaffContractAllowance a) => new()
    {
        Id = a.Id,
        AdditionalPaymentTypeId = a.AdditionalPaymentTypeId,
        Amount = a.Amount,
        PayFactor = a.PayFactor,
        StartDate = a.StartDate,
        EndDate = a.EndDate,
        IsSuperannuable = a.IsSuperannuable,
        IsSubjectToNi = a.IsSubjectToNi,
        IsBenefitInKind = a.IsBenefitInKind,
        Reason = a.Reason
    };

    private static StaffContractSuspensionResponse MapSuspension(StaffContractSuspension s) => new()
    {
        Id = s.Id,
        StartDate = s.StartDate,
        EndDate = s.EndDate,
        Reason = s.Reason
    };

    private static StaffContractSalaryChangeResponse MapSalaryChange(StaffContractSalaryChangeRow s) => new()
    {
        Id = s.Id,
        OldPayScalePointId = s.OldPayScalePointId,
        NewPayScalePointId = s.NewPayScalePointId,
        OldAnnualSalary = s.OldAnnualSalary,
        NewAnnualSalary = s.NewAnnualSalary,
        ChangedAt = s.ChangedAt,
        ChangedBy = s.ChangedBy
    };
}
