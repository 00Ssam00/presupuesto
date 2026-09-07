using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quincena.Api.Controladores;
using Quincena.Api.Serialization;
using Quincena.Application.Tarjetas;
using Quincena.Application.Categorias;
using Quincena.Application.MetodosPago;
using Quincena.Application.Periodos;
using Quincena.Application.Transacciones;
using Quincena.Domain.Entidades;
using Quincena.IntegrationTests.Infrastructure;
using Xunit;

namespace Quincena.IntegrationTests.Transacciones;

// Contra Postgres real (quincena_dotnet_dev) — cubre los JOIN reales (LINQ traducido
// a SQL), el filtrado por query string, y que un error de base de datos real (FK
// inexistente) efectivamente responde 500 vía ExceptionHandlingMiddleware, igual que
// el errorHandler.ts de Node ("propaga errores de base de datos como 500").
public class TransactionsControllerTests : IClassFixture<QuincenaWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = QuincenaJsonOptions.Create();

    private readonly QuincenaWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly List<Guid> _transactionIds = [];
    private readonly List<Guid> _periodIds = [];
    private readonly List<Guid> _categoryIds = [];
    private readonly List<Guid> _paymentMethodIds = [];

    public TransactionsControllerTests(QuincenaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var db = _factory.CreateDbContext();
        if (_transactionIds.Count > 0)
            db.Transacciones.RemoveRange(db.Transacciones.Where(t => _transactionIds.Contains(t.Id)));
        if (_periodIds.Count > 0)
            db.Periodos.RemoveRange(db.Periodos.Where(p => _periodIds.Contains(p.Id)));
        if (_categoryIds.Count > 0)
            db.Categorias.RemoveRange(db.Categorias.Where(c => _categoryIds.Contains(c.Id)));
        if (_paymentMethodIds.Count > 0)
            db.MetodosPago.RemoveRange(db.MetodosPago.Where(m => _paymentMethodIds.Contains(m.Id)));
        await db.SaveChangesAsync();
    }

    private async Task<(Periodo Periodo, Categoria Categoria, MetodoPago Method)> SeedFixturesAsync()
    {
        await using var db = _factory.CreateDbContext();
        var period = new Periodo { Etiqueta = "E2E", FechaInicio = new DateOnly(2026, 6, 1), FechaFin = new DateOnly(2026, 6, 15), IngresoBase = 0 };
        var category = new Categoria { Nombre = $"E2E-{Guid.NewGuid():N}" };
        var method = new MetodoPago { Nombre = $"E2E-{Guid.NewGuid():N}", RequiereTarjeta = false };
        db.Periodos.Add(period);
        db.Categorias.Add(category);
        db.MetodosPago.Add(method);
        await db.SaveChangesAsync();
        _periodIds.Add(period.Id);
        _categoryIds.Add(category.Id);
        _paymentMethodIds.Add(method.Id);
        return (period, category, method);
    }

    private async Task<Transaccion> CreateTransactionAsync(Periodo period, Categoria category, MetodoPago method, decimal amount = 1000m)
    {
        var res = await _client.PostAsJsonAsync("/api/transacciones", new CrearTransaccionPeticion
        {
            PeriodoId = period.Id, CategoriaId = category.Id, MetodoPagoId = method.Id,
            Tipo = "expense", Monto = amount, Descripcion = "E2E", Fecha = period.FechaInicio,
        }, JsonOptions);
        res.EnsureSuccessStatusCode();
        var transaction = (await res.Content.ReadFromJsonAsync<Transaccion>(JsonOptions))!;
        _transactionIds.Add(transaction.Id);
        return transaction;
    }

    [Fact]
    public async Task Create_no_incluye_los_campos_de_join_a_diferencia_de_List()
    {
        var (period, category, method) = await SeedFixturesAsync();

        var res = await _client.PostAsJsonAsync("/api/transacciones", new CrearTransaccionPeticion
        {
            PeriodoId = period.Id, CategoriaId = category.Id, MetodoPagoId = method.Id,
            Tipo = "expense", Monto = 1000m, Descripcion = "E2E", Fecha = period.FechaInicio,
        }, JsonOptions);
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        _transactionIds.Add(body.GetProperty("id").GetGuid());

        Assert.False(body.TryGetProperty("nombre_categoria", out _));
    }

    [Fact]
    public async Task List_incluye_los_nombres_via_join_y_filtra_por_period_id_type_y_category_id()
    {
        var (period, category, method) = await SeedFixturesAsync();
        var transaction = await CreateTransactionAsync(period, category, method);

        var res = await _client.GetAsync($"/api/transacciones?periodo_id={period.Id}&tipo=expense&categoria_id={category.Id}");
        res.EnsureSuccessStatusCode();
        var list = await res.Content.ReadFromJsonAsync<List<TransaccionListaItem>>(JsonOptions);

        var found = Assert.Single(list!, t => t.Id == transaction.Id);
        Assert.Equal(category.Nombre, found.NombreCategoria);
        Assert.Equal(method.Nombre, found.NombreMetodoPago);
        Assert.Null(found.NombreTarjeta);
    }

    [Fact]
    public async Task GetById_responde_404_con_el_shape_de_error_de_Node()
    {
        var res = await _client.GetAsync($"/api/transacciones/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("No encontrada", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Update_no_permite_cambiar_period_id_y_recalcula()
    {
        var (period, category, method) = await SeedFixturesAsync();
        var transaction = await CreateTransactionAsync(period, category, method, amount: 500m);

        var res = await _client.PutAsJsonAsync($"/api/transacciones/{transaction.Id}", new ActualizarTransaccionPeticion
        {
            CategoriaId = category.Id, MetodoPagoId = method.Id,
            Tipo = "expense", Monto = 900m, Descripcion = "Editada", Fecha = period.FechaInicio,
        }, JsonOptions);
        res.EnsureSuccessStatusCode();
        var updated = await res.Content.ReadFromJsonAsync<Transaccion>(JsonOptions);

        Assert.Equal(period.Id, updated!.PeriodoId);
        Assert.Equal(900m, updated.Monto);
    }

    [Fact]
    public async Task Delete_responde_204_y_la_transaccion_desaparece()
    {
        var (period, category, method) = await SeedFixturesAsync();
        var transaction = await CreateTransactionAsync(period, category, method);

        var deleteRes = await _client.DeleteAsync($"/api/transacciones/{transaction.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        var getRes = await _client.GetAsync($"/api/transacciones/{transaction.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getRes.StatusCode);
        _transactionIds.Remove(transaction.Id);
    }

    [Fact]
    public async Task Create_con_categoria_inexistente_responde_500_por_violacion_de_FK()
    {
        var (period, _, method) = await SeedFixturesAsync();

        var res = await _client.PostAsJsonAsync("/api/transacciones", new CrearTransaccionPeticion
        {
            PeriodoId = period.Id, CategoriaId = Guid.NewGuid(), MetodoPagoId = method.Id,
            Tipo = "expense", Monto = 1000m, Descripcion = "E2E", Fecha = period.FechaInicio,
        }, JsonOptions);

        Assert.Equal(HttpStatusCode.InternalServerError, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("error", out var error));
        Assert.False(string.IsNullOrEmpty(error.GetString())); // Development: mensaje real, no genérico
    }

    [Fact]
    public async Task Import_sin_rows_responde_400()
    {
        var res = await _client.PostAsJsonAsync("/api/transacciones/importar", new { }, JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Import_con_rows_vacio_responde_400()
    {
        var res = await _client.PostAsJsonAsync("/api/transacciones/importar", new ImportarPeticion { Filas = [] }, JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Import_de_una_fila_valida_crea_la_transaccion_y_resuelve_la_quincena_por_fecha()
    {
        var (period, category, method) = await SeedFixturesAsync();

        var res = await _client.PostAsJsonAsync("/api/transacciones/importar", new ImportarPeticion
        {
            Filas =
            [
                new FilaImportacion { Monto = "85000", Tipo = "expense", Fecha = period.FechaInicio.ToString("yyyy-MM-dd"), Categoria = category.Nombre, Metodo = method.Nombre },
            ],
        }, JsonOptions);
        res.EnsureSuccessStatusCode();
        var result = await res.Content.ReadFromJsonAsync<ResultadoImportacion>(JsonOptions);

        Assert.Equal(1, result!.Creados);
        Assert.Equal(0, result.Fallidos);
        _transactionIds.Add(result.Resultados[0].Id!.Value);
    }
}
