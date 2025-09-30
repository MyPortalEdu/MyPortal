using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace MyPortal.Auth.Models;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<OpenIddictEntityFrameworkCoreApplication<Guid>> Applications => Set<OpenIddictEntityFrameworkCoreApplication<Guid>>();
    public DbSet<OpenIddictEntityFrameworkCoreAuthorization<Guid>> Authorizations => Set<OpenIddictEntityFrameworkCoreAuthorization<Guid>>();
    public DbSet<OpenIddictEntityFrameworkCoreScope<Guid>> Scopes => Set<OpenIddictEntityFrameworkCoreScope<Guid>>();
    public DbSet<OpenIddictEntityFrameworkCoreToken<Guid>> Tokens => Set<OpenIddictEntityFrameworkCoreToken<Guid>>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseOpenIddict<Guid>();
        
        modelBuilder.Entity<OpenIddictEntityFrameworkCoreApplication<Guid>>()
            .ToTable("ApiApplications");
        
        modelBuilder.Entity<OpenIddictEntityFrameworkCoreAuthorization<Guid>>()
            .ToTable("ApiAuthorizations");
        
        modelBuilder.Entity<OpenIddictEntityFrameworkCoreScope<Guid>>()
            .ToTable("ApiScopes");
        
        modelBuilder.Entity<OpenIddictEntityFrameworkCoreToken<Guid>>()
            .ToTable("ApiTokens");
    }
}