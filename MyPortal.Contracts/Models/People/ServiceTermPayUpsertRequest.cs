namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A pay scale as submitted with the term's spine. Points are not listed — they are generated from
/// the range, and the salaries are keyed by point value so a row typed into the grid before the
/// point exists still lands on the right place.
/// </summary>
public class PayScaleUpsertItem
{
    /// <summary>Null for a scale being added.</summary>
    public Guid? Id { get; set; }

    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool Active { get; set; } = true;

    public decimal? MinimumPoint { get; set; }
    public decimal? MaximumPoint { get; set; }

    /// <summary>Ignored on a single-spine term, where the term's interval governs.</summary>
    public decimal? PointInterval { get; set; }

    public List<PointSalaryItem> Salaries { get; set; } = [];
}

/// <summary>
/// The whole pay setup for a service term, saved in one go: the spine range, its scales, and the
/// salaries for one generation. Scales absent from <see cref="Scales"/> are removed.
/// </summary>
public class ServiceTermPayUpsertRequest
{
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// Owned here rather than on the service term editor: flipping it moves points between the term
    /// and its scales, which only this request can reconcile in the same transaction.
    /// </summary>
    public bool SinglePaySpine { get; set; }

    public decimal? MinimumPoint { get; set; }
    public decimal? MaximumPoint { get; set; }
    public decimal? PointInterval { get; set; }

    public List<PayScaleUpsertItem> Scales { get; set; } = [];

    /// <summary>Salaries against the term-owned spine, when the term runs one.</summary>
    public List<PointSalaryItem> SpineSalaries { get; set; } = [];
}
