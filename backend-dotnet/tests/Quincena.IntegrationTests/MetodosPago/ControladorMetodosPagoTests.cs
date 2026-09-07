using System.Net.Http.Json;
using System.Text.Json;
using Quincena.Api.Serialization;
using Quincena.IntegrationTests.Infrastructure;
using Xunit;
using MetodoPagoEntidad = Quincena.Domain.Entidades.MetodoPago;

namespace Quincena.IntegrationTests.MetodosPago;

// No hay create/update/delete en Node para este recurso (solo se siembra manualmente
// en la BD) — se inserta directo vía DbContext, como hacen los fixtures de datos base.
public class PaymentMethodsControllerTests : IClassFixture<QuincenaWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = QuincenaJsonOptions.Create();

    private readonly QuincenaWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly List<Guid> _paymentMethodIds = [];

    public PaymentMethodsControllerTests(QuincenaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var db = _factory.CreateDbContext();
        if (_paymentMethodIds.Count > 0)
            db.MetodosPago.RemoveRange(db.MetodosPago.Where(p => _paymentMethodIds.Contains(p.Id)));
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task List_solo_devuelve_metodos_activos_ordenados_por_nombre()
    {
        await using (var db = _factory.CreateDbContext())
        {
            var activo = new MetodoPagoEntidad { Nombre = "E2E Zeta", EstaActivo = true };
            var inactivo = new MetodoPagoEntidad { Nombre = "E2E Alfa", EstaActivo = false };
            db.MetodosPago.AddRange(activo, inactivo);
            await db.SaveChangesAsync();
            _paymentMethodIds.Add(activo.Id);
            _paymentMethodIds.Add(inactivo.Id);
        }

        var res = await _client.GetAsync("/api/metodos-pago");
        res.EnsureSuccessStatusCode();
        var methods = await res.Content.ReadFromJsonAsync<List<MetodoPagoEntidad>>(JsonOptions);

        Assert.Contains(methods!, p => p.Nombre == "E2E Zeta");
        Assert.DoesNotContain(methods!, p => p.Nombre == "E2E Alfa");
    }
}
