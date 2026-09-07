using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quincena.Api.Serialization;
using Quincena.Application.Categorias;
using Quincena.Domain.Entidades;
using Quincena.IntegrationTests.Infrastructure;
using Xunit;

namespace Quincena.IntegrationTests.Categorias;

// Contra Postgres real (quincena_dotnet_dev) — cubre lo que las pruebas unitarias
// mockeadas no pueden: que EF Core realmente persiste y relee los cambios, y que
// el filtro is_active = TRUE de la lista se traduce a SQL correctamente.
public class CategoriesControllerTests : IClassFixture<QuincenaWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = QuincenaJsonOptions.Create();

    private readonly QuincenaWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly List<Guid> _categoryIds = [];

    public CategoriesControllerTests(QuincenaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var db = _factory.CreateDbContext();
        if (_categoryIds.Count > 0)
            db.Categorias.RemoveRange(db.Categorias.Where(c => _categoryIds.Contains(c.Id)));
        await db.SaveChangesAsync();
    }

    private async Task<Categoria> CreateCategoryAsync(string name)
    {
        var res = await _client.PostAsJsonAsync("/api/categorias", new CrearCategoriaPeticion { Nombre = name }, JsonOptions);
        res.EnsureSuccessStatusCode();
        var category = (await res.Content.ReadFromJsonAsync<Categoria>(JsonOptions))!;
        _categoryIds.Add(category.Id);
        return category;
    }

    [Fact]
    public async Task List_solo_devuelve_categorias_activas()
    {
        var activa = await CreateCategoryAsync("E2E Activa");
        var inactiva = await CreateCategoryAsync("E2E Inactiva");
        await _client.DeleteAsync($"/api/categorias/{inactiva.Id}");

        var res = await _client.GetAsync("/api/categorias");
        var categories = await res.Content.ReadFromJsonAsync<List<Categoria>>(JsonOptions);

        Assert.Contains(categories!, c => c.Id == activa.Id);
        Assert.DoesNotContain(categories!, c => c.Id == inactiva.Id);
    }

    [Fact]
    public async Task Put_solo_con_name_y_color_no_borra_icon_ni_sort_order()
    {
        var created = await CreateCategoryAsync("E2E Original");
        await using (var db = _factory.CreateDbContext())
        {
            var entity = await db.Categorias.FirstAsync(c => c.Id == created.Id);
            entity.Icono = "star";
            entity.Orden = 7;
            await db.SaveChangesAsync();
        }

        var res = await _client.PutAsJsonAsync($"/api/categorias/{created.Id}",
            new ActualizarCategoriaPeticion { Nombre = "E2E Editada", Color = "#123456" }, JsonOptions);
        res.EnsureSuccessStatusCode();
        var updated = await res.Content.ReadFromJsonAsync<Categoria>(JsonOptions);

        Assert.Equal("E2E Editada", updated!.Nombre);
        Assert.Equal("star", updated.Icono);
        Assert.Equal(7, updated.Orden);
        Assert.True(updated.EstaActivo);
    }

    [Fact]
    public async Task Put_a_id_inexistente_responde_404()
    {
        var res = await _client.PutAsJsonAsync($"/api/categorias/{Guid.NewGuid()}",
            new ActualizarCategoriaPeticion { Nombre = "X", Color = "#000000" }, JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("No encontrada", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Delete_hace_soft_delete_y_desaparece_del_listado()
    {
        var created = await CreateCategoryAsync("E2E Para borrar");

        var deleteRes = await _client.DeleteAsync($"/api/categorias/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        var listRes = await _client.GetAsync("/api/categorias");
        var categories = await listRes.Content.ReadFromJsonAsync<List<Categoria>>(JsonOptions);
        Assert.DoesNotContain(categories!, c => c.Id == created.Id);

        await using var db = _factory.CreateDbContext();
        var stillExists = await db.Categorias.FirstOrDefaultAsync(c => c.Id == created.Id);
        Assert.NotNull(stillExists); // soft-delete: sigue en la tabla, solo is_active=false
        Assert.False(stillExists!.EstaActivo);
    }
}
