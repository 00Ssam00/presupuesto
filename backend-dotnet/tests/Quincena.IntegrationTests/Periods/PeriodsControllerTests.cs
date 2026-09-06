using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quincena.Api.Serialization;
using Quincena.Application.Periods;
using Quincena.Domain.Entities;
using Quincena.IntegrationTests.Infrastructure;
using Xunit;

namespace Quincena.IntegrationTests.Periods;

// Contra Postgres real (quincena_dotnet_dev) — cubre lo que las pruebas unitarias
// mockeadas no pueden: la resolución de "período actual" por fecha (vive en el
// repositorio, con LINQ real traducido a SQL), la prioridad de la ruta literal
// "current" sobre la parametrizada "{id}", y el recálculo de rollover de punta a
// punta con una transacción real (valida que RolloverDataSource traduce correctamente
// a SQL contra Postgres, no solo en teoría).
public class PeriodsControllerTests : IClassFixture<QuincenaWebApplicationFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = QuincenaJsonOptions.Create();

    private readonly QuincenaWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly List<Guid> _periodIds = [];
    private readonly List<Guid> _transactionIds = [];
    private readonly List<Guid> _categoryIds = [];
    private readonly List<Guid> _paymentMethodIds = [];

    public PeriodsControllerTests(QuincenaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var db = _factory.CreateDbContext();
        if (_transactionIds.Count > 0)
            db.Transactions.RemoveRange(db.Transactions.Where(t => _transactionIds.Contains(t.Id)));
        if (_periodIds.Count > 0)
            db.Periods.RemoveRange(db.Periods.Where(p => _periodIds.Contains(p.Id)));
        if (_categoryIds.Count > 0)
            db.Categories.RemoveRange(db.Categories.Where(c => _categoryIds.Contains(c.Id)));
        if (_paymentMethodIds.Count > 0)
            db.PaymentMethods.RemoveRange(db.PaymentMethods.Where(m => _paymentMethodIds.Contains(m.Id)));
        await db.SaveChangesAsync();
    }

    private async Task<Period> CreatePeriodAsync(string label, DateOnly start, DateOnly end, decimal baseIncome = 0)
    {
        var request = new CreatePeriodRequest { Label = label, StartDate = start, EndDate = end, BaseIncome = baseIncome };
        var res = await _client.PostAsJsonAsync("/api/periods", request, JsonOptions);
        res.EnsureSuccessStatusCode();
        var period = (await res.Content.ReadFromJsonAsync<Period>(JsonOptions))!;
        _periodIds.Add(period.Id);
        return period;
    }

    private async Task AddExpenseAsync(Guid periodId, decimal amount)
    {
        await using var db = _factory.CreateDbContext();
        var category = new Category { Name = $"Cat-{Guid.NewGuid():N}" };
        var method = new PaymentMethod { Name = $"Metodo-{Guid.NewGuid():N}" };
        db.Categories.Add(category);
        db.PaymentMethods.Add(method);
        await db.SaveChangesAsync();
        _categoryIds.Add(category.Id);
        _paymentMethodIds.Add(method.Id);

        var transaction = new Transaction
        {
            PeriodId = periodId,
            CategoryId = category.Id,
            PaymentMethodId = method.Id,
            Type = "expense",
            Amount = amount,
            Description = "Gasto de prueba de integración",
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
        };
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();
        _transactionIds.Add(transaction.Id);
    }

    [Fact]
    public async Task Current_devuelve_null_cuando_no_hay_ningun_periodo()
    {
        var res = await _client.GetAsync("/api/periods/current");

        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadAsStringAsync();
        Assert.Equal("null", body);
    }

    [Fact]
    public async Task Current_no_es_interceptada_por_la_ruta_parametrizada_y_devuelve_el_periodo_que_cubre_hoy()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var period = await CreatePeriodAsync("Cubre hoy", today.AddDays(-2), today.AddDays(2));

        var res = await _client.GetAsync("/api/periods/current");

        res.EnsureSuccessStatusCode();
        var current = await res.Content.ReadFromJsonAsync<Period>(JsonOptions);
        Assert.Equal(period.Id, current!.Id);
    }

    [Fact]
    public async Task Current_cae_al_mas_reciente_si_ninguno_cubre_hoy()
    {
        var farFuture = new DateOnly(2094, 1, 1);
        var older = await CreatePeriodAsync("Vieja", farFuture, farFuture.AddDays(14));
        var newer = await CreatePeriodAsync("Nueva", farFuture.AddDays(15), farFuture.AddDays(29));

        var res = await _client.GetAsync("/api/periods/current");

        res.EnsureSuccessStatusCode();
        var current = await res.Content.ReadFromJsonAsync<Period>(JsonOptions);
        Assert.Equal(newer.Id, current!.Id);
        Assert.NotEqual(older.Id, current.Id);
    }

    [Fact]
    public async Task GetById_responde_404_con_el_shape_de_error_de_Node()
    {
        var res = await _client.GetAsync($"/api/periods/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("No encontrado", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Crear_dos_periodos_consecutivos_sin_transacciones_deja_ambos_en_rollover_cero()
    {
        var start = new DateOnly(2093, 1, 1);
        var a = await CreatePeriodAsync("A", start, start.AddDays(14));
        var b = await CreatePeriodAsync("B", start.AddDays(15), start.AddDays(29));

        Assert.Equal(0m, a.Rollover);
        Assert.Equal(0m, b.Rollover);
    }

    [Fact]
    public async Task El_gasto_de_un_periodo_se_traslada_como_rollover_al_siguiente()
    {
        var start = new DateOnly(2092, 1, 1);
        var a = await CreatePeriodAsync("A", start, start.AddDays(14), baseIncome: 1000m);
        var b = await CreatePeriodAsync("B", start.AddDays(15), start.AddDays(29));

        await AddExpenseAsync(a.Id, 400m);

        // Cualquier mutación de período recalcula todos los rollovers; forzamos una vía
        // PUT sin cambios reales sobre A para disparar el recálculo tras insertar el gasto
        // directo por DbContext (que no pasa por la API todavía — Transactions es la Fase 4).
        var updateRes = await _client.PutAsJsonAsync($"/api/periods/{a.Id}", new UpdatePeriodRequest
        {
            Label = "A",
            StartDate = a.StartDate,
            EndDate = a.EndDate,
            BaseIncome = a.BaseIncome,
            Rollover = a.Rollover,
            IsClosed = false,
        }, JsonOptions);
        updateRes.EnsureSuccessStatusCode();

        var bRes = await _client.GetAsync($"/api/periods/{b.Id}");
        var bFresh = await bRes.Content.ReadFromJsonAsync<Period>(JsonOptions);

        Assert.Equal(600m, bFresh!.Rollover); // 1000 (base_income) + 0 (rollover A) - 400 (gasto) = 600
    }

    [Fact]
    public async Task Delete_elimina_el_periodo_y_recalcula()
    {
        var start = new DateOnly(2091, 1, 1);
        var period = await CreatePeriodAsync("Para borrar", start, start.AddDays(14));

        var deleteRes = await _client.DeleteAsync($"/api/periods/{period.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        var getRes = await _client.GetAsync($"/api/periods/{period.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getRes.StatusCode);

        _periodIds.Remove(period.Id); // ya no existe, evita doble-delete en DisposeAsync
    }
}
