using Dapper;
using Microsoft.AspNetCore.Identity;
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
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        if (await manager.FindByClientIdAsync("myportal-client") is null)
        {
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
            }

            await manager.CreateAsync(desc);
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
            await users.CreateAsync(admin, "Passw0rd!");
            await users.AddToRoleAsync(admin, "System Administrator");
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
        
        var seededTables = new[] { "GradeSets" };

        foreach (var table in seededTables)
        {
            await conn.ExecuteAsync(
                $@"UPDATE dbo.[{table}] 
SET CreatedById = @adminId, 
    LastModifiedById = @adminId 
WHERE CreatedById IS NULL OR LastModifiedById IS NULL",
                new { adminId = admin.Id });

            await conn.ExecuteAsync(
                $@"ALTER TABLE dbo.[{table}] 
ALTER COLUMN CreatedById UNIQUEIDENTIFIER NOT NULL;");
            
            await conn.ExecuteAsync(
                $@"ALTER TABLE dbo.[{table}] 
ALTER COLUMN LastModifiedById UNIQUEIDENTIFIER NOT NULL;");
        }
    }
}