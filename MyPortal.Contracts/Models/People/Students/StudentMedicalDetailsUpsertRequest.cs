namespace MyPortal.Contracts.Models.People.Students;

/// <summary>
/// Write payload for the Medical area. Touches the medical-needs flag on the shared Person record and
/// reconciles the pupil's medical conditions, dietary requirements and disabilities. Other person
/// fields are not exposed here and are left untouched.
/// </summary>
public class StudentMedicalDetailsUpsertRequest
{
    public bool HasMedicalNeeds { get; set; }

    public List<PersonConditionItem> Conditions { get; set; } = [];
    public List<Guid> DietaryRequirementIds { get; set; } = [];
    public List<Guid> DisabilityIds { get; set; } = [];
}
