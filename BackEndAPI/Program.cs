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
    // Algumas vezes o Render retorna "postgresql://" — uniformizamos para "postgres://"
    databaseUrl = databaseUrl.Replace("postgresql://", "postgres://");

    var uri = new Uri(databaseUrl);

    // Se a URL não contém porta (uri.Port == -1), usar 5432 por padrão
    var port = uri.Port == -1 ? 5432 : uri.Port;

    var userInfo = uri.UserInfo.Split(':', 2); // evita erro se senha contiver ':'
    var username = userInfo.Length > 0 ? userInfo[0] : "";
    var password = userInfo.Length > 1 ? userInfo[1] : "";

    var builderConn = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = port,
        Username = username,
        Password = password,
        Database = uri.AbsolutePath.TrimStart('/'),
        SslMode = SslMode.Require
    };

    npgsqlConn = builderConn.ToString();
}
else
{
    // fallback para variáveis separadas (opcional)
    var host = Environment.GetEnvironmentVariable("DB_HOST");
    var portStr = Environment.GetEnvironmentVariable("DB_PORT");
    int port = 5432;
    if (!string.IsNullOrEmpty(portStr) && int.TryParse(portStr, out var p)) port = p;

    var user = Environment.GetEnvironmentVariable("DB_USER");
    var pass = Environment.GetEnvironmentVariable("DB_PASSWORD");
    var dbName = Environment.GetEnvironmentVariable("DB_NAME");

    if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(dbName))
    {
        var builderConn = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Username = user,
            Password = pass,
            Database = dbName,
            SslMode = SslMode.Require
        };
        npgsqlConn = builderConn.ToString();
    }
    else
    {
        // último recurso: pegar de appsettings
        npgsqlConn = builder.Configuration.GetConnectionString("BackEndAPIContext")
            ?? throw new InvalidOperationException("Connection string not found.");
    }
}

builder.Services.AddDbContext<BackEndAPIContext>(options =>
    options.UseNpgsql(npgsqlConn));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    // Define uma política chamada "FrontendPolicy" (você pode escolher o nome)
    options.AddPolicy(name: "FrontendPolicy",
                      builder =>
                      {
                          // 1. Permite as origens específicas do seu frontend
                          builder.WithOrigins("http://localhost:3000") 
                                 // 2. Permite todos os métodos HTTP
                                 .AllowAnyMethod()  
                                 // 3. Permite todos os cabeçalhos
                                 .AllowAnyHeader();
                      });

    /*options.AddDefaultPolicy(
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });*/
});

var app = builder.Build();

app.UseCors("AllowFrontEnd");

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
