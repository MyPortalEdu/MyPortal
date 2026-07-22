namespace MyPortal.Contracts.Models.People.Students;

/// <summary>Write payload for a single statutory SEN statement / EHC plan. Id is null for a new row;
/// set to reconcile an existing one.</summary>
public class SenStatementUpsertRequest
{
    public Guid? Id { get; set; }
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
