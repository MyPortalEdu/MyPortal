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
        
        if (await manager.FindByClientIdAsync("myportal-client") is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "myportal-client",
                DisplayName = "MyPortal Public Client",
                RedirectUris =
                {
                    
                },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    "scp:offline_access",
                    "scp:api"
                }
            });
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
                CreatedAt = DateTime.UtcNow
            };
            await users.CreateAsync(admin, "Passw0rd!");
            await users.AddToRoleAsync(admin, "System Administrator");
        }

        using var conn = connFactory.Create();
        var allPerms = (await conn.QueryAsync<Guid>("SELECT Id FROM Permissions")).ToArray();
        var adminRoleId = await conn.QuerySingleAsync<Guid>("SELECT Id FROM Roles WHERE Name = @name", new { name = "System Administrator" });

        foreach (var perm in allPerms)
        {
            await conn.ExecuteAsync(@"
                IF NOT EXISTS (SELECT 1 FROM dbo.RolePermissions WHERE RoleId=@r AND PermissionId=@p)
                INSERT INTO dbo.RolePermissions(Id, RoleId, PermissionId) VALUES(@id, @r, @p);",
                new { id = Guid.NewGuid(), r = adminRoleId, p = perm });
        }
    }
}