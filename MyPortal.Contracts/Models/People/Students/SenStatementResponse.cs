namespace MyPortal.Contracts.Models.People.Students;

/// <summary>A statutory SEN plan record — an EHC plan (IsEhcp) or a legacy statement — with its
/// statutory-assessment lifecycle dates and associated legal/dispute detail. England-only.</summary>
public class SenStatementResponse
{
    public Guid Id { get; set; }
    public bool IsEhcp { get; set; }
    public DateTime AssessmentRequestDate { get; set; }
    public DateTime? ParentConsultDate { get; set; }
    public DateTime? FinalisedDate { get; set; }
    public DateTime? CeasedDate { get; set; }
    public Guid? StatutoryAssessmentAgreedId { get; set; }
    public Guid? StatutoryAssessmentOutcomeId { get; set; }
    public bool SubjectToTribunal { get; set; }
    public bool UndergoingMediation { get; set; }
    public string? AppealNotes { get; set; }
    public string? TemporaryDisapplicationSubjects { get; set; }
    public string? PermanentDisapplicationSubjects { get; set; }
    public Guid? LocalAuthorityId { get; set; }
    public string? Comments { get; set; }
}
