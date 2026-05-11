using MyPortal.Data.Timetabler;

namespace MyPortal.Data.Interfaces;

public interface ITimetableSourceRepository
{
    /// Hydrates every entity slice the timetabler needs to build solver input for the given
    /// timetable. The week pattern is resolved from the timetable's academic year — schools
    /// run one cycle pattern per AY, so the caller doesn't need to specify it.
    Task<TimetableInputSources> LoadAsync(Guid timetableId, CancellationToken cancellationToken);
}
