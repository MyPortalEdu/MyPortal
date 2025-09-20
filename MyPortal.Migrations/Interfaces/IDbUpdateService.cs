namespace MyPortal.Migrations.Interfaces;

public interface IDbUpdateService
{
    Task CreateOrUpdateDatabaseAsync(CancellationToken cancellationToken);
}