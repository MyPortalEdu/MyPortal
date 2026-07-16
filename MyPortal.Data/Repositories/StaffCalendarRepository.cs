using Dapper;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Data.Interfaces;

namespace MyPortal.Data.Repositories;

/// <summary>
/// Read-only Dapper reads backing the staff Timetable area. Not bound to a single entity — each
/// method projects one event source into the flat <see cref="StaffCalendarEventResponse"/> shape.
/// Lessons and non-contact periods reuse the register/attendance views that already expand the
/// weekly slot pattern into dated, term-clamped instances.
/// </summary>
public class StaffCalendarRepository : IStaffCalendarRepository
{
    private readonly IDbConnectionFactory _factory;

    public StaffCalendarRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    private async Task<IEnumerable<StaffCalendarEventResponse>> QueryAsync(string sql, object parameters,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
        return await conn.QueryAsync<StaffCalendarEventResponse>(command);
    }

    public Task<IEnumerable<StaffCalendarEventResponse>> GetLessonsAsync(Guid staffMemberId, DateTime from,
        DateTime to, CancellationToken cancellationToken)
    {
        // vw_session_period_metadata already resolves cover (the effective TeacherId/RoomName) and
        // expands weekly slots to dated instances, so we just filter to this teacher + window.
        const string sql = @"
SELECT
    CONCAT('lesson:',
           CONVERT(nvarchar(36), COALESCE(m.SessionId, CAST(0x0 AS uniqueidentifier))), ':',
           CONVERT(nvarchar(36), m.PeriodId), ':',
           CONVERT(nvarchar(36), m.AttendanceWeekId))   AS Id,
    COALESCE(m.ClassCode, m.PeriodName)                 AS Title,
    m.StartTime                                         AS [Start],
    m.EndTime                                           AS [End],
    CAST(0 AS bit)                                      AS AllDay,
    CASE WHEN m.IsCover = 1 THEN 'Cover' ELSE 'Lesson' END AS Category,
    m.RoomName                                          AS Location,
    CAST(NULL AS nvarchar(16))                          AS ColourCode
FROM dbo.vw_session_period_metadata AS m
WHERE m.TeacherId = @staffMemberId
  AND m.StartTime < @to
  AND m.EndTime > @from;";

        return QueryAsync(sql, new { staffMemberId, from, to }, cancellationToken);
    }

    public Task<IEnumerable<StaffCalendarEventResponse>> GetDetentionsAsync(Guid staffMemberId, DateTime from,
        DateTime to, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT
    CONCAT('detention:', CONVERT(nvarchar(36), d.Id)) AS Id,
    e.Subject                                         AS Title,
    e.StartTime                                       AS [Start],
    e.EndTime                                         AS [End],
    e.IsAllDay                                        AS AllDay,
    'Detention'                                       AS Category,
    COALESCE(e.Location, r.[Name])                    AS Location,
    t.ColourCode                                      AS ColourCode
FROM dbo.Detentions AS d
JOIN dbo.DiaryEvents AS e ON e.Id = d.EventId
LEFT JOIN dbo.DiaryEventTypes AS t ON t.Id = e.EventTypeId
LEFT JOIN dbo.Rooms AS r ON r.Id = e.RoomId
WHERE d.SupervisorId = @staffMemberId
  AND e.StartTime < @to
  AND e.EndTime > @from;";

        return QueryAsync(sql, new { staffMemberId, from, to }, cancellationToken);
    }

    public Task<IEnumerable<StaffCalendarEventResponse>> GetDiaryEventsAsync(Guid personId, DateTime from,
        DateTime to, CancellationToken cancellationToken)
    {
        // Public events plus private ones this person attends. Kinds with a dedicated source are
        // excluded so they aren't double-counted: 3 = Cover, 4 = Detention, 10 = ParentEvening.
        const string sql = @"
SELECT DISTINCT
    CONCAT('event:', CONVERT(nvarchar(36), e.Id)) AS Id,
    e.Subject                                     AS Title,
    e.StartTime                                   AS [Start],
    e.EndTime                                     AS [End],
    e.IsAllDay                                    AS AllDay,
    CASE WHEN t.Kind IN (7, 8, 9) THEN 'Holiday' ELSE 'Event' END AS Category,
    COALESCE(e.Location, r.[Name])                AS Location,
    t.ColourCode                                  AS ColourCode
FROM dbo.DiaryEvents AS e
JOIN dbo.DiaryEventTypes AS t ON t.Id = e.EventTypeId
LEFT JOIN dbo.Rooms AS r ON r.Id = e.RoomId
LEFT JOIN dbo.DiaryEventAttendees AS a ON a.EventId = e.Id AND a.PersonId = @personId
WHERE e.StartTime < @to
  AND e.EndTime > @from
  AND t.Kind NOT IN (3, 4, 10)
  AND (e.IsPublic = 1 OR a.Id IS NOT NULL);";

        return QueryAsync(sql, new { personId, from, to }, cancellationToken);
    }

