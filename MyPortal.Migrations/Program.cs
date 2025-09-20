using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyPortal.Migrations.Services;

namespace MyPortal.Migrations;

public class Program
{
    public static async Task Main(string[] args)
    {
        var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };
        
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()     
            .AddCommandLine(args)                  
            .Build();

        string connectionString = config.GetConnectionString("MyPortal")
                                  ?? throw new InvalidOperationException("Connection string 'MyPortal' not found.");
        
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddSimpleConsole(options =>
                {
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                })
                .SetMinimumLevel(LogLevel.Information);
        });
        
        var logger = loggerFactory.CreateLogger<DbUpdateService>();
        
        var svc = new DbUpdateService(connectionString, logger);

        try
        {
            await svc.CreateOrUpdateDatabaseAsync(cts.Token);
            Console.WriteLine("Database updated completed successfully.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Database update cancelled.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Database update failed: {e.Message}");
            Environment.ExitCode = 1;
        }
    }
}