using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using MyPortal.Migrations.Interfaces;

namespace MyPortal.Migrations.Services;

public class DbUpdateService : IDbUpdateService
{
    private static readonly Regex GoSplitter = new(
        pattern: @"^\s*GO(?:\s+\d+)?\s*(?:--.*)?$",
        options: RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex DbLevelDetector = new(
        pattern: @"\b(?:CREATE|ALTER)\s+DATABASE\b|\bUSE\s+\[",
        options: RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly string _connectionString;
    private readonly ILogger<DbUpdateService> _log;
    private readonly Assembly _assembly;

    public DbUpdateService(
        string connectionString,
        ILogger<DbUpdateService> logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _log = logger ?? throw new ArgumentNullException(nameof(logger));
        _assembly = typeof(DbUpdateService).Assembly;
    }

    public async Task CreateOrUpdateDatabaseAsync(CancellationToken cancellationToken)
    {
        var brandNewDb = await EnsureDatabaseExistsAsync(cancellationToken);

        if (brandNewDb)
        {
            await ConfigureDatabaseAsync(cancellationToken);
            await ApplySchemaScriptAsync("MyPortal.Migrations.Sql.Scripts.db_create_tables.sql", cancellationToken);
        }

        await ApplyUpdates(cancellationToken);
        await CreateFunctions(cancellationToken);
        await CreateStoredProcedures(cancellationToken);
        await CreateViews(cancellationToken);
    }

    private string GetMasterConnectionString()
    {
        var builder = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "master"
        };
        return builder.ConnectionString;
    }

    private string GetDatabaseName()
    {
        var builder = new SqlConnectionStringBuilder(_connectionString);
        return builder.InitialCatalog;
    }

    private async Task CreateFunctions(CancellationToken cancellationToken)
        => await ExecuteScriptsUnderNamespaceAsync("MyPortal.Migrations.Sql.Functions", cancellationToken);

    private async Task CreateViews(CancellationToken cancellationToken)
        => await ExecuteScriptsUnderNamespaceAsync("MyPortal.Migrations.Sql.Views", cancellationToken);

    private async Task CreateStoredProcedures(CancellationToken cancellationToken)
        => await ExecuteScriptsUnderNamespaceAsync("MyPortal.Migrations.Sql.StoredProcedures", cancellationToken);
    
    private async Task ApplyUpdates(CancellationToken ct)
    {
        const string baseNamespace = "MyPortal.Migrations.Sql.Updates";

        // find scripts
        var resourceNames = _assembly
            .GetManifestResourceNames()
            .Where(n => n.StartsWith(baseNamespace + ".", StringComparison.Ordinal) &&
                        n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (resourceNames.Length == 0)
        {
            _log.LogInformation("No update scripts found under '{Namespace}'", baseNamespace);
            return;
        }

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        foreach (var resourceName in resourceNames)
        {
            var fileName = GetFileName(resourceName);
            
            ct.ThrowIfCancellationRequested();

            var raw = await ReadEmbeddedTextAsync(resourceName, ct);
            var scriptText = ReplaceTokens(raw);

            if (string.IsNullOrWhiteSpace(scriptText))
            {
                _log.LogWarning("Update script '{ResourceName}' is empty. Skipping", fileName);
                continue;
            }

            // Guard: updates should not contain DB-level statements
            if (IsDbLevelScript(scriptText))
            {
                throw new InvalidOperationException(
                    $"Update script '{resourceName}' contains DB-level statements (CREATE/ALTER DATABASE or USE).");
            }
            
            if (await IsScriptAppliedAsync(conn, fileName, ct))
            {
                _log.LogInformation("Already applied: {Script}", fileName);
                continue;
            }

            var hash = ComputeSha256(scriptText); // 32 bytes
            var sw = Stopwatch.StartNew();

            await using var tx = await conn.BeginTransactionAsync(ct);
            try
            {
                // optional: set options helpful for DDL/DML
                await ExecAsync(conn, (SqlTransaction)tx,
                    "SET XACT_ABORT ON; SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;", ct);

                foreach (var batch in SplitOnGo(scriptText))
                {
                    var sql = batch.Trim();
                    if (sql.Length == 0) continue;
                    await ExecAsync(conn, (SqlTransaction)tx, sql, ct);
                }

                await tx.CommitAsync(ct);
                sw.Stop();

                await InsertJournalAsync(conn, fileName, success: true, sw.ElapsedMilliseconds, hash,
                    errorMessage: null, ct);
                _log.LogInformation("Applied update: {Script} ({Ms} ms)", fileName, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                try
                {
                    await tx.RollbackAsync(ct);
                }
                catch
                {
                    /* best effort */
                }

                sw.Stop();
                await InsertJournalAsync(conn, fileName, success: false, sw.ElapsedMilliseconds, hash,
                    Truncate4000(ex.Message), ct);

                _log.LogError(ex, "Failed applying update: {Script}", fileName);
                throw; // let host decide whether to stop or continue
            }
        }
    }

    private static byte[] ComputeSha256(string text)
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(text));
    }

    private static string Truncate4000(string? s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s.Length <= 4000 ? s : s.Substring(0, 4000);
    }

    private static async Task<bool> IsScriptAppliedAsync(SqlConnection conn, string scriptName, CancellationToken ct)
    {
        const string sql = @"SELECT 1 FROM dbo._DatabaseUpdates WHERE ScriptName = @name AND Success = 1;";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", scriptName);
        var result = await cmd.ExecuteScalarAsync(ct);
        return result is not null && result != DBNull.Value;
    }

    private static async Task InsertJournalAsync(
        SqlConnection conn,
        string scriptName,
        bool success,
        long executionMs,
        byte[] scriptHash,
        string? errorMessage,
        CancellationToken ct)
    {
        const string sql = @"
INSERT INTO dbo._DatabaseUpdates (ScriptName, ExecutionMs, ScriptHash, Success, ErrorMessage)
VALUES (@name, @ms, @hash, @success, @err);";

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", scriptName);
        cmd.Parameters.AddWithValue("@ms", (int)Math.Min(executionMs, int.MaxValue));
        cmd.Parameters.Add("@hash", SqlDbType.VarBinary, 32).Value = scriptHash;
        cmd.Parameters.AddWithValue("@success", success);
        cmd.Parameters.AddWithValue("@err", (object?)errorMessage ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync(ct);
    }


    private async Task CreateDatabaseAsync(CancellationToken cancellationToken)
    {
        await using var conn = new SqlConnection(GetMasterConnectionString());
        await conn.OpenAsync(cancellationToken);

        using (var check = new SqlCommand("SELECT DB_ID(@name)", conn))
        {
            check.Parameters.AddWithValue("@name", GetDatabaseName());
            if (await check.ExecuteScalarAsync(cancellationToken) is not DBNull) return;
        }

        _log.LogInformation("Creating database '{Db}'…", GetDatabaseName());
        using (var cmd = new SqlCommand($"CREATE DATABASE [{GetDatabaseName()}];", conn))
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        _log.LogInformation("Created database '{Db}'", GetDatabaseName());
    }

    private async Task ConfigureDatabaseAsync(CancellationToken ct)
    {
        var db = GetDatabaseName();
        await using var conn = new SqlConnection(GetMasterConnectionString());
        await conn.OpenAsync(ct);

        await ExecAsync(conn, null, $"ALTER DATABASE [{db}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", ct);

        try
        {
            var alters = new[]
            {
                //$"ALTER DATABASE [{db}] SET RECOVERY SIMPLE;",
                $"ALTER DATABASE [{db}] SET READ_COMMITTED_SNAPSHOT ON;",
                $"ALTER DATABASE [{db}] SET ALLOW_SNAPSHOT_ISOLATION ON;",
                $"ALTER DATABASE [{db}] SET AUTO_CLOSE OFF;",
                $"ALTER DATABASE [{db}] SET AUTO_SHRINK OFF;",
                $"ALTER DATABASE [{db}] SET AUTO_CREATE_STATISTICS ON;",
                $"ALTER DATABASE [{db}] SET AUTO_UPDATE_STATISTICS ON;",
                $"ALTER DATABASE [{db}] SET AUTO_UPDATE_STATISTICS_ASYNC ON;"
            };

            foreach (var sql in alters)
                await ExecAsync(conn, null, sql, ct);

            _log.LogInformation("Configured database '{Db}'", db);
        }
        finally
        {
            await ExecAsync(conn, null, $"ALTER DATABASE [{db}] SET MULTI_USER;", ct);
        }
    }

    private async Task<bool> EnsureDatabaseExistsAsync(CancellationToken ct)
    {
        await using var conn = new SqlConnection(GetMasterConnectionString());
        await conn.OpenAsync(ct);

        using var existsCmd = new SqlCommand(
            "SELECT COUNT(*) FROM sys.databases WHERE name = @name;", conn);
        existsCmd.Parameters.AddWithValue("@name", GetDatabaseName());

        var count = (int)await existsCmd.ExecuteScalarAsync(ct);
        if (count > 0)
        {
            _log.LogInformation("Database '{Db}' already exists", GetDatabaseName());
            return false; // <-- nothing new
        }

        _log.LogInformation("Database '{Db}' not found. Creating…", GetDatabaseName());

        await CreateDatabaseAsync(ct); // your existing helper

        _log.LogInformation("Database '{Db}' created", GetDatabaseName());
        return true; // <-- created now
    }

    public async Task ApplySchemaScriptAsync(string resourceName, CancellationToken ct, int commandTimeoutSeconds = 120)
    {
        var raw = await ReadEmbeddedTextAsync(resourceName, ct);
        var sql = ReplaceTokens(raw); // $(DbName)

        if (IsDbLevelScript(sql))
            throw new InvalidOperationException(
                $"'{resourceName}' contains DB-level statements; run via master without txn.");

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var tx = await conn.BeginTransactionAsync(ct);
        try
        {
            await ExecAsync(conn, (SqlTransaction)tx, "SET XACT_ABORT ON; SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;",
                ct, commandTimeoutSeconds);

            foreach (var batch in SplitOnGo(sql))
            {
                var stmt = batch.Trim();
                if (stmt.Length == 0) continue;
                await ExecAsync(conn, (SqlTransaction)tx, stmt, ct, commandTimeoutSeconds);
            }

            await tx.CommitAsync(ct);
            _log.LogInformation("Applied schema script {Script}", resourceName);
        }
        catch (Exception ex)
        {
            try
            {
                await tx.RollbackAsync(ct);
            }
            catch
            {
                /* best effort */
            }

            _log.LogError(ex, "Failed applying schema script {Script}", resourceName);
            throw;
        }
    }

    /// <summary>
    /// Finds all embedded .sql resources under the given base namespace and executes them in name order.
    /// Each .sql file runs in a single transaction, split by GO batches.
    /// </summary>
    private async Task ExecuteScriptsUnderNamespaceAsync(string baseNamespace, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var resourceNames = _assembly
            .GetManifestResourceNames()
            .Where(n => n.StartsWith(baseNamespace + ".", StringComparison.Ordinal) &&
                        n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (resourceNames.Length == 0)
        {
            _log.LogDebug("No scripts found under namespace '{Namespace}' in assembly '{AssemblyName}'",
                baseNamespace, _assembly.GetName().Name);
            return;
        }

        _log.LogInformation("Executing {Count} SQL script(s) under '{Namespace}'", resourceNames.Length, baseNamespace);

        foreach (var resourceName in resourceNames)
        {
            ct.ThrowIfCancellationRequested();

            var raw = await ReadEmbeddedTextAsync(resourceName, ct);
            var scriptText = ReplaceTokens(raw);

            if (string.IsNullOrWhiteSpace(scriptText))
            {
                _log.LogWarning("Script '{ResourceName}' is empty. Skipping", resourceName);
                continue;
            }

            var isDbLevel = IsDbLevelScript(scriptText);
            var connStr = isDbLevel ? GetMasterConnectionString() : _connectionString;

            await using var conn = new SqlConnection(connStr);
            await conn.OpenAsync(ct);

            _log.LogInformation("Running script: {ResourceName}", resourceName);

            SqlTransaction? tx = null;
            if (!isDbLevel) tx = (SqlTransaction)await conn.BeginTransactionAsync(ct);

            try
            {
                foreach (var batch in SplitOnGo(scriptText))
                {
                    ct.ThrowIfCancellationRequested();

                    var sql = batch.Trim();
                    if (sql.Length == 0) continue;

                    await ExecAsync(conn, tx, sql, ct);
                }

                if (tx is not null) await tx.CommitAsync(ct);
                _log.LogInformation("Completed script: {ResourceName}", resourceName);
            }
            catch (Exception ex)
            {
                if (tx is not null)
                {
                    try
                    {
                        await tx.RollbackAsync(ct);
                    }
                    catch
                    {
                        /* best effort */
                    }
                }

                _log.LogError(ex, "Failed executing script: {ResourceName}", resourceName);
                throw;
            }
        }
    }
    
    private static string GetFileName(string resource)
    {
        // Embedded resources use dots instead of slashes, so just take the last segment
        // Example: "MyPortal.Migrations.Sql.Updates.0007_add_x.sql" -> "0007_add_x.sql"
        var parts = resource.Split('.');
        return parts.Length > 0 ? $"{parts[^2]}.{parts[^1]}" : resource;
    }
    
    private static bool IsDbLevelScript(string sql) => DbLevelDetector.IsMatch(sql);

    private string ReplaceTokens(string sql)
        => sql.Replace("$(DbName)", GetDatabaseName(), StringComparison.OrdinalIgnoreCase);

    private static async Task ExecAsync(SqlConnection conn, SqlTransaction? tx, string sql, CancellationToken ct,
        int timeoutSeconds = 120)
    {
        await using var cmd = tx is null ? new SqlCommand(sql, conn) : new SqlCommand(sql, conn, tx);
        cmd.CommandType = CommandType.Text;
        cmd.CommandTimeout = timeoutSeconds;
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task<string> ReadEmbeddedTextAsync(string resourceName, CancellationToken ct)
    {
        // We can't use async on StreamReader.ReadToEnd in all TFMs, so buffer manually
        var asm = Assembly.GetExecutingAssembly();
        await using var stream = asm.GetManifestResourceStream(resourceName)
                                 ?? throw new InvalidOperationException($"Embedded resource not found: {resourceName}");

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true,
            bufferSize: 4096, leaveOpen: false);
        var sb = new StringBuilder(capacity: (int)stream.Length);
        char[] buf = new char[4096];

        while (true)
        {
            ct.ThrowIfCancellationRequested();
            int read = await reader.ReadAsync(buf.AsMemory(0, buf.Length), ct);
            if (read == 0) break;
            sb.Append(buf, 0, read);
        }

        return sb.ToString();
    }

    private static IEnumerable<string> SplitOnGo(string script)
    {
        // Regex.Split keeps only content between GO lines, discarding the GO lines themselves.
        // Handles optional "GO 10" (we ignore the count) and comments on the GO line.
        var parts = GoSplitter.Split(script);

        // Regex.Split never returns nulls, but we’ll be safe and filter empties later.
        return parts;
    }
}