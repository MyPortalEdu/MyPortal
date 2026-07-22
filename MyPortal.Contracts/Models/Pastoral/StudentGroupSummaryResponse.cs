using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Models.Pastoral;

public class StudentGroupSummaryResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool Active { get; set; }
    public StudentGroupKind Kind { get; set; }
}
