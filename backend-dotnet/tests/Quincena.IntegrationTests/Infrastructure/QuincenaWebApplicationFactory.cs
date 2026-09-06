using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quincena.Infrastructure;

namespace Quincena.IntegrationTests.Infrastructure;

// Corre la API real contra el Postgres local de desarrollo (quincena_dotnet_dev), el
// mismo que usa `dotnet run` — no InMemory/Sqlite, para no ocultar bugs de tipos NUMERIC/
// DATE/CHECK constraints. Fuerza el entorno "Development" para leer la cadena de conexión
// de appsettings.Development.json y evita que un DATABASE_URL de producción se cuele por
// el entorno del proceso.
public class QuincenaWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("DATABASE_URL", null);
        builder.UseEnvironment("Development");
    }

    public QuincenaDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<QuincenaDbContext>();
    }
}
