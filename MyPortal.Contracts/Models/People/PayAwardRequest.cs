namespace MyPortal.Contracts.Models.People;

public class PayAwardScaleOverride
{
    public Guid PayScaleId { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Uplift an existing generation of this service term's rates into a new one. Scales listed in
/// <see cref="ScaleOverrides"/> use their own percentage; everything else uses
/// <see cref="DefaultPercentage"/>.
///
/// Awards are per service term because that is how they are negotiated — teachers under STPCD and
/// support staff under the NJC, at different percentages and often different dates.
/// </summary>
public class PayAwardRequest
{
    public DateTime EffectiveFrom { get; set; }

    /// <summary>The generation being uplifted from.</summary>
    public DateTime SourceEffectiveFrom { get; set; }

    public decimal DefaultPercentage { get; set; }

    /// <summary>Ignored on a single-spine term, where one uplift applies across the whole spine.</summary>
    public List<PayAwardScaleOverride> ScaleOverrides { get; set; } = [];
}

/// <summary>
/// One row of a modelled award. Carries the salary it came from so the review shows before and
/// after without the caller having to hold the source generation itself.
/// </summary>
public class PayAwardPreviewItem
{
    public Guid PayScalePointId { get; set; }
    public Guid? PayScaleId { get; set; }
    public decimal PointValue { get; set; }
    public Guid PayZoneId { get; set; }
    public decimal PreviousAnnualSalary { get; set; }
    public decimal AnnualSalary { get; set; }
}

public class PayAwardPreviewResponse
{
    public DateTime EffectiveFrom { get; set; }
    public DateTime SourceEffectiveFrom { get; set; }
    public List<PayAwardPreviewItem> Rates { get; set; } = [];
}
