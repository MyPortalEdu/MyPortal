namespace MyPortal.Services.Interfaces.Services;

public interface ITimetableMaterialisationService
{
    /// Walks every TimetableAssignment for the supplied timetable and emits the corresponding
    /// Session + SessionPeriod rows so the live timetable system sees the new schedule. Caller
    /// is expected to wrap this in a transaction alongside the Timetable.Status flip.
    Task MaterialiseAsync(Guid timetableId, DateTime startDate, DateTime? endDate,
        CancellationToken cancellationToken);
}
