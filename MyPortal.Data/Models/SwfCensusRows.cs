namespace MyPortal.Data.Models;

public class SwfCensusHeaderRow
{
    public string? LaNumber { get; set; }
    public string? Estab { get; set; }
    public string? Urn { get; set; }
}

public class SwfCensusMemberRow
{
    public Guid StaffMemberId { get; set; }
    public string? TeacherNumber { get; set; }
    public string FamilyName { get; set; } = null!;
    public string GivenName { get; set; } = null!;
    public string? FormerFamilyName { get; set; }
    public string? NiNumber { get; set; }
    public string? Sex { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? EthnicityCode { get; set; }
    public string? DisabilityCode { get; set; }
    public bool Qts { get; set; }
    public bool Qtls { get; set; }
    public bool Eyts { get; set; }
    public bool Hlta { get; set; }
    public string? QtsRouteCode { get; set; }
    public bool Slt { get; set; }
    public DateTime? InductionCompletedDate { get; set; }

    public string? ContractTypeCode { get; set; }
    public DateTime? ContractStart { get; set; }
    public DateTime? ContractEnd { get; set; }
    public string? PostCode { get; set; }
    public string? RoleCode { get; set; }
    public bool DailyRate { get; set; }
    public decimal? BasePay { get; set; }
    public string? PayRangeCode { get; set; }
    public bool SafeguardedSalary { get; set; }
    public decimal? FullTimeHoursPerWeek { get; set; }
    public decimal? WeeksPerYear { get; set; }
    public decimal? Fte { get; set; }
    public DateTime? ArrivalDate { get; set; }
    public string? OriginCode { get; set; }
    public string? DestinationCode { get; set; }
    public string? LeavingReasonCode { get; set; }
    public Guid? ContractId { get; set; }
}

public class SwfCensusAbsenceRow
{
    public Guid StaffMemberId { get; set; }
    public DateTime? FirstDay { get; set; }
    public DateTime? LastDay { get; set; }
    public decimal? WorkingDaysLost { get; set; }
    public string? CategoryCode { get; set; }
}

public class SwfCensusAllowanceRow
{
    public Guid ContractId { get; set; }
    public string? PaymentTypeCode { get; set; }
    public decimal Amount { get; set; }
    public DateTime? PayStartDate { get; set; }
    public DateTime? PayEndDate { get; set; }
}
