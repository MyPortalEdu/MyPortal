namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>
/// Whole-area replace for the Pre-Employment Checks area: the summary SCR flags plus the four
/// record lists, each reconciled by id (null id inserts, populated id updates, omitted rows
/// soft-delete). HR-edit only.
/// </summary>
public class StaffPreEmploymentChecksUpsertRequest
{
    public DateTime? IdentityCheckedDate { get; set; }
    public DateTime? ProhibitionFromTeachingCheckedDate { get; set; }
    public DateTime? ProhibitionFromManagementCheckedDate { get; set; }
    public DateTime? ChildcareDisqualificationCheckedDate { get; set; }
    public DateTime? MedicalFitnessCheckedDate { get; set; }
    public DateTime? QualificationsVerifiedDate { get; set; }
    public string? Notes { get; set; }

    public List<DbsCheckUpsertItem> DbsChecks { get; set; } = [];
    public List<RightToWorkCheckUpsertItem> RightToWorkChecks { get; set; } = [];
    public List<StaffReferenceUpsertItem> References { get; set; } = [];
    public List<StaffOverseasCheckUpsertItem> OverseasChecks { get; set; } = [];
}
