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
using MyPortal.Data.Models;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.People;
using MyPortal.Services.Interfaces.Providers;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

/// <summary>
/// The annual increment: moves eligible staff on a spinal-progression service term up one point,
/// capped at their grade's top point, and reprices them at the next point's statutory rate. Can be
/// applied now or scheduled for a future date and applied when due (eligibility re-computed then).
/// </summary>
public class StaffIncrementService(
    IAuthorizationService authorizationService,
    ILogger<StaffIncrementService> logger,
    IServiceTermRepository serviceTermRepository,
    IStaffContractRepository contractRepository,
    IStaffContractSalaryChangeRepository salaryChangeRepository,
    IScheduledIncrementRepository scheduledIncrementRepository,
    IPayScalePointRepository payScalePointRepository,
    IPayScalePointRateRepository rateRepository,
    ISchoolRepository schoolRepository,
    IDateTimeProvider dateTimeProvider,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : BaseService(authorizationService, logger), IStaffIncrementService
{
    private const string IncrementSource = "Increment";

    private const string Scheduled = "Scheduled";
    private const string Completed = "Completed";
    private const string Cancelled = "Cancelled";

    public async Task<IncrementPreviewResponse> PreviewAsync(Guid serviceTermId,
        IncrementPreviewRequest model, CancellationToken cancellationToken)
    {
        await RequireEditAsync(cancellationToken);
        await validationService.ValidateAsync(model);

        var effectiveFrom = model.EffectiveFrom.Date;
        var (term, items) = await BuildAsync(serviceTermId, effectiveFrom, null, cancellationToken);

        return new IncrementPreviewResponse
        {
            ServiceTermId = term.Id,
            EffectiveFrom = effectiveFrom,
            EligibleCount = items.Count(IsEligible),
            Items = items
        };
    }

    public async Task ApplyAsync(Guid serviceTermId, IncrementApplyRequest model,
        CancellationToken cancellationToken)
    {
        await RequireEditAsync(cancellationToken);
        await validationService.ValidateAsync(model);

        var effectiveFrom = model.EffectiveFrom.Date;
        var chosen = model.ContractIds.ToHashSet();

        if (chosen.Count == 0)
        {
            return;
        }

        // Applying now changes the contract immediately (contracts aren't effective-dated), so a
        // future date would put the new salary live before its own date — schedule that instead.
        if (effectiveFrom > dateTimeProvider.UtcNow.Date)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(IncrementApplyRequest.EffectiveFrom),
                    "Applying now can't use a future date — schedule the increment instead.")]);
        }

        await unitOfWorkFactory.RunInTransactionAsync(null,
            async uow => await ApplyCoreAsync(serviceTermId, effectiveFrom, chosen.Contains, uow.Transaction,
                cancellationToken),
            cancellationToken);
    }

    public async Task<Guid> ScheduleAsync(Guid serviceTermId, IncrementScheduleRequest model,
        CancellationToken cancellationToken)
    {
        await RequireEditAsync(cancellationToken);
        await validationService.ValidateAsync(model);

        var effectiveFrom = model.EffectiveFrom.Date;

        if (effectiveFrom < dateTimeProvider.UtcNow.Date)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(IncrementScheduleRequest.EffectiveFrom),
                    "A scheduled increment cannot be in the past.")]);
        }

        var term = await GetProgressionTermAsync(serviceTermId, cancellationToken, null);

        // One pending run per term + date is enough; a second is a mistake.
        var existing = await scheduledIncrementRepository.GetScheduledAsync(serviceTermId, true, null,
            cancellationToken);
        if (existing.Any(s => s.EffectiveDate.Date == effectiveFrom))
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(IncrementScheduleRequest.EffectiveFrom),
                    "An increment is already scheduled for that date on this service term.")]);
        }

        var id = SqlConvention.SequentialGuid();
        await scheduledIncrementRepository.InsertAsync(new ScheduledIncrement
        {
            Id = id,
            ServiceTermId = term.Id,
            EffectiveDate = effectiveFrom,
            Status = Scheduled
        }, cancellationToken);

        return id;
    }

    public async Task<IReadOnlyList<ScheduledIncrementResponse>> GetScheduledAsync(Guid serviceTermId,
        CancellationToken cancellationToken)
    {
        await RequireEditAsync(cancellationToken);

        var rows = await scheduledIncrementRepository.GetScheduledAsync(serviceTermId, false, null,
            cancellationToken);
        return rows.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<ScheduledIncrementResponse>> GetDueAsync(CancellationToken cancellationToken)
    {
        await RequireEditAsync(cancellationToken);

        var rows = await scheduledIncrementRepository.GetScheduledAsync(null, true,
            dateTimeProvider.UtcNow.Date, cancellationToken);
        return rows.Select(Map).ToList();
    }

    public async Task CancelScheduledAsync(Guid scheduledId, CancellationToken cancellationToken)
    {
        await RequireEditAsync(cancellationToken);

        var scheduled = await scheduledIncrementRepository.GetByIdAsync(scheduledId, cancellationToken)
                        ?? throw new NotFoundException("Scheduled increment not found.");

        if (scheduled.Status != Scheduled)
        {
            throw new EntityInUseException("Only a pending increment can be cancelled.");
        }

        scheduled.Status = Cancelled;
        await scheduledIncrementRepository.UpdateAsync(scheduled, cancellationToken);
    }

    public async Task ApplyScheduledAsync(Guid scheduledId, CancellationToken cancellationToken)
    {
        await RequireEditAsync(cancellationToken);

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var scheduled = await scheduledIncrementRepository.GetByIdAsync(scheduledId, cancellationToken,
                                uow.Transaction)
                            ?? throw new NotFoundException("Scheduled increment not found.");

            if (scheduled.Status != Scheduled)
            {
                throw new EntityInUseException("This increment has already been run or cancelled.");
            }

            // A scheduled run applies to everyone eligible at run time — no hand-picked subset.
            var applied = await ApplyCoreAsync(scheduled.ServiceTermId, scheduled.EffectiveDate.Date,
                _ => true, uow.Transaction, cancellationToken);

            scheduled.Status = Completed;
            scheduled.CompletedAt = dateTimeProvider.UtcNow;
            scheduled.AppliedCount = applied;
            await scheduledIncrementRepository.UpdateAsync(scheduled, cancellationToken, uow.Transaction);
        }, cancellationToken);
    }

    /// <summary>
    /// Applies the increment to every eligible contract matching <paramref name="chosen"/>, writing
    /// each move to salary history stamped so the cycle can't be re-applied. Returns the count moved.
    /// </summary>
    private async Task<int> ApplyCoreAsync(Guid serviceTermId, DateTime effectiveFrom, Func<Guid, bool> chosen,
        IDbTransaction transaction, CancellationToken cancellationToken)
    {
        var (_, items) = await BuildAsync(serviceTermId, effectiveFrom, transaction, cancellationToken);

        var applied = 0;

        foreach (var item in items.Where(i => IsEligible(i) && chosen(i.ContractId)))
        {
            var contract = await contractRepository.GetByIdAsync(item.ContractId, cancellationToken, transaction);

            if (contract is null)
            {
                continue;
            }

            var previousPointId = contract.PayScalePointId;
            var previousSalary = contract.AnnualSalary;

            contract.PayScalePointId = item.NextPointId;
            contract.AnnualSalary = item.NewSalary;
            await contractRepository.UpdateAsync(contract, cancellationToken, transaction);

            await salaryChangeRepository.InsertAsync(new StaffContractSalaryChange
            {
                Id = SqlConvention.SequentialGuid(),
                StaffContractId = contract.Id,
                OldPayScalePointId = previousPointId,
                NewPayScalePointId = contract.PayScalePointId,
                OldAnnualSalary = previousSalary,
                NewAnnualSalary = contract.AnnualSalary,
                EffectiveDate = effectiveFrom,
                Source = IncrementSource
            }, cancellationToken, transaction);

            applied++;
        }

        return applied;
    }

    /// <summary>
    /// Shared by preview and apply so what is shown is exactly what is written. Returns the term and
    /// one row per live contract, each resolved to its next point, new salary and eligibility.
    /// </summary>
    private async Task<(ServiceTerm Term, List<IncrementItem> Items)> BuildAsync(Guid serviceTermId,
        DateTime effectiveFrom, IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var term = await GetProgressionTermAsync(serviceTermId, cancellationToken, transaction);

        var candidates = await contractRepository.GetIncrementCandidatesAsync(serviceTermId, effectiveFrom,
            cancellationToken, transaction);

        var points = (await payScalePointRepository.GetListAsync(cancellationToken: cancellationToken,
                transaction: transaction))
            .ToList();

        var spinePoints = points.Where(p => p.ServiceTermId == serviceTermId).ToList();
        var pointsByScale = points
            .Where(p => p.PayScaleId.HasValue)
            .GroupBy(p => p.PayScaleId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var ratesByPoint = new Dictionary<Guid, decimal>();
        var payZoneId = await schoolRepository.GetLocalSchoolPayZoneIdAsync(cancellationToken);

        if (payZoneId.HasValue)
        {
            var rates = await rateRepository.GetCurrentByZoneAsync(payZoneId.Value, effectiveFrom,
                cancellationToken, transaction);

            foreach (var rate in rates.OrderBy(r => r.EffectiveFrom))
            {
                ratesByPoint[rate.PayScalePointId] = rate.AnnualSalary;
            }
        }

        // Contracts already incremented for this exact date must not move again.
        var alreadyIncremented = (await salaryChangeRepository.GetIncrementedContractIdsAsync(
                candidates.Select(c => c.ContractId), effectiveFrom, cancellationToken, transaction))
            .ToHashSet();

        var items = candidates
            .Select(c => BuildItem(c, term, spinePoints, pointsByScale, ratesByPoint,
                alreadyIncremented.Contains(c.ContractId)))
            .ToList();

        return (term, items);
    }

    private static IncrementItem BuildItem(IncrementCandidateRow c, ServiceTerm term,
        List<PayScalePoint> spinePoints, Dictionary<Guid, List<PayScalePoint>> pointsByScale,
        Dictionary<Guid, decimal> ratesByPoint, bool alreadyIncremented)
    {
        var item = new IncrementItem
        {
            ContractId = c.ContractId,
            StaffMemberId = c.StaffMemberId,
            StaffName = c.StaffName,
            StaffCode = c.StaffCode,
            ScaleCode = c.ScaleCode,
            CurrentPointCode = c.CurrentPointCode,
            CurrentPointValue = c.CurrentPointValue,
            CurrentSalary = c.AnnualSalary,
            AlreadyIncremented = alreadyIncremented
        };

        var candidatePoints = term.SinglePaySpine
            ? spinePoints
            : (pointsByScale.TryGetValue(c.PayScaleId, out var scalePoints) ? scalePoints : []);

        var ceiling = c.ScaleMaximumPoint
                      ?? (candidatePoints.Count > 0 ? candidatePoints.Max(p => p.PointValue) : c.CurrentPointValue);

        var next = candidatePoints
            .Where(p => p.PointValue > c.CurrentPointValue && p.PointValue <= ceiling)
            .OrderBy(p => p.PointValue)
            .FirstOrDefault();

        if (next is null)
        {
            item.AtMaximum = true;
            return item;
        }

        item.NextPointId = next.Id;
        item.NextPointCode = next.Code;
        item.NextPointValue = next.PointValue;

        if (ratesByPoint.TryGetValue(next.Id, out var rate))
        {
            item.NewSalary = Math.Round(rate * c.Fte, 0, MidpointRounding.AwayFromZero);
        }
        else
        {
            item.MissingRate = true;
        }

        return item;
    }

    private async Task<ServiceTerm> GetProgressionTermAsync(Guid serviceTermId,
        CancellationToken cancellationToken, IDbTransaction? transaction)
    {
        var term = await serviceTermRepository.GetByIdAsync(serviceTermId, cancellationToken, transaction)
                   ?? throw new NotFoundException("Service term not found.");

        if (!term.SpinalProgression)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(serviceTermId),
                    "The annual increment only applies to service terms with spinal progression.")]);
        }

        return term;
    }

    private ScheduledIncrementResponse Map(ScheduledIncrementRow r) => new()
    {
        Id = r.Id,
        ServiceTermId = r.ServiceTermId,
        ServiceTermCode = r.ServiceTermCode,
        ServiceTermDescription = r.ServiceTermDescription,
        EffectiveDate = r.EffectiveDate,
        Status = r.Status,
        CompletedAt = r.CompletedAt,
        AppliedCount = r.AppliedCount,
        ScheduledBy = r.ScheduledBy,
        IsDue = r.Status == Scheduled && r.EffectiveDate.Date <= dateTimeProvider.UtcNow.Date
    };

    private static bool IsEligible(IncrementItem item) =>
        !item.AtMaximum && !item.MissingRate && !item.AlreadyIncremented && item.NextPointId.HasValue;

    private Task RequireEditAsync(CancellationToken cancellationToken) =>
        AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffEmploymentDetails,
            cancellationToken);
}
