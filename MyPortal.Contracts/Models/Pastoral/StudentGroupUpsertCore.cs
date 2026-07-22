namespace MyPortal.Contracts.Models.Pastoral;

public class StudentGroupUpsertCore
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool Active { get; set; }
    public string? Notes { get; set; }
}
