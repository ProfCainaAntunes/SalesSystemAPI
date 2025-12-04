using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BackEndAPI.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// --- construir connectionstring ---
// Prioriza DATABASE_URL (gerada pelo Render). Se não existir, usa a connection string do appsettings.
string npgsqlConn;
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(databaseUrl))
{
    // databaseUrl: postgres://user:pass@host:port/dbname
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');

    var builderConn = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port,
        Username = userInfo[0],
        Password = userInfo[1],
        Database = uri.AbsolutePath.TrimStart('/'),
        SslMode = SslMode.Require
    };
    npgsqlConn = builderConn.ToString();
}
else
{
    npgsqlConn = builder.Configuration.GetConnectionString("BackEndAPIContext")
                ?? throw new InvalidOperationException("Connection string 'BackEndAPIContext' not found.");
}

builder.Services.AddDbContext<BackEndAPIContext>(options =>
    options.UseNpgsql(npgsqlConn));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Migra o database durante startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BackEndAPIContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
