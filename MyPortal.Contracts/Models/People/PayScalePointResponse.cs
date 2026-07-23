namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A pay-scale point option. Unlike a plain lookup it carries <see cref="PayScaleId"/> so the
/// editor can cascade — showing only the points that belong to the selected pay scale.
/// </summary>
public class PayScalePointResponse
{
    public Guid Id { get; set; }

    /// <summary>
    /// The scale this point is offered under. On a single-spine term the same point appears once
    /// per scale whose window contains it, since overlapping grades share the spine.
    /// </summary>
    public Guid PayScaleId { get; set; }

    public decimal PointValue { get; set; }
    public string Description { get; set; } = null!;

    // Current full-time statutory salary for this point in the school's pay zone (null if no
    // rate is on file). The editor pre-fills a contract's salary as this × FTE.
    public decimal? FullTimeSalary { get; set; }
}
