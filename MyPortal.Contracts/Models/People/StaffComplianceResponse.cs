namespace MyPortal.Contracts.Models.People;

/// <summary>
/// One thing needing HR attention for a staff member: an expiring/expired record, or a required
/// record not held at all. Category and Kind are strings shared with the SQL, kept loose so the
/// query can add sources without a contract change.
/// </summary>
public class ComplianceItemResponse
{
    public Guid StaffMemberId { get; set; }
    public string StaffName { get; set; } = null!;
    public string StaffCode { get; set; } = null!;

    /// <summary>Dbs | RightToWork | Training | Contract | PreEmployment.</summary>
    public string Category { get; set; } = null!;

    public string Detail { get; set; } = null!;

    /// <summary>Null for a Missing item.</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Expired | ExpiringSoon | Missing.</summary>
    public string Kind { get; set; } = null!;
}

public class StaffComplianceDashboardResponse
{
    /// <summary>How many days ahead "expiring soon" looks.</summary>
    public int HorizonDays { get; set; }

    public int ExpiredCount { get; set; }
    public int ExpiringSoonCount { get; set; }
    public int MissingCount { get; set; }

    public List<ComplianceItemResponse> Items { get; set; } = [];
}
