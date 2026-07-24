namespace MyPortal.Contracts.Models.People;

public class StaffContractAllowanceResponse
{
    public Guid Id { get; set; }
    public Guid AdditionalPaymentTypeId { get; set; }
    public decimal Amount { get; set; }
    public decimal? PayFactor { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsSuperannuable { get; set; }
    public bool IsSubjectToNi { get; set; }
    public bool IsBenefitInKind { get; set; }
    public string? Reason { get; set; }
}
