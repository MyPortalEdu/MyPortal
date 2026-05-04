namespace MyPortal.Contracts.Models.Curriculum;

public class AcademicTermUpsertRequest
{
    public Guid? AcademicTermId { get; set; }
    public string Name { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}