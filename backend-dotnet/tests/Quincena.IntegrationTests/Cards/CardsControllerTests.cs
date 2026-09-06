using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quincena.Api.Serialization;
using Quincena.Application.Cards;
using Quincena.Domain.Entities;
using Quincena.IntegrationTests.Infrastructure;
using Xunit;

namespace Quincena.IntegrationTests.Cards;

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
            db.Cards.RemoveRange(db.Cards.Where(c => _cardIds.Contains(c.Id)));
        await db.SaveChangesAsync();
    }

    private async Task<Card> CreateCardAsync(string name)
    {
        var res = await _client.PostAsJsonAsync("/api/cards", new CreateCardRequest { Name = name }, JsonOptions);
        res.EnsureSuccessStatusCode();
        var card = (await res.Content.ReadFromJsonAsync<Card>(JsonOptions))!;
        _cardIds.Add(card.Id);
        return card;
    }

    [Fact]
    public async Task List_solo_devuelve_tarjetas_activas()
    {
        var activa = await CreateCardAsync("E2E Activa");
        var inactiva = await CreateCardAsync("E2E Inactiva");
        await _client.DeleteAsync($"/api/cards/{inactiva.Id}");

        var res = await _client.GetAsync("/api/cards");
        var cards = await res.Content.ReadFromJsonAsync<List<Card>>(JsonOptions);

        Assert.Contains(cards!, c => c.Id == activa.Id);
        Assert.DoesNotContain(cards!, c => c.Id == inactiva.Id);
    }

    [Fact]
    public async Task Put_sin_is_active_no_desactiva_la_tarjeta()
    {
        var created = await CreateCardAsync("E2E Visa");

        var res = await _client.PutAsJsonAsync($"/api/cards/{created.Id}",
            new UpdateCardRequest { Name = "E2E Visa Gold", LastFour = "4321", Bank = "Bancolombia" }, JsonOptions);
        res.EnsureSuccessStatusCode();
        var updated = await res.Content.ReadFromJsonAsync<Card>(JsonOptions);

        Assert.Equal("E2E Visa Gold", updated!.Name);
        Assert.Equal("4321", updated.LastFour);
        Assert.True(updated.IsActive);
    }

    [Fact]
    public async Task Put_a_id_inexistente_responde_404()
    {
        var res = await _client.PutAsJsonAsync($"/api/cards/{Guid.NewGuid()}",
            new UpdateCardRequest { Name = "X" }, JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("No encontrada", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Delete_hace_soft_delete_y_desaparece_del_listado()
    {
        var created = await CreateCardAsync("E2E Para borrar");

        var deleteRes = await _client.DeleteAsync($"/api/cards/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        var listRes = await _client.GetAsync("/api/cards");
        var cards = await listRes.Content.ReadFromJsonAsync<List<Card>>(JsonOptions);
        Assert.DoesNotContain(cards!, c => c.Id == created.Id);

        await using var db = _factory.CreateDbContext();
        var stillExists = await db.Cards.FirstOrDefaultAsync(c => c.Id == created.Id);
        Assert.NotNull(stillExists);
        Assert.False(stillExists!.IsActive);
    }
}
