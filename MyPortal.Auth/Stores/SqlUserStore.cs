using System.Security.Claims;
using Dapper;
using Microsoft.AspNetCore.Identity;
using MyPortal.Auth.Models;
using MyPortal.Common.Interfaces;

namespace MyPortal.Auth.Stores;

public class SqlUserStore : IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>,
    IUserEmailStore<ApplicationUser>, IUserRoleStore<ApplicationUser>, IUserClaimStore<ApplicationUser>,
    IUserSecurityStampStore<ApplicationUser>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private static string? Normalize(string? value) => value?.ToUpperInvariant();

    public SqlUserStore(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public void Dispose() { /* nothing to dispose */ }

    // -------------------------------------------------------
    // Create / Update / Delete
    // -------------------------------------------------------
    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.ConcurrencyStamp ??= Guid.NewGuid().ToString("N");
        user.SecurityStamp    ??= Guid.NewGuid().ToString("N");
        user.NormalizedUserName = Normalize(user.UserName);
        user.NormalizedEmail    = Normalize(user.Email);

        const string sql = @"
INSERT INTO dbo.Users
(Id, CreatedAt, PersonId, UserType, IsEnabled, IsSystem,
 UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
 PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed,
 TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount)
VALUES
(@Id, @CreatedAt, @PersonId, @UserType, @IsEnabled, @IsSystem,
 @UserName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed,
 @PasswordHash, @SecurityStamp, @ConcurrencyStamp, @PhoneNumber, @PhoneNumberConfirmed,
 @TwoFactorEnabled, @LockoutEnd, @LockoutEnabled, @AccessFailedCount);";

        using var connection = _connectionFactory.Create();
        await connection.ExecuteAsync(new CommandDefinition(sql, user, cancellationToken: cancellationToken));
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var newConcurrencyStamp = Guid.NewGuid().ToString("N");
        user.NormalizedUserName = Normalize(user.UserName);
        user.NormalizedEmail    = Normalize(user.Email);

        const string sql = @"
UPDATE dbo.Users SET
 UserName=@UserName, NormalizedUserName=@NormalizedUserName,
 Email=@Email, NormalizedEmail=@NormalizedEmail, EmailConfirmed=@EmailConfirmed,
 PasswordHash=@PasswordHash, SecurityStamp=@SecurityStamp,
 ConcurrencyStamp=@NewConcurrencyStamp, PhoneNumber=@PhoneNumber,
 PhoneNumberConfirmed=@PhoneNumberConfirmed, TwoFactorEnabled=@TwoFactorEnabled,
 LockoutEnd=@LockoutEnd, LockoutEnabled=@LockoutEnabled, AccessFailedCount=@AccessFailedCount,
 PersonId=@PersonId, UserType=@UserType, IsEnabled=@IsEnabled, IsSystem=@IsSystem
WHERE Id=@Id AND ConcurrencyStamp=@ConcurrencyStamp;";

        using var connection = _connectionFactory.Create();
        var rowsAffected = await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                user.Id,
                user.UserName,
                user.NormalizedUserName,
                user.Email,
                user.NormalizedEmail,
                user.EmailConfirmed,
                user.PasswordHash,
                user.SecurityStamp,
                NewConcurrencyStamp = newConcurrencyStamp,
                user.PhoneNumber,
                user.PhoneNumberConfirmed,
                user.TwoFactorEnabled,
                user.LockoutEnd,
                user.LockoutEnabled,
                user.AccessFailedCount,
                user.PersonId,
                user.UserType,
                user.IsEnabled,
                user.IsSystem,
                user.ConcurrencyStamp
            }, cancellationToken: cancellationToken));

        if (rowsAffected == 0)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "ConcurrencyFailure",
                Description = "The user was updated by another process."
            });
        }

        user.ConcurrencyStamp = newConcurrencyStamp;
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = "DELETE FROM dbo.Users WHERE Id=@Id;";
        using var connection = _connectionFactory.Create();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { user.Id }, cancellationToken: cancellationToken));
        return IdentityResult.Success;
    }

    // -------------------------------------------------------
    // Finders
    // -------------------------------------------------------
    public async Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = "SELECT TOP 1 * FROM dbo.Users WHERE Id=@Id;";
        using var connection = _connectionFactory.Create();
        return await connection.QuerySingleOrDefaultAsync<ApplicationUser>(
            new CommandDefinition(sql, new { Id = Guid.Parse(userId) }, cancellationToken: cancellationToken));
    }

    public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = "SELECT TOP 1 * FROM dbo.Users WHERE NormalizedUserName=@NormalizedUserName;";
        using var connection = _connectionFactory.Create();
        return await connection.QuerySingleOrDefaultAsync<ApplicationUser>(
            new CommandDefinition(sql, new { NormalizedUserName = normalizedUserName }, cancellationToken: cancellationToken));
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = "SELECT TOP 1 * FROM dbo.Users WHERE NormalizedEmail=@NormalizedEmail;";
        using var connection = _connectionFactory.Create();
        return await connection.QuerySingleOrDefaultAsync<ApplicationUser>(
            new CommandDefinition(sql, new { NormalizedEmail = normalizedEmail }, cancellationToken: cancellationToken));
    }

    // -------------------------------------------------------
    // Identity trivial getters/setters
    // -------------------------------------------------------
    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.Id.ToString());

    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.UserName);

    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.NormalizedUserName);

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        return Task.CompletedTask;
    }

    // email
    public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken)
    {
        user.Email = email;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.Email);

    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.EmailConfirmed);

    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
    {
        user.EmailConfirmed = confirmed;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.NormalizedEmail);

    public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.NormalizedEmail = normalizedEmail;
        return Task.CompletedTask;
    }

    // password
    public Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.PasswordHash);

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));

    // security stamp
    public Task SetSecurityStampAsync(ApplicationUser user, string securityStamp, CancellationToken cancellationToken)
    {
        user.SecurityStamp = securityStamp;
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(ApplicationUser user, CancellationToken cancellationToken)
        => Task.FromResult(user.SecurityStamp);

    // -------------------------------------------------------
    // Roles
    // -------------------------------------------------------
    public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var roleId = await GetRoleIdByNameAsync(roleName, cancellationToken)
                     ?? throw new InvalidOperationException($"Role '{roleName}' not found.");

        const string sql = "INSERT INTO dbo.UserRoles (Id,UserId,RoleId) VALUES (@Id,@UserId,@RoleId);";
        using var connection = _connectionFactory.Create();
        await connection.ExecuteAsync(new CommandDefinition(sql,
            new { Id = Guid.NewGuid(), UserId = user.Id, RoleId = roleId }, cancellationToken: cancellationToken));
    }

    public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var roleId = await GetRoleIdByNameAsync(roleName, cancellationToken);
        if (roleId is null) return;

        const string sql = "DELETE FROM dbo.UserRoles WHERE UserId=@UserId AND RoleId=@RoleId;";
        using var connection = _connectionFactory.Create();
        await connection.ExecuteAsync(new CommandDefinition(sql,
            new { UserId = user.Id, RoleId = roleId }, cancellationToken: cancellationToken));
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = @"
SELECT r.Name
FROM dbo.UserRoles ur
JOIN dbo.Roles r ON r.Id = ur.RoleId
WHERE ur.UserId = @UserId;";

        using var connection = _connectionFactory.Create();
        var roles = await connection.QueryAsync<string>(
            new CommandDefinition(sql, new { UserId = user.Id }, cancellationToken: cancellationToken));
        return roles.ToList();
    }

    public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = @"
