namespace MyPortal.Data.Utilities;

internal static class SqlResourceLoader
{
    private const string SqlDir = "MyPortal.Data.Sql.System";
    
    internal static string Load(string resourceName)
    {
        var assembly = typeof(SqlResourceLoader).Assembly;
        using var stream = assembly.GetManifestResourceStream($"{SqlDir}.{resourceName}");

        if (stream == null)
        {
            throw new InvalidOperationException($"Could not find resource: {resourceName}");
        }
        
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}