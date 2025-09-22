using Microsoft.AspNetCore.Identity;
using MyPortal.Auth.Models;
using MyPortal.Auth.Stores;
using MyPortal.Common.Options;
using MyPortal.Data.Factories;
using QueryKit.Dialects;
using QueryKit.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection("Database"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString), "Connection string must be provided.")
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
