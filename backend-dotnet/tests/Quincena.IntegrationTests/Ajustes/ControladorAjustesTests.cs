using System.Net.Http.Json;
using System.Text.Json;
using Quincena.Api.Controladores;
using Quincena.Api.Serialization;
using Quincena.IntegrationTests.Infrastructure;
using Xunit;

namespace Quincena.IntegrationTests.Ajustes;

// Contra Postgres real (quincena_dotnet_dev).
public class ControladorAjustesTests : IClassFixture<QuincenaWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = QuincenaJsonOptions.Create();

    private readonly QuincenaWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly List<string> _keys = [];

    public ControladorAjustesTests(QuincenaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var db = _factory.CreateDbContext();
        if (_keys.Count > 0)
            db.Ajustes.RemoveRange(db.Ajustes.Where(a => _keys.Contains(a.Clave)));
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Put_crea_el_ajuste_si_la_clave_no_existe()
    {
        var key = $"e2e-{Guid.NewGuid():N}";
        _keys.Add(key);

        var res = await _client.PutAsJsonAsync($"/api/ajustes/{key}",
            new ControladorAjustes.ActualizarAjustePeticion { Valor = "COP" }, JsonOptions);
        res.EnsureSuccessStatusCode();

        var getRes = await _client.GetAsync("/api/ajustes");
        var settings = await getRes.Content.ReadFromJsonAsync<Dictionary<string, string>>(JsonOptions);
        Assert.Equal("COP", settings![key]);
    }

    [Fact]
    public async Task Put_actualiza_el_ajuste_si_la_clave_ya_existe()
    {
        var key = $"e2e-{Guid.NewGuid():N}";
        _keys.Add(key);
        await _client.PutAsJsonAsync($"/api/ajustes/{key}",
            new ControladorAjustes.ActualizarAjustePeticion { Valor = "COP" }, JsonOptions);

        var res = await _client.PutAsJsonAsync($"/api/ajustes/{key}",
            new ControladorAjustes.ActualizarAjustePeticion { Valor = "USD" }, JsonOptions);
        res.EnsureSuccessStatusCode();

        var getRes = await _client.GetAsync("/api/ajustes");
        var settings = await getRes.Content.ReadFromJsonAsync<Dictionary<string, string>>(JsonOptions);
        Assert.Equal("USD", settings![key]);
    }
}
