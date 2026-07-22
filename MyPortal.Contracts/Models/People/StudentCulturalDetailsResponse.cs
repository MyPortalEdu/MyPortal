namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Read payload for the Cultural area of the student profile — the pupil's ethnic/cultural
/// demographics (ethnicity, first language, religion, nationality) plus English proficiency (EAL
/// stage) and its assessment date, with the active option lists so the editor is self-contained.
/// GDPR special-category data. (Home language / national identity / country of birth are a later
/// addition — they need new Person columns.)
/// </summary>
public class StudentCulturalDetailsResponse
{
    public Guid? EthnicityId { get; set; }
    public Guid? FirstLanguageId { get; set; }
    public Guid? ReligionId { get; set; }
    public Guid? NationalityId { get; set; }

    public Guid? EnglishProficiencyId { get; set; }
    public DateTime? EnglishProficiencyDate { get; set; }

    public List<LookupResponse> Ethnicities { get; set; } = [];
    public List<LookupResponse> Languages { get; set; } = [];
    public List<LookupResponse> Religions { get; set; } = [];
    public List<LookupResponse> Nationalities { get; set; } = [];
    public List<LookupResponse> EnglishProficiencies { get; set; } = [];
}
