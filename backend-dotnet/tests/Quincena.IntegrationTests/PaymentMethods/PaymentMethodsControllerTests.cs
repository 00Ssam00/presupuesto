using System.Net.Http.Json;
using System.Text.Json;
using Quincena.Api.Serialization;
using Quincena.IntegrationTests.Infrastructure;
using Xunit;
using PaymentMethodEntity = Quincena.Domain.Entities.PaymentMethod;

namespace Quincena.IntegrationTests.PaymentMethods;

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
            db.PaymentMethods.RemoveRange(db.PaymentMethods.Where(p => _paymentMethodIds.Contains(p.Id)));
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task List_solo_devuelve_metodos_activos_ordenados_por_nombre()
    {
        await using (var db = _factory.CreateDbContext())
        {
            var activo = new PaymentMethodEntity { Name = "E2E Zeta", IsActive = true };
            var inactivo = new PaymentMethodEntity { Name = "E2E Alfa", IsActive = false };
            db.PaymentMethods.AddRange(activo, inactivo);
            await db.SaveChangesAsync();
            _paymentMethodIds.Add(activo.Id);
            _paymentMethodIds.Add(inactivo.Id);
        }

        var res = await _client.GetAsync("/api/payment-methods");
        res.EnsureSuccessStatusCode();
        var methods = await res.Content.ReadFromJsonAsync<List<PaymentMethodEntity>>(JsonOptions);

        Assert.Contains(methods!, p => p.Name == "E2E Zeta");
        Assert.DoesNotContain(methods!, p => p.Name == "E2E Alfa");
    }
}
