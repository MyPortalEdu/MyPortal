namespace MyPortal.Contracts.Models.People;

public class SwfCensusReadinessIssue
{
    public Guid StaffMemberId { get; set; }
    public string StaffName { get; set; } = null!;
    public string Field { get; set; } = null!;
    public string Detail { get; set; } = null!;
}

public class SwfCensusMemberSummary
{
    public Guid StaffMemberId { get; set; }
    public string Name { get; set; } = null!;
    public string? Trn { get; set; }
    public string? PostCode { get; set; }
    public string? RoleCode { get; set; }
    public bool HasContract { get; set; }
    public int AbsenceCount { get; set; }
    public int IssueCount { get; set; }
}

public class SwfCensusPreviewResponse
{
    public DateTime ReferenceDate { get; set; }
    public int Year { get; set; }
    public string? LaNumber { get; set; }
    public string? Estab { get; set; }
    public string? Urn { get; set; }

    public int MemberCount { get; set; }
    public int IssueCount { get; set; }

    public List<SwfCensusReadinessIssue> Issues { get; set; } = [];
    public List<SwfCensusMemberSummary> Members { get; set; } = [];
}
