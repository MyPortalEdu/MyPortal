using Dapper;

namespace MyPortal.Data.Parameters;

public static class TableValuedParameters
{
    public static SqlMapper.ICustomQueryParameter ToGuidTvp(this IEnumerable<Guid> guids)
    {
        var table = new System.Data.DataTable();
        table.Columns.Add("Value", typeof(Guid));
        foreach (var guid in guids)
        {
            table.Rows.Add(guid);
        }
        
        return table.AsTableValuedParameter("dbo.GuidList");
    }
}