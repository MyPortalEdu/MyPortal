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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
