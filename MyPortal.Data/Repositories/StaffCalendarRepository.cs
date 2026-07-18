using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Data.Interfaces;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

/// <summary>
/// Read-only Dapper reads backing the staff Timetable area. Not bound to a single entity — each
/// method projects one event source into the flat <see cref="StaffCalendarEventResponse"/> shape.
/// Lessons and non-contact periods reuse the register/attendance views that already expand the
/// weekly slot pattern into dated, term-clamped instances.
/// </summary>
public class StaffCalendarRepository(IDbConnectionFactory factory) : IStaffCalendarRepository
{
    private async Task<IEnumerable<StaffCalendarEventResponse>> QueryAsync(string sproc, object parameters,
        CancellationToken cancellationToken)
    {
        using var conn = factory.Create();
        return await conn.ExecuteStoredProcedureAsync<StaffCalendarEventResponse>(sproc, parameters,
            cancellationToken: cancellationToken);
    }

    public Task<IEnumerable<StaffCalendarEventResponse>> GetLessonsAsync(Guid staffMemberId, DateTime from,
        DateTime to, CancellationToken cancellationToken)
    {
        return QueryAsync("[dbo].[usp_staff_calendar_get_lessons]", new { staffMemberId, from, to },
            cancellationToken);
    }

    public Task<IEnumerable<StaffCalendarEventResponse>> GetDetentionsAsync(Guid staffMemberId, DateTime from,
        DateTime to, CancellationToken cancellationToken)
    {
        return QueryAsync("[dbo].[usp_staff_calendar_get_detentions]", new { staffMemberId, from, to },
            cancellationToken);
    }

    public Task<IEnumerable<StaffCalendarEventResponse>> GetDiaryEventsAsync(Guid personId, DateTime from,
        DateTime to, CancellationToken cancellationToken)
    {
        return QueryAsync("[dbo].[usp_staff_calendar_get_diary_events]", new { personId, from, to },
            cancellationToken);
    }

    public Task<IEnumerable<StaffCalendarEventResponse>> GetNonContactAsync(Guid staffMemberId, DateTime from,
        DateTime to, CancellationToken cancellationToken)
    {
        return QueryAsync("[dbo].[usp_staff_calendar_get_non_contact]", new { staffMemberId, from, to },
            cancellationToken);
    }

    public Task<IEnumerable<StaffCalendarEventResponse>> GetParentEveningAppointmentsAsync(Guid staffMemberId,
        DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        return QueryAsync("[dbo].[usp_staff_calendar_get_parent_evening_appointments]",
            new { staffMemberId, from, to }, cancellationToken);
    }

    public Task<IEnumerable<StaffCalendarEventResponse>> GetAbsencesAsync(Guid staffMemberId, DateTime from,
        DateTime to, bool includeConfidential, CancellationToken cancellationToken)
    {
        return QueryAsync("[dbo].[usp_staff_calendar_get_absences]",
            new { staffMemberId, from, to, includeConfidential }, cancellationToken);
    }
}
