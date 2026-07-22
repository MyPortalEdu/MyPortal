namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A pension scheme option, carrying the employer contribution rate currently in effect so the
/// editor can show the indicative employer cost alongside the salary.
/// </summary>
public class SuperannuationSchemeResponse
{
    public Guid Id { get; set; }
    public string Description { get; set; } = null!;

    /// <summary>Employer contribution as a percentage of pensionable pay; null if no rate is set.</summary>
    public decimal? EmployerRate { get; set; }
}
