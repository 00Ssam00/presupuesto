using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Quincena.Api.Serialization;
using Quincena.Domain.Entities;
using Quincena.IntegrationTests.Infrastructure;
using Xunit;

namespace Quincena.IntegrationTests.Dashboard;

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
            db.UpcomingPayments.RemoveRange(db.UpcomingPayments.Where(u => _upcomingPaymentIds.Contains(u.Id)));
        if (_budgetIds.Count > 0)
            db.Budgets.RemoveRange(db.Budgets.Where(b => _budgetIds.Contains(b.Id)));
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

    [Fact]
    public async Task GetById_inexistente_responde_404_con_mensaje_propio()
    {
        var res = await _client.GetAsync($"/api/dashboard/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Período no encontrado", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Available_total_expenses_y_remaining_salen_como_numero_json_plano()
    {
        await using var db = _factory.CreateDbContext();
        var period = new Period { Label = "E2E Dashboard", StartDate = new DateOnly(2026, 6, 1), EndDate = new DateOnly(2026, 6, 15), BaseIncome = 1000000m, Rollover = 50000m };
        var category = new Category { Name = $"E2E-{Guid.NewGuid():N}" };
        var method = new PaymentMethod { Name = $"E2E-{Guid.NewGuid():N}" };
        db.Periods.Add(period);
        db.Categories.Add(category);
        db.PaymentMethods.Add(method);
        await db.SaveChangesAsync();
        _periodIds.Add(period.Id);
        _categoryIds.Add(category.Id);
        _paymentMethodIds.Add(method.Id);

        var transaction = new Transaction
        {
            PeriodId = period.Id, CategoryId = category.Id, PaymentMethodId = method.Id,
            Type = "expense", Amount = 300000m, Description = "E2E", Date = period.StartDate,
        };
        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();
        _transactionIds.Add(transaction.Id);

        var res = await _client.GetAsync($"/api/dashboard/{period.Id}");
        res.EnsureSuccessStatusCode();
        var raw = await res.Content.ReadAsStringAsync();
        var body = JsonDocument.Parse(raw).RootElement;

        // ValueKind.Number confirma que NO están entre comillas (a diferencia de
        // period.base_income, que sí debe seguir siendo string).
        Assert.Equal(JsonValueKind.Number, body.GetProperty("available").ValueKind);
        Assert.Equal(JsonValueKind.Number, body.GetProperty("total_expenses").ValueKind);
        Assert.Equal(JsonValueKind.Number, body.GetProperty("remaining").ValueKind);
        Assert.Equal(JsonValueKind.String, body.GetProperty("period").GetProperty("base_income").ValueKind);

        Assert.Equal(1050000d, body.GetProperty("available").GetDouble());
        Assert.Equal(750000d, body.GetProperty("remaining").GetDouble());
    }

    [Fact]
    public async Task ExpensesByCategory_incluye_todas_las_categorias_activas_ordenadas_por_total_desc()
    {
        await using var db = _factory.CreateDbContext();
        var period = new Period { Label = "E2E Dashboard 2", StartDate = new DateOnly(2026, 6, 1), EndDate = new DateOnly(2026, 6, 15) };
        var catAlta = new Category { Name = $"E2E-Alta-{Guid.NewGuid():N}" };
        var catBaja = new Category { Name = $"E2E-Baja-{Guid.NewGuid():N}" };
        var method = new PaymentMethod { Name = $"E2E-{Guid.NewGuid():N}" };
        db.Periods.Add(period);
        db.Categories.AddRange(catAlta, catBaja);
        db.PaymentMethods.Add(method);
        await db.SaveChangesAsync();
        _periodIds.Add(period.Id);
        _categoryIds.Add(catAlta.Id);
        _categoryIds.Add(catBaja.Id);
        _paymentMethodIds.Add(method.Id);

        db.Transactions.Add(new Transaction { PeriodId = period.Id, CategoryId = catAlta.Id, PaymentMethodId = method.Id, Type = "expense", Amount = 90000m, Description = "E2E", Date = period.StartDate });
        db.Transactions.Add(new Transaction { PeriodId = period.Id, CategoryId = catBaja.Id, PaymentMethodId = method.Id, Type = "expense", Amount = 10000m, Description = "E2E", Date = period.StartDate });
        await db.SaveChangesAsync();
        _transactionIds.AddRange(db.Transactions.Local.Select(t => t.Id));

        var res = await _client.GetAsync($"/api/dashboard/{period.Id}");
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        var items = body.GetProperty("expenses_by_category").EnumerateArray()
            .Where(i => i.GetProperty("category_id").GetGuid() == catAlta.Id || i.GetProperty("category_id").GetGuid() == catBaja.Id)
            .ToList();

        Assert.Equal(2, items.Count);
        Assert.Equal(catAlta.Id, items[0].GetProperty("category_id").GetGuid()); // mayor total primero
        Assert.Equal("90000.00", items[0].GetProperty("total").GetString());
    }

    [Fact]
    public async Task UpcomingPayments_incluye_pagos_sin_period_id_y_excluye_los_no_pendientes()
    {
        await using var db = _factory.CreateDbContext();
        var period = new Period { Label = "E2E Dashboard 3", StartDate = new DateOnly(2026, 6, 1), EndDate = new DateOnly(2026, 6, 15) };
        var category = new Category { Name = $"E2E-{Guid.NewGuid():N}" };
        db.Periods.Add(period);
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        _periodIds.Add(period.Id);
        _categoryIds.Add(category.Id);

        var sinPeriodo = new UpcomingPayment { PeriodId = null, CategoryId = category.Id, Name = "E2E sin periodo", Amount = 1000m, DueDate = period.StartDate, Status = "pending" };
        var pagado = new UpcomingPayment { PeriodId = period.Id, CategoryId = category.Id, Name = "E2E pagado", Amount = 2000m, DueDate = period.StartDate, Status = "paid" };
        db.UpcomingPayments.AddRange(sinPeriodo, pagado);
        await db.SaveChangesAsync();
        _upcomingPaymentIds.Add(sinPeriodo.Id);
        _upcomingPaymentIds.Add(pagado.Id);

        var res = await _client.GetAsync($"/api/dashboard/{period.Id}");
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        var names = body.GetProperty("upcoming_payments").EnumerateArray()
            .Select(i => i.GetProperty("name").GetString()).ToList();

        Assert.Contains("E2E sin periodo", names);
        Assert.DoesNotContain("E2E pagado", names);
    }
}
