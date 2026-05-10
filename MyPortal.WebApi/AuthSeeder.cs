using System.Transactions;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MyPortal.Auth.Models;
using MyPortal.Common.Enums;
using MyPortal.Common.Interfaces;
using OpenIddict.Abstractions;

namespace MyPortal.WebApi;

public static class AuthSeeder
{
    // Re-runnable seeder to populate auth data
    public static async Task RunAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var users   = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles   = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var env     = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var config  = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Build the descriptor every run so changes here (new redirect URIs, scopes,
        // permissions) flow into the existing client without requiring a DB reset.
        var desc = new OpenIddictApplicationDescriptor
        {
            ClientId = "myportal-client",
            DisplayName = "MyPortal Public Client",
            ClientType = OpenIddictConstants.ClientTypes.Public,
            ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.ResponseTypes.Code,
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Scopes.Profile,
                $"{OpenIddictConstants.Permissions.Prefixes.Scope}{OpenIddictConstants.Scopes.OfflineAccess}",
                $"{OpenIddictConstants.Permissions.Prefixes.Scope}api"
            },
            Requirements =
            {
                OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
            }
        };

        if (env.IsDevelopment())
        {
            desc.RedirectUris.Add(new Uri("https://oauth.pstmn.io/v1/callback"));
            desc.RedirectUris.Add(new Uri("https://localhost:7201/scalar/v1"));
        }

        var existingClient = await manager.FindByClientIdAsync("myportal-client");
        if (existingClient is null)
        {
            await manager.CreateAsync(desc);
        }
        else
        {
            await manager.UpdateAsync(existingClient, desc);
        }
        
        var connFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        
        var roleNames = new[] { "System Administrator" };
        foreach (var rn in roleNames)
            if (!await roles.RoleExistsAsync(rn))
                await roles.CreateAsync(new ApplicationRole { Id = Guid.NewGuid(), Name = rn, IsSystem = true });
        
        var email = "admin@myportal.local";
        var admin = await users.FindByEmailAsync(email);
        if (admin == null)
        {
            // Initial admin password: must come from configuration (Auth:AdminInitialPassword,
            // typically via user-secrets / environment variable). Dev-only fallback so a fresh
            // clone runs out of the box.
            var initialPassword = config["Auth:AdminInitialPassword"];
            if (string.IsNullOrWhiteSpace(initialPassword))
            {
                if (!env.IsDevelopment())
                {
                    throw new InvalidOperationException(
                        "Auth:AdminInitialPassword must be configured to seed the initial admin user.");
                }
                initialPassword = "Passw0rd!";
            }

            admin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                Email = email,
                EmailConfirmed = true,
                UserType = UserType.Staff,
                IsEnabled = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                CreatedByIpAddress = "::1",
                LastModifiedByIpAddress = "::1",
                LastModifiedAt = DateTime.UtcNow,
                Version = 1
            };

            // Wrap create + role-add so a failure between them doesn't leave a dangling
            // admin user without their role. Restartable: next boot finds no user, retries.
            using var tx = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled);

            var createResult = await users.CreateAsync(admin, initialPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to create initial admin user: " +
                    string.Join("; ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}")));
            }

            var addToRoleResult = await users.AddToRoleAsync(admin, "System Administrator");
            if (!addToRoleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to add initial admin user to System Administrator role: " +
                    string.Join("; ", addToRoleResult.Errors.Select(e => $"{e.Code}: {e.Description}")));
            }

            tx.Complete();
        }

        using var conn = connFactory.Create();
        
        await conn.ExecuteAsync(@"
DECLARE @result int;
EXEC @result = sp_getapplock 
    @Resource = 'MyPortal.AuthSeeder',
    @LockMode = 'Exclusive',
    @LockOwner = 'Session',
    @LockTimeout = 60000;
IF @result < 0 THROW 50000, 'Could not acquire seed lock', 1;
");
        
        var allPerms = (await conn.QueryAsync<Guid>("SELECT Id FROM Permissions")).ToArray();
        var adminRoleId = await conn.QuerySingleAsync<Guid>("SELECT Id FROM Roles WHERE Name = @name", new { name = "System Administrator" });

        foreach (var perm in allPerms)
        {
            await conn.ExecuteAsync(@"
                IF NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId=@r AND PermissionId=@p)
                INSERT INTO dbo.RolePermissions(Id, RoleId, PermissionId) VALUES(@id, @r, @p);",
                new { id = Guid.NewGuid(), r = adminRoleId, p = perm });
        }

        // GradeSets audit-column backfill + NOT NULL was previously here. It has moved to
        // numbered migration 0011_gradesets_audit_not_null.sql so schema changes are journalled
        // and don't run unconditionally on every boot.
    }
}