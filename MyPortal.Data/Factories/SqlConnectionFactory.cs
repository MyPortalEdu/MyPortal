using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using MyPortal.Common.Interfaces;
using MyPortal.Common.Options;

namespace MyPortal.Data.Factories;

public class SqlConnectionFactory(IOptions<DatabaseOptions> db) : IDbConnectionFactory
{
    private readonly DatabaseOptions _db = db.Value;


    public IDbConnection Create()
    {
        var conn = new SqlConnection(_db.ConnectionString);
        return conn;
    }
}