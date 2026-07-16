namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A pay-scale point option. Unlike a plain lookup it carries <see cref="PayScaleId"/> so the
/// editor can cascade — showing only the points that belong to the selected pay scale.
/// </summary>
public class PayScalePointResponse
{
    public Guid Id { get; set; }
    public Guid PayScaleId { get; set; }
    public string Description { get; set; } = null!;

    // Current full-time statutory salary for this point in the school's pay zone (null if no
    // rate is on file). The editor pre-fills a contract's salary as this × FTE.
    public decimal? FullTimeSalary { get; set; }
}
