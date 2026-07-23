namespace MyPortal.Data.Models;

/// <summary>
/// How many contracts reference a pay scale or one of its points. Drives the guards that stop a
/// scale being deleted, or its point range shrunk past a point somebody is paid on.
/// </summary>
public class PayScaleUsageRow
{
    public Guid Id { get; set; }
    public int ContractCount { get; set; }
}
