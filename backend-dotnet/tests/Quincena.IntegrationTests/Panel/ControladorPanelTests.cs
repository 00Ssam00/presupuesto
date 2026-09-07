using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Quincena.Api.Serialization;
using Quincena.Domain.Entidades;
using Quincena.IntegrationTests.Infrastructure;
using Xunit;

namespace Quincena.IntegrationTests.Panel;

// Contra Postgres real — cubre el LEFT JOIN + subquery de expenses_by_category, el
// filtro OR period_id IS NULL de upcoming_payments, y sobre todo que
// available/total_expenses/total_income_extra/remaining salgan como JSON number
// plano (sin comillas), a diferencia de todos los demás campos NUMERIC de la app.
public class DashboardControllerTests : IClassFixture<QuincenaWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = QuincenaJsonOptions.Create();

    private readonly QuincenaWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly List<Guid> _periodIds = [];
    private readonly List<Guid> _categoryIds = [];
    private readonly List<Guid> _paymentMethodIds = [];
    private readonly List<Guid> _transactionIds = [];
    private readonly List<Guid> _budgetIds = [];
    private readonly List<Guid> _upcomingPaymentIds = [];

    public DashboardControllerTests(QuincenaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var db = _factory.CreateDbContext();
        if (_upcomingPaymentIds.Count > 0)
            db.PagosProximos.RemoveRange(db.PagosProximos.Where(u => _upcomingPaymentIds.Contains(u.Id)));
        if (_budgetIds.Count > 0)
            db.Presupuestos.RemoveRange(db.Presupuestos.Where(b => _budgetIds.Contains(b.Id)));
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

    [Fact]
    public async Task GetById_inexistente_responde_404_con_mensaje_propio()
    {
        var res = await _client.GetAsync($"/api/panel/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Período no encontrado", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Available_total_expenses_y_remaining_salen_como_numero_json_plano()
    {
        await using var db = _factory.CreateDbContext();
        var period = new Periodo { Etiqueta = "E2E Panel", FechaInicio = new DateOnly(2026, 6, 1), FechaFin = new DateOnly(2026, 6, 15), IngresoBase = 1000000m, Arrastre = 50000m };
        var category = new Categoria { Nombre = $"E2E-{Guid.NewGuid():N}" };
        var method = new MetodoPago { Nombre = $"E2E-{Guid.NewGuid():N}" };
        db.Periodos.Add(period);
        db.Categorias.Add(category);
        db.MetodosPago.Add(method);
        await db.SaveChangesAsync();
        _periodIds.Add(period.Id);
        _categoryIds.Add(category.Id);
        _paymentMethodIds.Add(method.Id);

        var transaction = new Transaccion
        {
            PeriodoId = period.Id, CategoriaId = category.Id, MetodoPagoId = method.Id,
            Tipo = "expense", Monto = 300000m, Descripcion = "E2E", Fecha = period.FechaInicio,
        };
        db.Transacciones.Add(transaction);
        await db.SaveChangesAsync();
        _transactionIds.Add(transaction.Id);

        var res = await _client.GetAsync($"/api/panel/{period.Id}");
        res.EnsureSuccessStatusCode();
        var raw = await res.Content.ReadAsStringAsync();
        var body = JsonDocument.Parse(raw).RootElement;

        // ValueKind.Number confirma que NO están entre comillas (a diferencia de
        // periodo.ingreso_base, que sí debe seguir siendo string).
        Assert.Equal(JsonValueKind.Number, body.GetProperty("disponible").ValueKind);
        Assert.Equal(JsonValueKind.Number, body.GetProperty("total_gastos").ValueKind);
        Assert.Equal(JsonValueKind.Number, body.GetProperty("restante").ValueKind);
        Assert.Equal(JsonValueKind.String, body.GetProperty("periodo").GetProperty("ingreso_base").ValueKind);

        Assert.Equal(1050000d, body.GetProperty("disponible").GetDouble());
        Assert.Equal(750000d, body.GetProperty("restante").GetDouble());
    }

    [Fact]
    public async Task ExpensesByCategory_incluye_todas_las_categorias_activas_ordenadas_por_total_desc()
    {
        await using var db = _factory.CreateDbContext();
        var period = new Periodo { Etiqueta = "E2E Panel 2", FechaInicio = new DateOnly(2026, 6, 1), FechaFin = new DateOnly(2026, 6, 15) };
        var catAlta = new Categoria { Nombre = $"E2E-Alta-{Guid.NewGuid():N}" };
        var catBaja = new Categoria { Nombre = $"E2E-Baja-{Guid.NewGuid():N}" };
        var method = new MetodoPago { Nombre = $"E2E-{Guid.NewGuid():N}" };
        db.Periodos.Add(period);
        db.Categorias.AddRange(catAlta, catBaja);
        db.MetodosPago.Add(method);
        await db.SaveChangesAsync();
        _periodIds.Add(period.Id);
        _categoryIds.Add(catAlta.Id);
        _categoryIds.Add(catBaja.Id);
        _paymentMethodIds.Add(method.Id);

        db.Transacciones.Add(new Transaccion { PeriodoId = period.Id, CategoriaId = catAlta.Id, MetodoPagoId = method.Id, Tipo = "expense", Monto = 90000m, Descripcion = "E2E", Fecha = period.FechaInicio });
        db.Transacciones.Add(new Transaccion { PeriodoId = period.Id, CategoriaId = catBaja.Id, MetodoPagoId = method.Id, Tipo = "expense", Monto = 10000m, Descripcion = "E2E", Fecha = period.FechaInicio });
        await db.SaveChangesAsync();
        _transactionIds.AddRange(db.Transacciones.Local.Select(t => t.Id));

        var res = await _client.GetAsync($"/api/panel/{period.Id}");
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        var items = body.GetProperty("gastos_por_categoria").EnumerateArray()
            .Where(i => i.GetProperty("categoria_id").GetGuid() == catAlta.Id || i.GetProperty("categoria_id").GetGuid() == catBaja.Id)
            .ToList();

        Assert.Equal(2, items.Count);
        Assert.Equal(catAlta.Id, items[0].GetProperty("categoria_id").GetGuid()); // mayor total primero
        Assert.Equal("90000.00", items[0].GetProperty("total").GetString());
    }

    [Fact]
    public async Task UpcomingPayments_incluye_pagos_sin_period_id_y_excluye_los_no_pendientes()
    {
        await using var db = _factory.CreateDbContext();
        var period = new Periodo { Etiqueta = "E2E Panel 3", FechaInicio = new DateOnly(2026, 6, 1), FechaFin = new DateOnly(2026, 6, 15) };
        var category = new Categoria { Nombre = $"E2E-{Guid.NewGuid():N}" };
        db.Periodos.Add(period);
        db.Categorias.Add(category);
        await db.SaveChangesAsync();
        _periodIds.Add(period.Id);
        _categoryIds.Add(category.Id);

        var sinPeriodo = new PagoProximo { PeriodoId = null, CategoriaId = category.Id, Nombre = "E2E sin periodo", Monto = 1000m, FechaVencimiento = period.FechaInicio, Estado = "pending" };
        var pagado = new PagoProximo { PeriodoId = period.Id, CategoriaId = category.Id, Nombre = "E2E pagado", Monto = 2000m, FechaVencimiento = period.FechaInicio, Estado = "paid" };
        db.PagosProximos.AddRange(sinPeriodo, pagado);
        await db.SaveChangesAsync();
        _upcomingPaymentIds.Add(sinPeriodo.Id);
        _upcomingPaymentIds.Add(pagado.Id);

        var res = await _client.GetAsync($"/api/panel/{period.Id}");
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        var names = body.GetProperty("pagos_proximos").EnumerateArray()
            .Select(i => i.GetProperty("nombre").GetString()).ToList();

        Assert.Contains("E2E sin periodo", names);
        Assert.DoesNotContain("E2E pagado", names);
    }
}
