namespace MyPortal.Contracts.Models.Curriculum;

public class AcademicTermUpsertRequest
{
    public Guid? AcademicTermId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}