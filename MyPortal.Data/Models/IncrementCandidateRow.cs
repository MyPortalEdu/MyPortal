namespace MyPortal.Data.Models;

/// <summary>
/// A live contract on a service term, with its current pay position, for the annual increment
/// routine. The next point and new salary are worked out in the service.
/// </summary>
public class IncrementCandidateRow
{
    public Guid ContractId { get; set; }
    public Guid StaffMemberId { get; set; }
    public string StaffName { get; set; } = null!;
    public string StaffCode { get; set; } = null!;
    public Guid PayScaleId { get; set; }
    public Guid PayScalePointId { get; set; }
    public decimal Fte { get; set; }
    public decimal? AnnualSalary { get; set; }
    public decimal CurrentPointValue { get; set; }
    public string CurrentPointCode { get; set; } = null!;
    public string ScaleCode { get; set; } = null!;
    public string ScaleDescription { get; set; } = null!;

    /// <summary>The scale/grade window ceiling. Staff at it can't progress without a promotion.</summary>
    public decimal? ScaleMaximumPoint { get; set; }
}
