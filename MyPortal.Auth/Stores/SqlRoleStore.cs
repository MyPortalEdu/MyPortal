using Dapper;
using Microsoft.AspNetCore.Identity;
using MyPortal.Auth.Models;
using MyPortal.Common.Interfaces;

namespace MyPortal.Auth.Stores;

public class SqlRoleStore(IDbConnectionFactory connectionFactory) : IRoleStore<ApplicationRole>
{
    private static string? Normalize(string? value) => value?.ToUpperInvariant();

    public void Dispose() { /* nothing to dispose */ }

    public async Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        role.ConcurrencyStamp ??= Guid.NewGuid().ToString("N");
        role.NormalizedName     = Normalize(role.Name);

        const string sql = @"
INSERT INTO dbo.Roles (Id, Name, NormalizedName, ConcurrencyStamp, Description, IsSystem, UserType, IsDefault)
VALUES (@Id, @Name, @NormalizedName, @ConcurrencyStamp, @Description, @IsSystem, @UserType, @IsDefault);";

        using var connection = connectionFactory.Create();
        await connection.ExecuteAsync(new CommandDefinition(sql, role, cancellationToken: cancellationToken));
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var newConcurrencyStamp = Guid.NewGuid().ToString("N");
        var newNormalizedName = Normalize(role.Name);
        
        const string sql = @"
UPDATE dbo.Roles SET
  Name=@Name,
  NormalizedName=@NewNormalizedName,
  ConcurrencyStamp=@NewConcurrencyStamp,
  Description=@Description,
  IsSystem=@IsSystem
WHERE Id=@Id AND ConcurrencyStamp=@OldConcurrencyStamp;";

        using var connection = connectionFactory.Create();
        var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            role.Id,
            role.Name,
            NewNormalizedName = newNormalizedName,
            NewConcurrencyStamp = newConcurrencyStamp,
            role.Description,
            role.IsSystem,
            OldConcurrencyStamp = role.ConcurrencyStamp
        }, cancellationToken: cancellationToken));

        if (rowsAffected == 0)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "ConcurrencyFailure",
                Description = "The role was updated by another process."
            });
        }

        role.NormalizedName = newNormalizedName;
        role.ConcurrencyStamp = newConcurrencyStamp;
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = "DELETE FROM dbo.Roles WHERE Id=@Id;";
        using var connection = connectionFactory.Create();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { role.Id }, cancellationToken: cancellationToken));
        return IdentityResult.Success;
    }

    public Task<string> GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
        => Task.FromResult(role.Id.ToString());

    public Task<string?> GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        => Task.FromResult(role.Name);

    public Task SetRoleNameAsync(ApplicationRole role, string? roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
        => Task.FromResult(role.NormalizedName);

    public Task SetNormalizedRoleNameAsync(ApplicationRole role, string? normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public async Task<ApplicationRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Guid.TryParse(roleId, out var id))
            return null;

        const string sql = "SELECT TOP 1 * FROM dbo.Roles WHERE Id=@Id;";
        using var connection = connectionFactory.Create();
        return await connection.QuerySingleOrDefaultAsync<ApplicationRole>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<ApplicationRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = "SELECT TOP 1 * FROM dbo.Roles WHERE NormalizedName=@NormalizedName;";
        using var connection = connectionFactory.Create();
        return await connection.QuerySingleOrDefaultAsync<ApplicationRole>(
            new CommandDefinition(sql, new { NormalizedName = normalizedRoleName }, cancellationToken: cancellationToken));
    }
}