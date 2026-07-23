namespace MyPortal.Contracts.Models.People;

public class PayScalePointItem
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal PointValue { get; set; }

    /// <summary>Contracts paid on this point. A point in use cannot be generated away.</summary>
    public int ContractCount { get; set; }
}

public class PayScaleItem
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool Active { get; set; }
    public decimal? MinimumPoint { get; set; }
    public decimal? MaximumPoint { get; set; }

    /// <summary>Null on a single-spine term, where the scale steps along the term's interval.</summary>
    public decimal? PointInterval { get; set; }

    public int ContractCount { get; set; }

    /// <summary>
    /// Empty on a single-spine term — the scale is a window onto <see cref="ServiceTermPayResponse.SpinePoints"/>
    /// rather than the owner of any points.
    /// </summary>
    public List<PayScalePointItem> Points { get; set; } = [];
}

/// <summary>
/// One salary in the grid. Keyed by point value rather than point id so the client can show a row
/// for a point that has been typed into the range but not yet saved.
/// </summary>
public class PointSalaryItem
{
    public decimal PointValue { get; set; }
    public Guid PayZoneId { get; set; }
    public decimal AnnualSalary { get; set; }
}

/// <summary>
/// A set of rates sharing an effective-from date — one published version of this term's spine.
/// </summary>
public class PayScaleGenerationItem
{
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public int RateCount { get; set; }
    public bool IsCurrent { get; set; }
}

/// <summary>
/// Everything the Pay Spine panel of a service term needs: the scales, the points they resolve to,
/// and the salaries for one generation.
/// </summary>
public class ServiceTermPayResponse
{
    public Guid ServiceTermId { get; set; }
    public bool SinglePaySpine { get; set; }

    /// <summary>The term's own range, used only when <see cref="SinglePaySpine"/> is set.</summary>
    public decimal? MinimumPoint { get; set; }
    public decimal? MaximumPoint { get; set; }
    public decimal? PointInterval { get; set; }

    /// <summary>The term-owned spine. Populated only when <see cref="SinglePaySpine"/> is set.</summary>
    public List<PayScalePointItem> SpinePoints { get; set; } = [];

    public List<PayScaleItem> Scales { get; set; } = [];
    public List<LookupResponse> PayZones { get; set; } = [];

    /// <summary>The school's own pay zone, so the grid can open on the column that matters.</summary>
    public Guid? LocalPayZoneId { get; set; }

    public List<PayScaleGenerationItem> Generations { get; set; } = [];
    public DateTime? SelectedEffectiveFrom { get; set; }

    /// <summary>Salaries for the selected generation, across every point on this term.</summary>
    public List<PointSalaryItem> SpineSalaries { get; set; } = [];

    /// <summary>Salaries for the selected generation, per scale that owns its own points.</summary>
    public List<PayScaleSalariesItem> ScaleSalaries { get; set; } = [];

    public bool CanEdit { get; set; }
}

public class PayScaleSalariesItem
{
    public Guid PayScaleId { get; set; }
    public List<PointSalaryItem> Salaries { get; set; } = [];
}
