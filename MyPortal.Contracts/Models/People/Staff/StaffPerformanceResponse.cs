namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>
/// Read payload for the Performance (appraisal) area: the review-cycle list (history), the
/// objective / observation / training-record lists, and the option lists each editor needs.
/// Line-manager + HR only (no self-view). Review overall ratings and observation outcomes share
/// the <see cref="Outcomes"/> scale; <see cref="Staff"/> backs both the reviewer and observer
/// pickers.
/// </summary>
public class StaffPerformanceResponse
{
    public List<PerformanceReviewResponse> Reviews { get; set; } = [];
    public List<StaffObjectiveResponse> Objectives { get; set; } = [];
    public List<StaffObservationResponse> Observations { get; set; } = [];
    public List<StaffTrainingRecordResponse> TrainingRecords { get; set; } = [];

    public List<LookupResponse> ReviewStatuses { get; set; } = [];
    public List<LookupResponse> ObjectiveStatuses { get; set; } = [];
    public List<LookupResponse> ObjectiveCategories { get; set; } = [];
    public List<LookupResponse> Outcomes { get; set; } = [];
    public List<LookupResponse> Staff { get; set; } = [];
    public List<LookupResponse> TrainingCourses { get; set; } = [];
    public List<LookupResponse> TrainingStatuses { get; set; } = [];
}
