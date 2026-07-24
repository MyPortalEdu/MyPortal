namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Read payload for the Equality &amp; Diversity area: the person's special-category equality
/// fields plus the staff disability declaration, and the active option lists for every picker
/// so the editor is self-contained in one fetch. Special-category data — gated to HR (All) and
/// the person themselves (view-only); there is no Managed scope.
/// </summary>
public class StaffEqualityDetailsResponse
{
    // Person-level single-selects.
    public Guid? EthnicityId { get; set; }
    public Guid? NationalityId { get; set; }
    public Guid? FirstLanguageId { get; set; }
    public Guid? MaritalStatusId { get; set; }
    public Guid? ReligionId { get; set; }
    public Guid? SexualOrientationId { get; set; }
    public Guid? GenderIdentityId { get; set; }

    public bool HasDisability { get; set; }
    public string? DisabilityDetails { get; set; }
    public Guid? ImpairmentEffectId { get; set; }
    public string? DisabilityNumber { get; set; }
    public List<StaffDisabilityResponse> DeclaredDisabilities { get; set; } = [];

    // Option lists (active only).
    public List<LookupResponse> Ethnicities { get; set; } = [];
    public List<LookupResponse> Nationalities { get; set; } = [];
    public List<LookupResponse> Languages { get; set; } = [];
    public List<LookupResponse> MaritalStatuses { get; set; } = [];
    public List<LookupResponse> Religions { get; set; } = [];
    public List<LookupResponse> SexualOrientations { get; set; } = [];
    public List<LookupResponse> GenderIdentities { get; set; } = [];
    public List<LookupResponse> Disabilities { get; set; } = [];
    public List<LookupResponse> ImpairmentEffects { get; set; } = [];
}
