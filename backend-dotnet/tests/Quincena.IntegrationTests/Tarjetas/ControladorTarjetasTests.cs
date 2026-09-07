using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quincena.Api.Serialization;
using Quincena.Application.Tarjetas;
using Quincena.Domain.Entidades;
using Quincena.IntegrationTests.Infrastructure;
using Xunit;

namespace Quincena.IntegrationTests.Tarjetas;

// Contra Postgres real (quincena_dotnet_dev).
public class CardsControllerTests : IClassFixture<QuincenaWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = QuincenaJsonOptions.Create();

    private readonly QuincenaWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly List<Guid> _cardIds = [];

    public CardsControllerTests(QuincenaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var db = _factory.CreateDbContext();
        if (_cardIds.Count > 0)
            db.Tarjetas.RemoveRange(db.Tarjetas.Where(c => _cardIds.Contains(c.Id)));
        await db.SaveChangesAsync();
    }

    private async Task<Tarjeta> CreateCardAsync(string name)
    {
        var res = await _client.PostAsJsonAsync("/api/tarjetas", new CrearTarjetaPeticion { Nombre = name }, JsonOptions);
        res.EnsureSuccessStatusCode();
        var card = (await res.Content.ReadFromJsonAsync<Tarjeta>(JsonOptions))!;
        _cardIds.Add(card.Id);
        return card;
    }

    [Fact]
    public async Task List_solo_devuelve_tarjetas_activas()
    {
        var activa = await CreateCardAsync("E2E Activa");
        var inactiva = await CreateCardAsync("E2E Inactiva");
        await _client.DeleteAsync($"/api/tarjetas/{inactiva.Id}");

        var res = await _client.GetAsync("/api/tarjetas");
        var cards = await res.Content.ReadFromJsonAsync<List<Tarjeta>>(JsonOptions);

        Assert.Contains(cards!, c => c.Id == activa.Id);
        Assert.DoesNotContain(cards!, c => c.Id == inactiva.Id);
    }

    [Fact]
    public async Task Put_sin_is_active_no_desactiva_la_tarjeta()
    {
        var created = await CreateCardAsync("E2E Visa");

        var res = await _client.PutAsJsonAsync($"/api/tarjetas/{created.Id}",
            new ActualizarTarjetaPeticion { Nombre = "E2E Visa Gold", UltimosCuatro = "4321", Banco = "Bancolombia" }, JsonOptions);
        res.EnsureSuccessStatusCode();
        var updated = await res.Content.ReadFromJsonAsync<Tarjeta>(JsonOptions);

        Assert.Equal("E2E Visa Gold", updated!.Nombre);
        Assert.Equal("4321", updated.UltimosCuatro);
        Assert.True(updated.EstaActivo);
    }

    [Fact]
    public async Task Put_a_id_inexistente_responde_404()
    {
        var res = await _client.PutAsJsonAsync($"/api/tarjetas/{Guid.NewGuid()}",
            new ActualizarTarjetaPeticion { Nombre = "X" }, JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("No encontrada", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Delete_hace_soft_delete_y_desaparece_del_listado()
    {
        var created = await CreateCardAsync("E2E Para borrar");

        var deleteRes = await _client.DeleteAsync($"/api/tarjetas/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        var listRes = await _client.GetAsync("/api/tarjetas");
        var cards = await listRes.Content.ReadFromJsonAsync<List<Tarjeta>>(JsonOptions);
        Assert.DoesNotContain(cards!, c => c.Id == created.Id);

        await using var db = _factory.CreateDbContext();
        var stillExists = await db.Tarjetas.FirstOrDefaultAsync(c => c.Id == created.Id);
        Assert.NotNull(stillExists);
        Assert.False(stillExists!.EstaActivo);
    }
}