    public Task<IEnumerable<StaffCalendarEventResponse>> GetNonContactAsync(Guid staffMemberId, DateTime from,
        DateTime to, CancellationToken cancellationToken)
    {
        // Allocations are recurring period blocks; vw_attendance_period_instances supplies the dated
        // occurrences. Clamp each occurrence to the allocation's effective date window.
        const string sql = @"
SELECT
    CONCAT('ncc:', CONVERT(nvarchar(36), s.Id), ':', CONVERT(nvarchar(36), i.AttendanceWeekId)) AS Id,
    s.Code                          AS Title,
    i.ActualStartTime               AS [Start],
    i.ActualEndTime                 AS [End],
    CAST(0 AS bit)                  AS AllDay,
    'NonContact'                    AS Category,
    CAST(NULL AS nvarchar(256))     AS Location,
    CAST(NULL AS nvarchar(16))      AS ColourCode
FROM dbo.StaffNonContactAllocations AS s
JOIN dbo.vw_attendance_period_instances AS i ON i.PeriodId = s.AttendancePeriodId
WHERE s.StaffMemberId = @staffMemberId
  AND i.ActualStartTime < @to
  AND i.ActualEndTime > @from
  AND s.StartDate <= i.ActualStartTime
  AND s.EndDate >= i.ActualEndTime;";

        return QueryAsync(sql, new { staffMemberId, from, to }, cancellationToken);
    }

    public Task<IEnumerable<StaffCalendarEventResponse>> GetParentEveningAppointmentsAsync(Guid staffMemberId,
        DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT
    CONCAT('parenteve:', CONVERT(nvarchar(36), a.Id)) AS Id,
    CONCAT('Parent meeting',
           CASE WHEN nm.[Name] IS NOT NULL THEN CONCAT(': ', nm.[Name]) ELSE '' END) AS Title,
    a.[Start]                       AS [Start],
    a.[End]                         AS [End],
    CAST(0 AS bit)                  AS AllDay,
    'ParentEvening'                 AS Category,
    CAST(NULL AS nvarchar(256))     AS Location,
    CAST(NULL AS nvarchar(16))      AS ColourCode
FROM dbo.ParentEveningAppointments AS a
JOIN dbo.ParentEveningStaffMembers AS psm ON psm.Id = a.ParentEveningStaffMemberId
LEFT JOIN dbo.Students AS st ON st.Id = a.StudentId
OUTER APPLY dbo.fn_person_get_name(st.PersonId, 3, 1, 0) AS nm
WHERE psm.StaffMemberId = @staffMemberId
  AND a.[Start] < @to
  AND a.[End] > @from;";

        return QueryAsync(sql, new { staffMemberId, from, to }, cancellationToken);
    }

    public Task<IEnumerable<StaffCalendarEventResponse>> GetAbsencesAsync(Guid staffMemberId, DateTime from,
        DateTime to, bool includeConfidential, CancellationToken cancellationToken)
    {
        // All-day spans. EndDate is inclusive in the model; the calendar treats all-day end as
        // exclusive, so push it forward a day for display.
        const string sql = @"
SELECT
    CONCAT('absence:', CONVERT(nvarchar(36), ab.Id)) AS Id,
    COALESCE(t.[Description], 'Absence')             AS Title,
    ab.StartDate                                    AS [Start],
    DATEADD(DAY, 1, ab.EndDate)                     AS [End],
    CAST(1 AS bit)                                  AS AllDay,
    'Absence'                                       AS Category,
    CAST(NULL AS nvarchar(256))                     AS Location,
    CAST(NULL AS nvarchar(16))                      AS ColourCode
FROM dbo.StaffAbsences AS ab
LEFT JOIN dbo.StaffAbsenceTypes AS t ON t.Id = ab.AbsenceTypeId
WHERE ab.StaffMemberId = @staffMemberId
  AND ab.StartDate <= @to
  AND ab.EndDate >= @from
  AND (@includeConfidential = 1 OR ab.IsConfidential = 0);";

        return QueryAsync(sql, new { staffMemberId, from, to, includeConfidential }, cancellationToken);
    }
}
