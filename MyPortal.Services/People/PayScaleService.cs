using System.Data;
using System.Globalization;
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

/// <summary>
/// The pay spine, scoped to a service term.
///
/// A term either runs a single spine that its scales are windows onto (NJC: SCP 1-43, with grades
/// overlapping across it), or it runs separate scales that each own their points (teachers: MPS 1-6
/// and UPS 1-3 are different money at the same point number). <c>SinglePaySpine</c> picks which,
/// and it decides whether points hang off the term or off the scale.
/// </summary>
public class PayScaleService(
    IAuthorizationService authorizationService,
    ILogger<PayScaleService> logger,
    IPayScaleRepository payScaleRepository,
    IPayScalePointRepository payScalePointRepository,
    IPayScalePointRateRepository rateRepository,
    IPayZoneRepository payZoneRepository,
    ISchoolRepository schoolRepository,
    IServiceTermRepository serviceTermRepository,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : BaseService(authorizationService, logger), IPayScaleService
{
    // A typo in the interval ("0.01" across 1..50) would otherwise generate thousands of points.
    private const int MaxPointsPerScale = 250;

    private const decimal DefaultPointInterval = 1m;

    public async Task<ServiceTermPayResponse> GetServiceTermPayAsync(Guid serviceTermId,
        DateTime? effectiveFrom, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewStaffSetup, cancellationToken);

        var term = await serviceTermRepository.GetByIdAsync(serviceTermId, cancellationToken)
                   ?? throw new NotFoundException("Service term not found.");

        var scales = (await payScaleRepository.GetListAsync(cancellationToken: cancellationToken))
            .Where(s => s.ServiceTermId == serviceTermId)
            .OrderBy(s => s.Code)
            .ToList();

        var scaleIds = scales.Select(s => s.Id).ToHashSet();

        var points = (await payScalePointRepository.GetListAsync(cancellationToken: cancellationToken))
            .Where(p => p.ServiceTermId == serviceTermId
                        || (p.PayScaleId is { } id && scaleIds.Contains(id)))
            .ToList();

        var pointIds = points.Select(p => p.Id).ToHashSet();

        var rates = (await rateRepository.GetListAsync(cancellationToken: cancellationToken))
            .Where(r => pointIds.Contains(r.PayScalePointId))
            .ToList();

        var zones = (await payZoneRepository.GetListAsync(cancellationToken: cancellationToken)).ToList();

        var usage = (await payScalePointRepository.GetContractCountsAsync(cancellationToken))
            .ToDictionary(u => u.Id, u => u.ContractCount);
        var scaleUsage = (await payScaleRepository.GetContractCountsAsync(cancellationToken))
            .ToDictionary(u => u.Id, u => u.ContractCount);

        var generations = BuildGenerations(rates);
        var selected = effectiveFrom?.Date
                       ?? generations.FirstOrDefault(g => g.IsCurrent)?.EffectiveFrom
                       ?? generations.LastOrDefault()?.EffectiveFrom;

        var selectedRates = selected == null
            ? []
            : rates.Where(r => r.EffectiveFrom.Date == selected.Value.Date).ToList();

        var pointById = points.ToDictionary(p => p.Id);

        return new ServiceTermPayResponse
        {
            ServiceTermId = serviceTermId,
            SinglePaySpine = term.SinglePaySpine,
            MinimumPoint = term.MinimumPoint,
            MaximumPoint = term.MaximumPoint,
            PointInterval = term.PointInterval,
            SpinePoints = points
                .Where(p => p.ServiceTermId == serviceTermId)
                .OrderBy(p => p.PointValue)
                .Select(p => MapPoint(p, usage))
                .ToList(),
            Scales = scales
                .Select(s => new PayScaleItem
                {
                    Id = s.Id,
                    Code = s.Code,
                    Description = s.Description,
                    Active = s.Active,
                    MinimumPoint = s.MinimumPoint,
                    MaximumPoint = s.MaximumPoint,
                    PointInterval = s.PointInterval,
                    ContractCount = scaleUsage.GetValueOrDefault(s.Id),
                    Points = points
                        .Where(p => p.PayScaleId == s.Id)
                        .OrderBy(p => p.PointValue)
                        .Select(p => MapPoint(p, usage))
                        .ToList()
                })
                .ToList(),
            PayZones = zones.ToAlphabeticalLookup(),
            LocalPayZoneId = await schoolRepository.GetLocalSchoolPayZoneIdAsync(cancellationToken),
            Generations = generations,
            SelectedEffectiveFrom = selected,
            SpineSalaries = selectedRates
                .Where(r => pointById[r.PayScalePointId].ServiceTermId == serviceTermId)
                .Select(r => MapSalary(r, pointById))
                .ToList(),
            ScaleSalaries = scales
                .Select(s => new PayScaleSalariesItem
                {
                    PayScaleId = s.Id,
                    Salaries = selectedRates
                        .Where(r => pointById[r.PayScalePointId].PayScaleId == s.Id)
                        .Select(r => MapSalary(r, pointById))
                        .ToList()
                })
                .ToList(),
            CanEdit = await AuthorizationService.HasPermissionAsync(Permissions.Staff.EditStaffSetup,
                cancellationToken)
        };
    }

    /// <summary>
    /// Saves the term's whole pay setup at once: the spine range, its scales, the points those
    /// imply, and the salaries for one generation. Doing it in one transaction is what lets the
    /// editor show salary rows for points that do not exist yet.
    /// </summary>
    public async Task UpdateServiceTermPayAsync(Guid serviceTermId, ServiceTermPayUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditStaffSetup, cancellationToken);
        await validationService.ValidateAsync(model);

        var term = await serviceTermRepository.GetByIdAsync(serviceTermId, cancellationToken)
                   ?? throw new NotFoundException("Service term not found.");

        EnsureCodesUnique(model.Scales);

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var tx = uow.Transaction;

            // Applied before the points are reconciled below, so a flipped spine moves its points
            // to the new owner in the same transaction rather than leaving the term contradicting
            // where its points actually hang.
            term.SinglePaySpine = model.SinglePaySpine;
            term.MinimumPoint = model.MinimumPoint;
            term.MaximumPoint = model.MaximumPoint;
            term.PointInterval = model.PointInterval;
            await serviceTermRepository.UpdateAsync(term, cancellationToken, tx);

            var existingScales = (await payScaleRepository.GetListAsync(cancellationToken: cancellationToken,
                    transaction: tx))
                .Where(s => s.ServiceTermId == serviceTermId)
                .ToList();

            var keptScaleIds = model.Scales.Where(s => s.Id.HasValue).Select(s => s.Id!.Value).ToHashSet();

            foreach (var removed in existingScales.Where(s => !keptScaleIds.Contains(s.Id)))
            {
                await DeleteScaleAsync(removed, tx, cancellationToken);
            }

            // The term-owned spine exists only while SinglePaySpine is on; turning it off drops it,
            // and turning it on drops the scale-owned points the scales no longer need.
            var spineValues = term.SinglePaySpine
                ? GeneratePointValues(model.MinimumPoint, model.MaximumPoint,
                    model.PointInterval ?? DefaultPointInterval)
                : [];

            var spinePoints = await ReconcilePointsAsync(spineValues,
                p => p.ServiceTermId == serviceTermId,
                value => new PayScalePoint
                {
                    Id = SqlConvention.SequentialGuid(),
                    ServiceTermId = serviceTermId,
                    PointValue = value,
                    Code = Truncate($"{term.Code}{FormatPoint(value)}", 20),
                    Description = $"{term.Description} {FormatPoint(value)}",
                    Active = true
                },
                tx, cancellationToken);

            foreach (var item in model.Scales)
            {
                var scale = existingScales.FirstOrDefault(s => s.Id == item.Id);

                if (scale is null)
                {
                    scale = new PayScale { Id = SqlConvention.SequentialGuid(), ServiceTermId = serviceTermId };
                    ApplyScale(scale, item, term);
                    await payScaleRepository.InsertAsync(scale, cancellationToken, tx);
                }
                else
                {
                    ApplyScale(scale, item, term);
                    await payScaleRepository.UpdateAsync(scale, cancellationToken, tx);
                }

                EnsureWindowWithinSpine(scale, term);

                var scaleId = scale.Id;

                var scaleValues = term.SinglePaySpine
                    ? []
                    : GeneratePointValues(scale.MinimumPoint, scale.MaximumPoint,
                        scale.PointInterval ?? DefaultPointInterval);

                var scalePoints = await ReconcilePointsAsync(scaleValues,
                    p => p.PayScaleId == scaleId,
                    value => new PayScalePoint
                    {
                        Id = SqlConvention.SequentialGuid(),
                        PayScaleId = scaleId,
                        PointValue = value,
                        Code = Truncate($"{scale.Code}{FormatPoint(value)}", 20),
                        Description = $"{scale.Description} {FormatPoint(value)}",
                        Active = true
                    },
                    tx, cancellationToken);

                await WriteSalariesAsync(item.Salaries, scalePoints, model.EffectiveFrom.Date, tx,
                    cancellationToken);
            }

            await WriteSalariesAsync(model.SpineSalaries, spinePoints, model.EffectiveFrom.Date, tx,
                cancellationToken);
        }, cancellationToken);
    }

    /// <summary>
    /// Brings one owner's points in line with the values its range implies. Matching is on point
    /// value, so existing points — and every contract that references one — survive untouched.
    /// </summary>
    private async Task<List<PayScalePoint>> ReconcilePointsAsync(List<decimal> desired,
        Func<PayScalePoint, bool> owned, Func<decimal, PayScalePoint> build, IDbTransaction? transaction,
        CancellationToken cancellationToken)
    {
        var existing = (await payScalePointRepository.GetListAsync(cancellationToken: cancellationToken,
                transaction: transaction))
            .Where(owned)
            .ToList();

        var desiredValues = desired.ToHashSet();
        var dropped = existing.Where(p => !desiredValues.Contains(p.PointValue)).ToList();

        if (dropped.Count > 0)
        {
            await EnsureNotInUseAsync(dropped, transaction, cancellationToken);
            await DeletePointsAsync(dropped, transaction, cancellationToken);
        }

        var kept = existing.Where(p => desiredValues.Contains(p.PointValue)).ToList();
        var byValue = kept.ToDictionary(p => p.PointValue);

        foreach (var value in desired.Where(value => !byValue.ContainsKey(value)))
        {
            var point = build(value);
            await payScalePointRepository.InsertAsync(point, cancellationToken, transaction);
            kept.Add(point);
        }

        return kept.OrderBy(p => p.PointValue).ToList();
    }

    /// <summary>
    /// Writes the salaries for one generation. Keyed by point value rather than point id, since a
    /// row typed into the grid may belong to a point created moments ago in this same transaction.
    /// </summary>
    private async Task WriteSalariesAsync(List<PointSalaryItem> salaries, List<PayScalePoint> points,
        DateTime effectiveFrom, IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        if (points.Count == 0)
        {
            return;
        }

        var pointByValue = points.ToDictionary(p => p.PointValue, p => p.Id);
        var pointIds = points.Select(p => p.Id).ToHashSet();

        var existing = (await rateRepository.GetListAsync(cancellationToken: cancellationToken,
                transaction: transaction))
            .Where(r => r.EffectiveFrom.Date == effectiveFrom && pointIds.Contains(r.PayScalePointId))
            .ToList();

        // Every row in a generation shares its end date, so a new cell inherits it rather than
        // being left open and overlapping the generation that follows.
        var effectiveTo = existing.FirstOrDefault()?.EffectiveTo;
        var byKey = existing.ToDictionary(r => (r.PayScalePointId, r.PayZoneId));
        var submitted = new HashSet<(Guid, Guid)>();

        foreach (var item in salaries)
        {
            if (!pointByValue.TryGetValue(item.PointValue, out var pointId))
            {
                continue;
            }

            submitted.Add((pointId, item.PayZoneId));

            if (byKey.TryGetValue((pointId, item.PayZoneId), out var entity))
            {
                if (entity.AnnualSalary == item.AnnualSalary)
                {
                    continue;
                }

                entity.AnnualSalary = item.AnnualSalary;
                await rateRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                await rateRepository.InsertAsync(new PayScalePointRate
                {
                    Id = SqlConvention.SequentialGuid(),
                    PayScalePointId = pointId,
                    PayZoneId = item.PayZoneId,
                    EffectiveFrom = effectiveFrom,
                    EffectiveTo = effectiveTo,
                    AnnualSalary = item.AnnualSalary
                }, cancellationToken, transaction);
            }
        }

        // A cleared cell comes back absent rather than as zero, so drop what is no longer there.
        foreach (var orphan in existing.Where(r => !submitted.Contains((r.PayScalePointId, r.PayZoneId))))
        {
            await rateRepository.DeleteAsync(orphan.Id, cancellationToken, true, transaction);
        }
    }

    public async Task DeletePayScaleAsync(Guid payScaleId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditStaffSetup, cancellationToken);

        var scale = await payScaleRepository.GetByIdAsync(payScaleId, cancellationToken)
                    ?? throw new NotFoundException("Pay scale not found.");

        await unitOfWorkFactory.RunInTransactionAsync(null,
            async uow => await DeleteScaleAsync(scale, uow.Transaction, cancellationToken), cancellationToken);
    }

    private async Task DeleteScaleAsync(PayScale scale, IDbTransaction? transaction,
        CancellationToken cancellationToken)
    {
        var contractCount = (await payScaleRepository.GetContractCountsAsync(cancellationToken, transaction))
            .FirstOrDefault(u => u.Id == scale.Id)?.ContractCount ?? 0;

        if (contractCount > 0)
        {
            throw new EntityInUseException(
                $"'{scale.Description}' is used by {contractCount} contract(s) and cannot be deleted.");
        }

        var points = (await payScalePointRepository.GetListAsync(cancellationToken: cancellationToken,
                transaction: transaction))
            .Where(p => p.PayScaleId == scale.Id)
            .ToList();

        await EnsureNotInUseAsync(points, transaction, cancellationToken);
        await DeletePointsAsync(points, transaction, cancellationToken);
        await payScaleRepository.DeleteAsync(scale.Id, cancellationToken, true, transaction);
    }

    /// <summary>
    /// Narrowing a range past a point somebody is paid on would orphan their contract, so block it
    /// and let the user move them first.
    /// </summary>
    private async Task EnsureNotInUseAsync(List<PayScalePoint> points, IDbTransaction? transaction,
        CancellationToken cancellationToken)
    {
        if (points.Count == 0)
        {
            return;
        }

        var usage = (await payScalePointRepository.GetContractCountsAsync(cancellationToken, transaction))
            .ToDictionary(u => u.Id, u => u.ContractCount);

        var inUse = points
            .Where(p => usage.GetValueOrDefault(p.Id) > 0)
            .OrderBy(p => p.PointValue)
            .ToList();

        if (inUse.Count == 0)
        {
            return;
        }

        var names = string.Join(", ", inUse.Select(p => p.Code));

        throw new EntityInUseException(
            $"The point range cannot be reduced: {names} {(inUse.Count == 1 ? "is" : "are")} " +
            "used by existing contracts.");
    }

    private async Task DeletePointsAsync(List<PayScalePoint> points, IDbTransaction? transaction,
        CancellationToken cancellationToken)
    {
        if (points.Count == 0)
        {
            return;
        }

        var pointIds = points.Select(p => p.Id).ToHashSet();

        // Rates hang off the point, so they go first or the delete violates the foreign key.
        var rates = (await rateRepository.GetListAsync(cancellationToken: cancellationToken,
                transaction: transaction))
            .Where(r => pointIds.Contains(r.PayScalePointId))
            .ToList();

        foreach (var rate in rates)
        {
            await rateRepository.DeleteAsync(rate.Id, cancellationToken, true, transaction);
        }

        foreach (var point in points)
        {
            await payScalePointRepository.DeleteAsync(point.Id, cancellationToken, true, transaction);
        }
    }

    internal static List<decimal> GeneratePointValues(decimal? minimum, decimal? maximum, decimal interval)
    {
        if (minimum is null || maximum is null)
        {
            return [];
        }

        if (interval <= 0)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(PayScaleUpsertItem.PointInterval),
                    "The point interval must be greater than zero.")]);
        }

        var values = new List<decimal>();

        for (var value = minimum.Value; value <= maximum.Value; value += interval)
        {
            values.Add(decimal.Round(value, 2));

            if (values.Count > MaxPointsPerScale)
            {
                throw new ValidationException(
                    [new ValidationFailure(nameof(PayScaleUpsertItem.MaximumPoint),
                        $"That range and interval would generate more than {MaxPointsPerScale} points.")]);
            }
        }

        return values;
    }

    /// <summary>
    /// On a single-spine term a scale is a window onto the term's points, so it cannot reach past
    /// where the spine runs.
    /// </summary>
    private static void EnsureWindowWithinSpine(PayScale scale, ServiceTerm term)
    {
        if (!term.SinglePaySpine)
        {
            return;
        }

        if (scale.MinimumPoint is { } minimum && term.MinimumPoint is { } spineMinimum && minimum < spineMinimum)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(PayScaleUpsertItem.MinimumPoint),
                    $"'{scale.Description}' starts below the spine ({FormatPoint(spineMinimum)}).")]);
        }

        if (scale.MaximumPoint is { } maximum && term.MaximumPoint is { } spineMaximum && maximum > spineMaximum)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(PayScaleUpsertItem.MaximumPoint),
                    $"'{scale.Description}' runs past the end of the spine ({FormatPoint(spineMaximum)}).")]);
        }
    }

    private static void EnsureCodesUnique(List<PayScaleUpsertItem> scales)
    {
        var duplicate = scales
            .GroupBy(s => s.Code.Trim(), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicate is not null)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(PayScaleUpsertItem.Code),
                    $"Pay scale code '{duplicate.Key}' is used more than once on this service term.")]);
        }
    }

    private static void ApplyScale(PayScale entity, PayScaleUpsertItem model, ServiceTerm term)
    {
        entity.Code = model.Code.Trim().ToUpperInvariant();
        entity.Description = model.Description.Trim();
        entity.Active = model.Active;
        entity.MinimumPoint = model.MinimumPoint;
        entity.MaximumPoint = model.MaximumPoint;
        // On a single spine the term's interval governs, so the scale carries none of its own.
        entity.PointInterval = term.SinglePaySpine ? null : model.PointInterval;
    }

    private static PayScalePointItem MapPoint(PayScalePoint point, Dictionary<Guid, int> usage) => new()
    {
        Id = point.Id,
        Code = point.Code,
        Description = point.Description,
        PointValue = point.PointValue,
        ContractCount = usage.GetValueOrDefault(point.Id)
    };

    private static PointSalaryItem MapSalary(PayScalePointRate rate, Dictionary<Guid, PayScalePoint> points) =>
        new()
        {
            PointValue = points[rate.PayScalePointId].PointValue,
            PayZoneId = rate.PayZoneId,
            AnnualSalary = rate.AnnualSalary
        };

    private static string FormatPoint(decimal value) =>
        value == decimal.Truncate(value)
            ? ((int)value).ToString(CultureInfo.InvariantCulture)
            : value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Truncate(string value, int length) =>
        value.Length <= length ? value : value[..length];

    public async Task<PayAwardPreviewResponse> PreviewPayAwardAsync(Guid serviceTermId, PayAwardRequest model,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditStaffSetup, cancellationToken);
        await validationService.ValidateAsync(model);

        var (rates, points) = await LoadTermSpineAsync(serviceTermId, null, cancellationToken);

        return new PayAwardPreviewResponse
        {
            EffectiveFrom = model.EffectiveFrom.Date,
            SourceEffectiveFrom = model.SourceEffectiveFrom.Date,
            Rates = BuildAward(model, rates, points)
        };
    }

    public async Task ApplyPayAwardAsync(Guid serviceTermId, PayAwardRequest model,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditStaffSetup, cancellationToken);
        await validationService.ValidateAsync(model);

        var effectiveFrom = model.EffectiveFrom.Date;

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var (rates, points) = await LoadTermSpineAsync(serviceTermId, uow.Transaction, cancellationToken);

            var uplifted = BuildAward(model, rates, points);

            // Close every generation still open before the new one starts, then insert. Reusing the
            // close-and-insert pattern keeps the spine free of overlapping effective ranges.
            foreach (var row in rates.Where(r => r.EffectiveTo == null && r.EffectiveFrom.Date < effectiveFrom))
            {
                row.EffectiveTo = effectiveFrom.AddDays(-1);
                await rateRepository.UpdateAsync(row, cancellationToken, uow.Transaction);
            }

            foreach (var item in uplifted)
            {
                await rateRepository.InsertAsync(new PayScalePointRate
                {
                    Id = SqlConvention.SequentialGuid(),
                    PayScalePointId = item.PayScalePointId,
                    PayZoneId = item.PayZoneId,
                    EffectiveFrom = effectiveFrom,
                    EffectiveTo = null,
                    AnnualSalary = item.AnnualSalary
                }, cancellationToken, uow.Transaction);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// The points and rates belonging to one service term — its own spine plus everything owned by
    /// its scales. An award only ever moves this term, since teacher and support pay are settled
    /// separately.
    /// </summary>
    private async Task<(List<PayScalePointRate> Rates, List<PayScalePoint> Points)> LoadTermSpineAsync(
        Guid serviceTermId, IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var scaleIds = (await payScaleRepository.GetListAsync(cancellationToken: cancellationToken,
                transaction: transaction))
            .Where(s => s.ServiceTermId == serviceTermId)
            .Select(s => s.Id)
            .ToHashSet();

        var points = (await payScalePointRepository.GetListAsync(cancellationToken: cancellationToken,
                transaction: transaction))
            .Where(p => p.ServiceTermId == serviceTermId
                        || (p.PayScaleId is { } id && scaleIds.Contains(id)))
            .ToList();

        var pointIds = points.Select(p => p.Id).ToHashSet();

        var rates = (await rateRepository.GetListAsync(cancellationToken: cancellationToken,
                transaction: transaction))
            .Where(r => pointIds.Contains(r.PayScalePointId))
            .ToList();

        return (rates, points);
    }

    /// <summary>
    /// Shared by preview and apply so what the user approves is exactly what gets written.
    /// </summary>
    private static List<PayAwardPreviewItem> BuildAward(PayAwardRequest model, List<PayScalePointRate> rates,
        List<PayScalePoint> points)
    {
        var effectiveFrom = model.EffectiveFrom.Date;
        var sourceEffectiveFrom = model.SourceEffectiveFrom.Date;

        var source = rates.Where(r => r.EffectiveFrom.Date == sourceEffectiveFrom).ToList();

        if (source.Count == 0)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(PayAwardRequest.SourceEffectiveFrom),
                    "There is no pay spine generation effective from that date.")]);
        }

        if (effectiveFrom <= sourceEffectiveFrom)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(PayAwardRequest.EffectiveFrom),
                    "The pay award must take effect after the generation it uplifts.")]);
        }

        // Only ever append: inserting a generation before an existing one would need the surrounding
        // ranges resplitting, which is not what a pay award means.
        var latest = rates.Max(r => r.EffectiveFrom.Date);

        if (effectiveFrom <= latest)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(PayAwardRequest.EffectiveFrom),
                    $"A pay award must take effect after the latest generation ({latest:dd/MM/yyyy}).")]);
        }

        var pointById = points.ToDictionary(p => p.Id);
        var overrideByScale = model.ScaleOverrides.ToDictionary(o => o.PayScaleId, o => o.Percentage);

        return source
            .Select(r =>
            {
                var point = pointById[r.PayScalePointId];

                var percentage = point.PayScaleId is { } scaleId
                                 && overrideByScale.TryGetValue(scaleId, out var scalePercentage)
                    ? scalePercentage
                    : model.DefaultPercentage;

                // STPCD publishes spine values in whole pounds, so round rather than carry pence
                // forward and compound the drift with each award.
                var salary = Math.Round(r.AnnualSalary * (1m + percentage / 100m), 0,
                    MidpointRounding.AwayFromZero);

                return new PayAwardPreviewItem
                {
                    PayScalePointId = r.PayScalePointId,
                    PayScaleId = point.PayScaleId,
                    PointValue = point.PointValue,
                    PayZoneId = r.PayZoneId,
                    PreviousAnnualSalary = r.AnnualSalary,
                    AnnualSalary = salary
                };
            })
            .ToList();
    }

    private static List<PayScaleGenerationItem> BuildGenerations(List<PayScalePointRate> rates) =>
        rates
            .GroupBy(r => r.EffectiveFrom.Date)
            .OrderBy(g => g.Key)
            .Select(g => new PayScaleGenerationItem
            {
                EffectiveFrom = g.Key,
                EffectiveTo = g.Min(r => r.EffectiveTo),
                RateCount = g.Count(),
                IsCurrent = g.Any(r => r.EffectiveTo == null)
            })
            .ToList();
}
