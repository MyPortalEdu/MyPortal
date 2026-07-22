namespace MyPortal.Contracts.Models.People.Students;

/// <summary>
/// Write payload for the Cultural area. Touches the pupil's ethnic/cultural demographics on the
/// shared Person record (ethnicity, first language, religion, nationality) and the English-proficiency
/// fields on the Student record. Other person fields (marital status, gender identity, etc.) are not
/// exposed here and are left untouched.
/// </summary>
public class StudentCulturalDetailsUpsertRequest
{
    public Guid? EthnicityId { get; set; }
    public Guid? FirstLanguageId { get; set; }
    public Guid? ReligionId { get; set; }
    public Guid? NationalityId { get; set; }

    public Guid? EnglishProficiencyId { get; set; }
    public DateTime? EnglishProficiencyDate { get; set; }
}
