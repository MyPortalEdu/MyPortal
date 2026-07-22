namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>
/// Read payload for the Employment Details area: the staff member's bank / NI details, their
/// employment spells (each with its contracts), and the active option lists for every picker so
/// the editor is self-contained in one fetch. The "crown jewels" of the profile — HR-edit only,
/// self / HR view (no line-manager scope).
/// </summary>
public class StaffEmploymentDetailsResponse
{
    // Bank / NI (held on the StaffMember).
    public string? BankName { get; set; }
    public string? BankAccount { get; set; }
    public string? BankSortCode { get; set; }
    public string? NiNumber { get; set; }

    // The local school's pay zone — the basis for the statutory salaries on the points below.
    public Guid? PayZoneId { get; set; }
    public string? PayZoneName { get; set; }

    public List<StaffEmploymentResponse> Employments { get; set; } = [];

    // Option lists (active only).
    public List<LookupResponse> LeavingReasons { get; set; } = [];
    public List<LookupResponse> Origins { get; set; } = [];
    public List<LookupResponse> Destinations { get; set; } = [];
    public List<LookupResponse> ContractTypes { get; set; } = [];
    public List<LookupResponse> StaffRoles { get; set; } = [];
    public List<LookupResponse> ServiceTerms { get; set; } = [];
    public List<LookupResponse> Departments { get; set; } = [];
    public List<LookupResponse> PayScales { get; set; } = [];
    public List<PayScalePointResponse> PayScalePoints { get; set; } = [];
}
