using System.Data;
using Microsoft.Data.SqlClient;
using MyPortal.Common.Interfaces;
using MyPortal.Common.Options;

namespace MyPortal.Data.Factories;

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _db;

    public SqlConnectionFactory(DatabaseOptions db)
    {
        _db = db;
    }


    public IDbConnection Create()
    {
        var conn = new SqlConnection(_db.ConnectionString);
        return conn;
    }
}