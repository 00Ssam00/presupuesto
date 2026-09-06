using Npgsql;
using Quincena.Api.Serialization;
using Quincena.Application;
using Quincena.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --- Conexión a PostgreSQL ---
// En producción (Render) la variable de entorno DATABASE_URL llega como URI estilo
// "postgresql://user:pass@host/db?sslmode=require" (igual que el Node actual). Npgsql
// necesita el formato keyword=value, así que se convierte aquí si viene en formato URI.
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string connectionString;
if (!string.IsNullOrEmpty(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var csb = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "",
        Database = uri.AbsolutePath.TrimStart('/'),
        SslMode = SslMode.Require,
    };
    connectionString = csb.ConnectionString;
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("No hay DATABASE_URL ni ConnectionStrings:DefaultConnection configurados.");
}

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();

// --- JSON: paridad byte a byte con las respuestas del backend Node actual ---
builder.Services.AddControllers().AddJsonOptions(options =>
{
    QuincenaJsonOptions.Configure(options.JsonSerializerOptions);
    // Los nulls se incluyen (no WhenWritingNull) — el frontend espera "card_id": null, etc.
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.MapControllers();

app.Run();

public partial class Program { } // visible para Quincena.IntegrationTests (WebApplicationFactory<Program>)
