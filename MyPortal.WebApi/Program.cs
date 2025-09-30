using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyPortal.Auth.Handlers;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Models;
using MyPortal.Auth.Providers;
using MyPortal.Auth.Stores;
using MyPortal.Common.Interfaces;
using MyPortal.Common.Options;
using MyPortal.Data.Factories;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Security;
using MyPortal.Services.Configuration;
using MyPortal.WebApi;
using MyPortal.WebApi.Infrastructure;
using MyPortal.WebApi.Services;
using QueryKit.Dialects;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection("Database"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), 
        "Connection string must be provided.")
    .ValidateOnStart();

builder.Services.AddOptions<StorageOptions>()
    .Bind(builder.Configuration.GetSection("Storage"))
    .ValidateOnStart();

QueryKit.Extensions.ConnectionExtensions.UseDialect(Dialect.SQLServer);

builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<AuthDbContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetSection("Database:ConnectionString").Value);
});

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<ApplicationRole>()
    .AddUserStore<SqlUserStore>()
    .AddRoleStore<SqlRoleStore>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddOpenIddict()
    .AddCore(o =>
    {
        o.UseEntityFrameworkCore()
            .UseDbContext<AuthDbContext>()
            .ReplaceDefaultEntities<Guid>();
    })
    .AddServer(o =>
    {
        o.SetAccessTokenLifetime(TimeSpan.FromMinutes(60));
        o.SetRefreshTokenLifetime(TimeSpan.FromDays(14));
        
        o.SetTokenEndpointUris("/connect/token");
        o.SetAuthorizationEndpointUris("/connect/authorize");
        o.SetEndSessionEndpointUris("/connect/endsession");
        o.SetUserInfoEndpointUris("/connect/userinfo");

        // TODO: remove once client applications can use authorization code flow
        o.AllowPasswordFlow();
        
        o.AcceptAnonymousClients();
        o.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();
        o.AllowRefreshTokenFlow();

        o.RegisterScopes("api", "email", "profile", "offline_access");

        o.AddDevelopmentEncryptionCertificate();
        o.AddDevelopmentSigningCertificate();

        o.UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough()
            .EnableUserInfoEndpointPassthrough();
    })
    .AddValidation(o =>
    {
        o.UseLocalServer();
        o.UseAspNetCore();
    });

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = OpenIddict.Validation.AspNetCore
        .OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIddict.Validation.AspNetCore
        .OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<IAuthorizationHandler, UserTypeHandler>();

builder.Services.AddSingleton<IAuthorizationPolicyProvider, ApplicationPolicyProvider>();

builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IRoleAccessor, SqlRoleAccessor>();
builder.Services.AddScoped<IRolePermissionCache, RolePermissionCache>();
builder.Services.AddScoped<IRolePermissionProvider, SqlRolePermissionProvider>();

builder.Services.AddRepositories();
builder.Services.AddMyPortalServices();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<ExceptionMiddleware>();

var app = builder.Build();

await Seed.RunAsync(app.Services);

app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();