using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MyPortal.Auth.Models;
using MyPortal.Auth.Stores;
using MyPortal.Common.Options;
using MyPortal.Data.Factories;
using MyPortal.Services.Configuration;
using MyPortal.WebApi.Infrastructure;
using MyPortal.WebApi.Services;
using QueryKit.Dialects;
using QueryKit.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection("Database"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), 
        "Connection string must be provided.")
    .ValidateOnStart();

builder.Services.AddOptions<StorageOptions>()
    .Bind(builder.Configuration.GetSection("Storage"))
    .ValidateOnStart();

var db = builder.Configuration.GetSection("Database").Get<DatabaseOptions>()!;
QueryKit.Extensions.ConnectionExtensions.UseDialect(Dialect.SQLServer);

builder.Services.AddScoped<IConnectionFactory, SqlConnectionFactory>();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
    })
    .AddRoles<ApplicationRole>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserStore<ApplicationUser>, SqlUserStore>();
builder.Services.AddScoped<IRoleStore<ApplicationRole>, SqlRoleStore>();

builder.Services.AddRepositories();

builder.Services.AddMyPortalServices();

builder.Services.AddOpenIddict()
    .AddCore(o =>
    {

    })
    .AddServer(o =>
    {
        o.SetAuthorizationEndpointUris("/connect/authorize")
            .SetTokenEndpointUris("/connect/token")
            .SetUserInfoEndpointUris("/connect/userinfo")
            .SetEndSessionEndpointUris("/connect/endsession");

        o.AllowAuthorizationCodeFlow().AllowRefreshTokenFlow();

        o.RegisterScopes("api", "offline_access", "email", "profile");

        o.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        o.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<ExceptionMiddleware>();

var app = builder.Build();

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