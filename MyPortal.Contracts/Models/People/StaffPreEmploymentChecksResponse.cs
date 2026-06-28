namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Read payload for the Pre-Employment Checks (Single Central Record) area: the summary
/// "checked on" flags plus the DBS, right-to-work, reference and overseas-check lists, and
/// the option lists for every picker so the editor is self-contained in one fetch.
/// Safeguarding/HR data — All-scope view and edit only (no self / line-manager scope).
/// </summary>
public class StaffPreEmploymentChecksResponse
{
    // Summary SCR flags (held on the 1:1 StaffPreEmploymentChecks record).
    public DateTime? IdentityCheckedDate { get; set; }
    public DateTime? ProhibitionFromTeachingCheckedDate { get; set; }
    public DateTime? ProhibitionFromManagementCheckedDate { get; set; }
    public DateTime? ChildcareDisqualificationCheckedDate { get; set; }
    public DateTime? MedicalFitnessCheckedDate { get; set; }
    public DateTime? QualificationsVerifiedDate { get; set; }
    public string? Notes { get; set; }

    // Record lists.
    public List<DbsCheckResponse> DbsChecks { get; set; } = [];
    public List<RightToWorkCheckResponse> RightToWorkChecks { get; set; } = [];
    public List<StaffReferenceResponse> References { get; set; } = [];
    public List<StaffOverseasCheckResponse> OverseasChecks { get; set; } = [];

    // Option lists (active only).
    public List<LookupResponse> DbsCheckTypes { get; set; } = [];
    public List<LookupResponse> RightToWorkDocumentTypes { get; set; } = [];
    public List<LookupResponse> ReferenceTypes { get; set; } = [];
    public List<LookupResponse> ReferenceStatuses { get; set; } = [];
    public List<LookupResponse> Countries { get; set; } = [];
}
