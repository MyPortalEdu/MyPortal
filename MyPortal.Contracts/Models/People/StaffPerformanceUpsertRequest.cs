namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Whole-area replace for the Performance area: the review-cycle list plus the objective /
/// observation / training-record lists (each reconciled by id).
/// </summary>
public class StaffPerformanceUpsertRequest
{
    public List<PerformanceReviewUpsertItem> Reviews { get; set; } = [];
    public List<StaffObjectiveUpsertItem> Objectives { get; set; } = [];
    public List<StaffObservationUpsertItem> Observations { get; set; } = [];
    public List<StaffTrainingRecordUpsertItem> TrainingRecords { get; set; } = [];
}
