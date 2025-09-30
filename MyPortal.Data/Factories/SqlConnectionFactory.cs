using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using MyPortal.Common.Interfaces;
using MyPortal.Common.Options;

namespace MyPortal.Data.Factories;

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _db;

    public SqlConnectionFactory(IOptions<DatabaseOptions> db)
    {
        _db = db.Value;
    }


    public IDbConnection Create()
    {
        var conn = new SqlConnection(_db.ConnectionString);
        return conn;
    }
}