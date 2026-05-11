using MyPortal.Core.Entities;

namespace MyPortal.Data.Timetabler;

/// Carries every entity slice the builder needs to materialise a TimetableInput. Constructed
/// by the loading service from repository queries; consumed by TimetableInputBuilder.Build.
public record TimetableInputSources(
    IReadOnlyList<AttendancePeriod> Periods,
    IReadOnlyList<StaffMember> Teachers,
    IReadOnlyList<SubjectStaffMember> StaffSubjects,
    IReadOnlyList<Room> Rooms,
    IReadOnlyList<SubjectRoom> RoomSubjects,
    IReadOnlyList<CurriculumBand> Bands,
    IReadOnlyList<CurriculumBandBlockAssignment> BandBlocks,
    IReadOnlyList<CurriculumBlock> Blocks,
    IReadOnlyList<CurriculumGroup> Groups,
    IReadOnlyList<CurriculumGroupSession> GroupSessions,
    IReadOnlyList<SessionType> SessionTypes,
    IReadOnlyList<Class> Classes,
    IReadOnlyList<Course> Courses,
    IReadOnlyList<TimetablePin> Pins);
