using System.Data;
using Dapper;

namespace MyPortal.Data.Extensions;

/// <summary>
/// Dapper type handler for <see cref="TimeOnly"/>. SQL Server stores time-of-day in
/// a <c>TIME</c> column, which ADO.NET surfaces as <see cref="TimeSpan"/>. Without an
/// explicit handler, Dapper's entity-property binder doesn't always set the parameter
/// type correctly when a <c>TimeOnly</c> sits inside an inserted entity (e.g.
/// <c>AttendancePeriod.StartTime</c>), resulting in NULL being sent to a NOT NULL
/// column.
/// </summary>
public sealed class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override TimeOnly Parse(object value) =>
        value switch
        {
            TimeSpan ts => TimeOnly.FromTimeSpan(ts),
            DateTime dt => TimeOnly.FromDateTime(dt),
            string s => TimeOnly.Parse(s),
            _ => throw new DataException($"Cannot convert {value?.GetType().Name ?? "null"} to TimeOnly."),
        };

    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.DbType = DbType.Time;
        parameter.Value = value.ToTimeSpan();
    }
}
