namespace MyPortal.Contracts.Models.People;

public class PersonUpsertRequest
{
    public string? Title { get; set; }

    public string? PreferredFirstName { get; set; }

    public string? PreferredLastName { get; set; }

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string LastName { get; set; } = null!;

    public Guid? PhotoId { get; set; }

    public string? NhsNumber { get; set; }

    public string Gender { get; set; } = null!;

    public DateTime? Dob { get; set; }

    public DateTime? Deceased { get; set; }

    public Guid? EthnicityId { get; set; }

    public Guid? NationalityId { get; set; }

    public Guid? FirstLanguageId { get; set; }

    public Guid? MaritalStatusId { get; set; }
}
