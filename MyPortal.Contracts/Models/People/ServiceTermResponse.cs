namespace MyPortal.Contracts.Models.People;

public class ServiceTermResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool Active { get; set; }

    public bool IsTeacher { get; set; }
    public bool Salaried { get; set; }
    public bool SpinalProgression { get; set; }
    public bool SinglePaySpine { get; set; }
    public bool TermTimeOnlyPossible { get; set; }

    public byte? IncrementMonth { get; set; }
    public byte? IncrementDay { get; set; }

    public decimal? MinimumPoint { get; set; }
    public decimal? MaximumPoint { get; set; }
    public decimal? PointInterval { get; set; }

    public decimal? HoursPerWeek { get; set; }
    public decimal? WeeksPerYear { get; set; }

    public int ContractCount { get; set; }
    public int PostCount { get; set; }

    public List<ServiceTermSchemeItem> SuperannuationSchemes { get; set; } = [];
}
