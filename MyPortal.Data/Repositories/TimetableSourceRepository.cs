using System.Data;
using Dapper;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Timetabler;

namespace MyPortal.Data.Repositories;

public class TimetableSourceRepository : ITimetableSourceRepository
{
    private readonly IDbConnectionFactory _factory;

    public TimetableSourceRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<TimetableInputSources> LoadAsync(Guid timetableId, Guid weekPatternId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        if (conn is IDbConnection dbConn && dbConn.State != ConnectionState.Open) dbConn.Open();

        // Resolve the timetable's academic year — that scopes the curriculum tree we load.
        var academicYearId = await conn.QuerySingleOrDefaultAsync<Guid?>(new CommandDefinition(
            "SELECT AcademicYearId FROM dbo.Timetables WHERE Id = @timetableId;",
            new { timetableId }, cancellationToken: cancellationToken));

        if (academicYearId is null)
            throw new NotFoundException($"Timetable '{timetableId}' not found.");

        // 1) Period set for the supplied week pattern.
        var periods = (await conn.QueryAsync<AttendancePeriod>(new CommandDefinition(
            "SELECT * FROM dbo.AttendancePeriods WHERE WeekPatternId = @weekPatternId;",
            new { weekPatternId }, cancellationToken: cancellationToken))).ToArray();

        // 2) Teaching staff (all teachers, not just those who'll appear in the curriculum —
        //    the solver picks among any qualified teacher per class).
        var teachers = (await conn.QueryAsync<StaffMember>(new CommandDefinition(
            "SELECT * FROM dbo.StaffMembers WHERE IsTeachingStaff = 1 AND IsDeleted = 0;",
            cancellationToken: cancellationToken))).ToArray();
        var teacherIds = teachers.Select(t => t.Id).ToArray();

        var staffSubjects = teacherIds.Length == 0
            ? Array.Empty<SubjectStaffMember>()
            : (await conn.QueryAsync<SubjectStaffMember>(new CommandDefinition(
                "SELECT * FROM dbo.SubjectStaffMembers WHERE StaffMemberId IN @teacherIds;",
                new { teacherIds }, cancellationToken: cancellationToken))).ToArray();

        // 3) All rooms with their subject suitability.
        var rooms = (await conn.QueryAsync<Room>(new CommandDefinition(
            "SELECT * FROM dbo.Rooms;",
            cancellationToken: cancellationToken))).ToArray();

        var roomSubjects = (await conn.QueryAsync<SubjectRoom>(new CommandDefinition(
            "SELECT * FROM dbo.SubjectRooms;",
            cancellationToken: cancellationToken))).ToArray();

        // 4) Curriculum tree, anchored on bands for the academic year.
        var bands = (await conn.QueryAsync<CurriculumBand>(new CommandDefinition(
            "SELECT * FROM dbo.CurriculumBands WHERE AcademicYearId = @academicYearId;",
            new { academicYearId }, cancellationToken: cancellationToken))).ToArray();
        var bandIds = bands.Select(b => b.Id).ToArray();

        var bandBlocks = bandIds.Length == 0
            ? Array.Empty<CurriculumBandBlockAssignment>()
            : (await conn.QueryAsync<CurriculumBandBlockAssignment>(new CommandDefinition(
                "SELECT * FROM dbo.CurriculumBandBlockAssignments WHERE BandId IN @bandIds;",
                new { bandIds }, cancellationToken: cancellationToken))).ToArray();
        var blockIds = bandBlocks.Select(bb => bb.BlockId).Distinct().ToArray();

        var blocks = blockIds.Length == 0
            ? Array.Empty<CurriculumBlock>()
            : (await conn.QueryAsync<CurriculumBlock>(new CommandDefinition(
                "SELECT * FROM dbo.CurriculumBlocks WHERE Id IN @blockIds;",
                new { blockIds }, cancellationToken: cancellationToken))).ToArray();

        var groups = blockIds.Length == 0
            ? Array.Empty<CurriculumGroup>()
            : (await conn.QueryAsync<CurriculumGroup>(new CommandDefinition(
                "SELECT * FROM dbo.CurriculumGroups WHERE BlockId IN @blockIds;",
                new { blockIds }, cancellationToken: cancellationToken))).ToArray();
        var groupIds = groups.Select(g => g.Id).ToArray();

        var groupSessions = groupIds.Length == 0
            ? Array.Empty<CurriculumGroupSession>()
            : (await conn.QueryAsync<CurriculumGroupSession>(new CommandDefinition(
                "SELECT * FROM dbo.CurriculumGroupSessions WHERE CurriculumGroupId IN @groupIds;",
                new { groupIds }, cancellationToken: cancellationToken))).ToArray();

        // SessionTypes is a small lookup — fetch all.
        var sessionTypes = (await conn.QueryAsync<SessionType>(new CommandDefinition(
            "SELECT * FROM dbo.SessionTypes;",
            cancellationToken: cancellationToken))).ToArray();

        var classes = groupIds.Length == 0
            ? Array.Empty<Class>()
            : (await conn.QueryAsync<Class>(new CommandDefinition(
                "SELECT * FROM dbo.Classes WHERE CurriculumGroupId IN @groupIds;",
                new { groupIds }, cancellationToken: cancellationToken))).ToArray();
        var courseIds = classes.Select(c => c.CourseId).Distinct().ToArray();

        var courses = courseIds.Length == 0
            ? Array.Empty<Course>()
            : (await conn.QueryAsync<Course>(new CommandDefinition(
                "SELECT * FROM dbo.Courses WHERE Id IN @courseIds;",
                new { courseIds }, cancellationToken: cancellationToken))).ToArray();

        // 5) Pins specific to this timetable.
        var pins = (await conn.QueryAsync<TimetablePin>(new CommandDefinition(
            "SELECT * FROM dbo.TimetablePins WHERE TimetableId = @timetableId;",
            new { timetableId }, cancellationToken: cancellationToken))).ToArray();

        return new TimetableInputSources(
            periods, teachers, staffSubjects, rooms, roomSubjects,
            bands, bandBlocks, blocks, groups, groupSessions, sessionTypes,
            classes, courses, pins);
    }
}
