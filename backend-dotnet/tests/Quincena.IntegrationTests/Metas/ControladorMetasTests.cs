using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quincena.Api.Serialization;
using Quincena.Application.Metas;
using Quincena.Domain.Entidades;
using Quincena.IntegrationTests.Infrastructure;
using Xunit;

namespace Quincena.IntegrationTests.Metas;

// Contra Postgres real (quincena_dotnet_dev).
public class ControladorMetasTests : IClassFixture<QuincenaWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = QuincenaJsonOptions.Create();

    private readonly QuincenaWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly List<Guid> _goalIds = [];

    public ControladorMetasTests(QuincenaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var db = _factory.CreateDbContext();
        if (_goalIds.Count > 0)
            db.Metas.RemoveRange(db.Metas.Where(g => _goalIds.Contains(g.Id)));
        await db.SaveChangesAsync();
    }

    private async Task<Meta> CreateGoalAsync(string name, string type = "savings")
    {
        var res = await _client.PostAsJsonAsync("/api/metas",
            new CrearMetaPeticion { Nombre = name, Tipo = type, MontoObjetivo = 1000000 }, JsonOptions);
        res.EnsureSuccessStatusCode();
        var goal = (await res.Content.ReadFromJsonAsync<Meta>(JsonOptions))!;
        _goalIds.Add(goal.Id);
        return goal;
    }

    [Fact]
    public async Task List_solo_devuelve_metas_activas()
    {
        var activa = await CreateGoalAsync("E2E Activa");
        var inactiva = await CreateGoalAsync("E2E Inactiva");
        await _client.DeleteAsync($"/api/metas/{inactiva.Id}");

        var res = await _client.GetAsync("/api/metas");
        var goals = await res.Content.ReadFromJsonAsync<List<Meta>>(JsonOptions);

        Assert.Contains(goals!, g => g.Id == activa.Id);
        Assert.DoesNotContain(goals!, g => g.Id == inactiva.Id);
    }

    [Fact]
    public async Task Put_sin_EstaActivo_no_desactiva_la_meta()
    {
        var created = await CreateGoalAsync("E2E Vacaciones");

        var res = await _client.PutAsJsonAsync($"/api/metas/{created.Id}",
            new ActualizarMetaPeticion { Nombre = "E2E Vacaciones 2027", Tipo = "savings", MontoObjetivo = 2000000, MontoActual = 100000 },
            JsonOptions);
        res.EnsureSuccessStatusCode();
        var updated = await res.Content.ReadFromJsonAsync<Meta>(JsonOptions);

        Assert.Equal("E2E Vacaciones 2027", updated!.Nombre);
        Assert.True(updated.EstaActivo);
    }

    [Fact]
    public async Task Put_a_id_inexistente_responde_404()
    {
        var res = await _client.PutAsJsonAsync($"/api/metas/{Guid.NewGuid()}",
            new ActualizarMetaPeticion { Nombre = "X", Tipo = "savings", MontoObjetivo = 1 }, JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("No encontrada", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Delete_hace_soft_delete_y_desaparece_del_listado()
    {
        var created = await CreateGoalAsync("E2E Para borrar");

        var deleteRes = await _client.DeleteAsync($"/api/metas/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        var listRes = await _client.GetAsync("/api/metas");
        var goals = await listRes.Content.ReadFromJsonAsync<List<Meta>>(JsonOptions);
        Assert.DoesNotContain(goals!, g => g.Id == created.Id);

        await using var db = _factory.CreateDbContext();
        var stillExists = await db.Metas.FirstOrDefaultAsync(g => g.Id == created.Id);
        Assert.NotNull(stillExists);
        Assert.False(stillExists!.EstaActivo);
    }
}