SELECT 1
FROM dbo.UserRoles ur
JOIN dbo.Roles r ON r.Id = ur.RoleId
WHERE ur.UserId = @UserId AND r.NormalizedName = @NormalizedRoleName;";

        using var connection = _connectionFactory.Create();
        var exists = await connection.ExecuteScalarAsync<int?>(
            new CommandDefinition(sql, new { UserId = user.Id, NormalizedRoleName = Normalize(roleName) },
                cancellationToken: cancellationToken));

        return exists.HasValue;
    }

    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = @"
SELECT u.*
FROM dbo.UserRoles ur
JOIN dbo.Roles r ON r.Id = ur.RoleId
JOIN dbo.Users u ON u.Id = ur.UserId
WHERE r.NormalizedName = @NormalizedRoleName;";

        using var connection = _connectionFactory.Create();
        var users = await connection.QueryAsync<ApplicationUser>(
            new CommandDefinition(sql, new { NormalizedRoleName = Normalize(roleName) }, cancellationToken: cancellationToken));
        return users.ToList();
    }

    // -------------------------------------------------------
    // Claims
    // -------------------------------------------------------
    public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = "SELECT ClaimType, ClaimValue FROM dbo.UserClaims WHERE UserId=@UserId;";
        using var connection = _connectionFactory.Create();
        var rows = await connection.QueryAsync<(string? ClaimType, string? ClaimValue)>(
            new CommandDefinition(sql, new { UserId = user.Id }, cancellationToken: cancellationToken));

        return rows.Select(r => new Claim(r.ClaimType ?? string.Empty, r.ClaimValue ?? string.Empty)).ToList();
    }

    public async Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = "INSERT INTO dbo.UserClaims (Id,UserId,ClaimType,ClaimValue) VALUES (@Id,@UserId,@Type,@Value);";
        using var connection = _connectionFactory.Create();

        foreach (var claim in claims)
        {
            await connection.ExecuteAsync(new CommandDefinition(sql,
                new
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Type = claim.Type,
                    Value = claim.Value
                }, cancellationToken: cancellationToken));
        }
    }

    public async Task ReplaceClaimAsync(ApplicationUser user, Claim oldClaim, Claim newClaim, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = @"
UPDATE dbo.UserClaims
SET ClaimType=@NewType, ClaimValue=@NewValue
WHERE UserId=@UserId AND ClaimType=@OldType AND ClaimValue=@OldValue;";

        using var connection = _connectionFactory.Create();
        await connection.ExecuteAsync(new CommandDefinition(sql,
            new
            {
                UserId = user.Id,
                NewType = newClaim.Type,
                NewValue = newClaim.Value,
                OldType = oldClaim.Type,
                OldValue = oldClaim.Value
            }, cancellationToken: cancellationToken));
    }

    public async Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = "DELETE FROM dbo.UserClaims WHERE UserId=@UserId AND ClaimType=@Type AND ClaimValue=@Value;";
        using var connection = _connectionFactory.Create();

        foreach (var claim in claims)
        {
            await connection.ExecuteAsync(new CommandDefinition(sql,
                new { UserId = user.Id, Type = claim.Type, Value = claim.Value },
                cancellationToken: cancellationToken));
        }
    }

    public async Task<IList<ApplicationUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        const string sql = @"
SELECT u.*
FROM dbo.UserClaims c
JOIN dbo.Users u ON u.Id = c.UserId
WHERE c.ClaimType = @Type AND c.ClaimValue = @Value;";

        using var connection = _connectionFactory.Create();
        var users = await connection.QueryAsync<ApplicationUser>(
            new CommandDefinition(sql, new { Type = claim.Type, Value = claim.Value }, cancellationToken: cancellationToken));
        return users.ToList();
    }

    // -------------------------------------------------------
    // helper
    // -------------------------------------------------------
    private async Task<Guid?> GetRoleIdByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        const string sql = "SELECT Id FROM dbo.Roles WHERE NormalizedName=@NormalizedRoleName;";
        using var connection = _connectionFactory.Create();
        return await connection.ExecuteScalarAsync<Guid?>(
            new CommandDefinition(sql, new { NormalizedRoleName = Normalize(roleName) }, cancellationToken: cancellationToken));
    }
}