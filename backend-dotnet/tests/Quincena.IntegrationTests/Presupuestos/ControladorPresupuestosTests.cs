using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quincena.Api.Serialization;
using Quincena.Application.Presupuestos;
using Quincena.Domain.Entidades;
using Quincena.IntegrationTests.Infrastructure;
using Xunit;

namespace Quincena.IntegrationTests.Presupuestos;

// Contra Postgres real — el LINQ de ObtenerPorPeriodoAsync (LEFT JOIN + subquery
// correlacionada) y el upsert por SQL crudo con ON CONFLICT solo se pueden
// validar de verdad contra Postgres, no con mocks.
public class BudgetsControllerTests : IClassFixture<QuincenaWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = QuincenaJsonOptions.Create();

    private readonly QuincenaWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly List<Guid> _budgetIds = [];
    private readonly List<Guid> _transactionIds = [];
    private readonly List<Guid> _periodIds = [];
    private readonly List<Guid> _categoryIds = [];
    private readonly List<Guid> _paymentMethodIds = [];

    public BudgetsControllerTests(QuincenaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var db = _factory.CreateDbContext();
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

    private async Task<(Periodo Periodo, Categoria CategoryConPresupuesto, Categoria CategoriaSinPresupuesto, MetodoPago Method)> SeedFixturesAsync()
    {
        await using var db = _factory.CreateDbContext();
        var period = new Periodo { Etiqueta = "E2E Presupuestos", FechaInicio = new DateOnly(2026, 6, 1), FechaFin = new DateOnly(2026, 6, 15), IngresoBase = 0 };
        var withBudget = new Categoria { Nombre = $"E2E-Con-{Guid.NewGuid():N}", Orden = 1 };
        var withoutBudget = new Categoria { Nombre = $"E2E-Sin-{Guid.NewGuid():N}", Orden = 2 };
        var method = new MetodoPago { Nombre = $"E2E-{Guid.NewGuid():N}" };
        db.Periodos.Add(period);
        db.Categorias.AddRange(withBudget, withoutBudget);
        db.MetodosPago.Add(method);
        await db.SaveChangesAsync();
        _periodIds.Add(period.Id);
        _categoryIds.Add(withBudget.Id);
        _categoryIds.Add(withoutBudget.Id);
        _paymentMethodIds.Add(method.Id);
        return (period, withBudget, withoutBudget, method);
    }

    [Fact]
    public async Task GetByPeriod_incluye_categorias_sin_presupuesto_con_amount_en_cero()
    {
        var (period, withBudget, withoutBudget, method) = await SeedFixturesAsync();

        var upsertRes = await _client.PostAsJsonAsync("/api/presupuestos", new GuardarPresupuestoPeticion
        {
            PeriodoId = period.Id, CategoriaId = withBudget.Id, Monto = 50000m,
        }, JsonOptions);
        upsertRes.EnsureSuccessStatusCode();
        var upserted = await upsertRes.Content.ReadFromJsonAsync<Presupuesto>(JsonOptions);
        _budgetIds.Add(upserted!.Id);

        var res = await _client.GetAsync($"/api/presupuestos/periodo/{period.Id}");
        res.EnsureSuccessStatusCode();
        var items = await res.Content.ReadFromJsonAsync<List<PresupuestoPorPeriodoItem>>(JsonOptions);

        var conPresupuesto = items!.Single(i => i.CategoriaId == withBudget.Id);
        Assert.NotNull(conPresupuesto.Id);
        Assert.Equal(50000m, conPresupuesto.Monto);

        var sinPresupuesto = items!.Single(i => i.CategoriaId == withoutBudget.Id);
        Assert.Null(sinPresupuesto.Id);
        Assert.Equal(0m, sinPresupuesto.Monto);
        Assert.Null(sinPresupuesto.Porcentaje);
    }

    [Fact]
    public async Task GetByPeriod_calcula_spent_solo_con_gastos_de_esa_categoria_y_quincena()
    {
        var (period, category, _, method) = await SeedFixturesAsync();
        await using (var db = _factory.CreateDbContext())
        {
            db.Transacciones.Add(new Transaccion
            {
                PeriodoId = period.Id, CategoriaId = category.Id, MetodoPagoId = method.Id,
                Tipo = "expense", Monto = 15000m, Descripcion = "E2E", Fecha = period.FechaInicio,
            });
            db.Transacciones.Add(new Transaccion
            {
                PeriodoId = period.Id, CategoriaId = category.Id, MetodoPagoId = method.Id,
                Tipo = "income", Monto = 999999m, Descripcion = "E2E ingreso, no debe sumar", Fecha = period.FechaInicio,
            });
            await db.SaveChangesAsync();
            _transactionIds.AddRange(db.Transacciones.Local.Select(t => t.Id));
        }

        var res = await _client.GetAsync($"/api/presupuestos/periodo/{period.Id}");
        var items = await res.Content.ReadFromJsonAsync<List<PresupuestoPorPeriodoItem>>(JsonOptions);

        var item = items!.Single(i => i.CategoriaId == category.Id);
        Assert.Equal(15000m, item.Gastado);
    }

    [Fact]
    public async Task Upsert_hace_on_conflict_do_update_en_vez_de_duplicar()
    {
        var (period, category, _, _) = await SeedFixturesAsync();

        var first = await _client.PostAsJsonAsync("/api/presupuestos", new GuardarPresupuestoPeticion
        {
            PeriodoId = period.Id, CategoriaId = category.Id, Monto = 10000m,
        }, JsonOptions);
        var firstBudget = await first.Content.ReadFromJsonAsync<Presupuesto>(JsonOptions);
        _budgetIds.Add(firstBudget!.Id);

        var second = await _client.PostAsJsonAsync("/api/presupuestos", new GuardarPresupuestoPeticion
        {
            PeriodoId = period.Id, CategoriaId = category.Id, Monto = 25000m, Porcentaje = 50m,
        }, JsonOptions);
        second.EnsureSuccessStatusCode();
        var secondBudget = await second.Content.ReadFromJsonAsync<Presupuesto>(JsonOptions);

        Assert.Equal(firstBudget.Id, secondBudget!.Id); // mismo registro, no un duplicado
        Assert.Equal(25000m, secondBudget.Monto);
        Assert.Equal(50m, secondBudget.Porcentaje);

        await using var db = _factory.CreateDbContext();
        var count = await db.Presupuestos.CountAsync(b => b.PeriodoId == period.Id && b.CategoriaId == category.Id);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Delete_responde_204_y_lo_elimina()
    {
        var (period, category, _, _) = await SeedFixturesAsync();
        var res = await _client.PostAsJsonAsync("/api/presupuestos", new GuardarPresupuestoPeticion
        {
            PeriodoId = period.Id, CategoriaId = category.Id, Monto = 10000m,
        }, JsonOptions);
        var budget = await res.Content.ReadFromJsonAsync<Presupuesto>(JsonOptions);

        var deleteRes = await _client.DeleteAsync($"/api/presupuestos/{budget!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        await using var db = _factory.CreateDbContext();
        Assert.Null(await db.Presupuestos.FirstOrDefaultAsync(b => b.Id == budget.Id));
    }
}
