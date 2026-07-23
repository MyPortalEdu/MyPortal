namespace MyPortal.Contracts.Models.People;

public class ServiceTermUpsertRequest
{
    public string Code { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool Active { get; set; } = true;

    public bool IsTeacher { get; set; }
    public bool Salaried { get; set; } = true;
    public bool SpinalProgression { get; set; }
    public bool TermTimeOnlyPossible { get; set; }

    public byte? IncrementMonth { get; set; }
    public byte? IncrementDay { get; set; }

    // SinglePaySpine and the point range live on the Pay Spine panel — see ServiceTermPayUpsertRequest.

    public decimal? HoursPerWeek { get; set; }
    public decimal? WeeksPerYear { get; set; }

    public List<ServiceTermSchemeItem> SuperannuationSchemes { get; set; } = [];
}
