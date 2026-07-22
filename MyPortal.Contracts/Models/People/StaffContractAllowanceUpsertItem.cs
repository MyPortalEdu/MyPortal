namespace MyPortal.Contracts.Models.People;

/// <summary>
/// One allowance in a contract's replace payload. A null <see cref="Id"/> is a new row; a populated
/// one updates. Allowances present in the database but absent from the payload are soft-deleted.
/// </summary>
public class StaffContractAllowanceUpsertItem
{
    public Guid? Id { get; set; }
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
