using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quincena.Api.Serialization;
using Quincena.Application.PagosProximos;
using Quincena.Domain.Entidades;
using Quincena.IntegrationTests.Infrastructure;
using Xunit;

namespace Quincena.IntegrationTests.PagosProximos;

// Contra Postgres real (quincena_dotnet_dev).
public class ControladorPagosProximosTests : IClassFixture<QuincenaWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = QuincenaJsonOptions.Create();

    private readonly QuincenaWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly List<Guid> _paymentIds = [];
    private readonly List<Guid> _categoryIds = [];

    public ControladorPagosProximosTests(QuincenaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var db = _factory.CreateDbContext();
        if (_paymentIds.Count > 0)
            db.PagosProximos.RemoveRange(db.PagosProximos.Where(p => _paymentIds.Contains(p.Id)));
        if (_categoryIds.Count > 0)
            db.Categorias.RemoveRange(db.Categorias.Where(c => _categoryIds.Contains(c.Id)));
        await db.SaveChangesAsync();
    }

    private async Task<Categoria> SeedCategoryAsync()
    {
        await using var db = _factory.CreateDbContext();
        var category = new Categoria { Nombre = $"E2E-{Guid.NewGuid():N}" };
        db.Categorias.Add(category);
        await db.SaveChangesAsync();
        _categoryIds.Add(category.Id);
        return category;
    }

    [Fact]
    public async Task Create_y_List_devuelve_el_nombre_y_color_de_la_categoria_asociada()
    {
        var category = await SeedCategoryAsync();

        var res = await _client.PostAsJsonAsync("/api/pagos-proximos", new CrearPagoProximoPeticion
        {
            CategoriaId = category.Id,
            Nombre = "E2E Netflix",
            Monto = 25000,
            FechaVencimiento = new DateOnly(2026, 12, 1),
        }, JsonOptions);
        res.EnsureSuccessStatusCode();
        var created = (await res.Content.ReadFromJsonAsync<PagoProximo>(JsonOptions))!;
        _paymentIds.Add(created.Id);

        var listRes = await _client.GetAsync("/api/pagos-proximos");
        var items = await listRes.Content.ReadFromJsonAsync<List<PagoProximoListaItem>>(JsonOptions);
        var item = items!.Single(i => i.Id == created.Id);

        Assert.Equal(category.Nombre, item.NombreCategoria);
        Assert.Equal(category.Color, item.ColorCategoria);
        Assert.Equal("pending", item.Estado);
    }

    [Fact]
    public async Task List_filtra_por_estado()
    {
        var category = await SeedCategoryAsync();
        var pending = await CreatePaymentAsync(category.Id, "E2E Pendiente");
        var paid = await CreatePaymentAsync(category.Id, "E2E Pagado");
        await _client.PutAsJsonAsync($"/api/pagos-proximos/{paid.Id}", new ActualizarPagoProximoPeticion
        {
            CategoriaId = category.Id, Nombre = paid.Nombre, Monto = paid.Monto,
            FechaVencimiento = paid.FechaVencimiento, Estado = "paid",
        }, JsonOptions);

        var res = await _client.GetAsync("/api/pagos-proximos?estado=pending");
        var items = await res.Content.ReadFromJsonAsync<List<PagoProximoListaItem>>(JsonOptions);

        Assert.Contains(items!, i => i.Id == pending.Id);
        Assert.DoesNotContain(items!, i => i.Id == paid.Id);
    }

    private async Task<PagoProximo> CreatePaymentAsync(Guid categoryId, string name)
    {
        var res = await _client.PostAsJsonAsync("/api/pagos-proximos", new CrearPagoProximoPeticion
        {
            CategoriaId = categoryId, Nombre = name, Monto = 10000, FechaVencimiento = new DateOnly(2026, 12, 1),
        }, JsonOptions);
        res.EnsureSuccessStatusCode();
        var payment = (await res.Content.ReadFromJsonAsync<PagoProximo>(JsonOptions))!;
        _paymentIds.Add(payment.Id);
        return payment;
    }

    [Fact]
    public async Task Put_a_id_inexistente_responde_404()
    {
        var category = await SeedCategoryAsync();
        var res = await _client.PutAsJsonAsync($"/api/pagos-proximos/{Guid.NewGuid()}", new ActualizarPagoProximoPeticion
        {
            CategoriaId = category.Id, Nombre = "X", Monto = 1, FechaVencimiento = new DateOnly(2026, 1, 1), Estado = "pending",
        }, JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("No encontrado", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Delete_hace_borrado_fisico()
    {
        var category = await SeedCategoryAsync();
        var created = await CreatePaymentAsync(category.Id, "E2E Para borrar");

        var deleteRes = await _client.DeleteAsync($"/api/pagos-proximos/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);
        _paymentIds.Remove(created.Id);

        await using var db = _factory.CreateDbContext();
        var stillExists = await db.PagosProximos.FirstOrDefaultAsync(p => p.Id == created.Id);
        Assert.Null(stillExists);
    }
}
