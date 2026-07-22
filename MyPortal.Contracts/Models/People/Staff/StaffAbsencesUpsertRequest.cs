namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>
/// Whole-area replace for the Absences &amp; Leave area. For an All-scope (HR) editor this
/// reconciles every row by id. For a line-manager (Managed) editor the reconcile is scoped to
/// non-confidential rows only — confidential absences they can't see are never touched.
/// </summary>
public class StaffAbsencesUpsertRequest
{
    public List<StaffAbsenceUpsertItem> Absences { get; set; } = [];
}
