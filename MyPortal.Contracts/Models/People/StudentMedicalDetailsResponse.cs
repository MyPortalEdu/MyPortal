namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Read payload for the Medical area of the student profile — the medical-needs flag (on the shared
/// Person) plus the pupil's medical conditions, dietary requirements and disabilities, with the active
/// option lists so the editor is self-contained. GDPR special-category data. Gated on Student.Medical.
/// </summary>
public class StudentMedicalDetailsResponse
{
    public bool HasMedicalNeeds { get; set; }

    public List<PersonConditionItem> Conditions { get; set; } = [];
    public List<Guid> DietaryRequirementIds { get; set; } = [];
    public List<Guid> DisabilityIds { get; set; } = [];

    public List<LookupResponse> MedicalConditions { get; set; } = [];
    public List<LookupResponse> DietaryRequirements { get; set; } = [];
    public List<LookupResponse> Disabilities { get; set; } = [];
}
