using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quincena.Api.Serialization;
using Quincena.Application.Budgets;
using Quincena.Domain.Entities;
using Quincena.IntegrationTests.Infrastructure;
using Xunit;

namespace Quincena.IntegrationTests.Budgets;

// Contra Postgres real — el LINQ de GetByPeriodAsync (LEFT JOIN + subquery
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

    private async Task<(Period Period, Category CategoryConPresupuesto, Category CategoriaSinPresupuesto, PaymentMethod Method)> SeedFixturesAsync()
    {
        await using var db = _factory.CreateDbContext();
        var period = new Period { Label = "E2E Budgets", StartDate = new DateOnly(2026, 6, 1), EndDate = new DateOnly(2026, 6, 15), BaseIncome = 0 };
        var withBudget = new Category { Name = $"E2E-Con-{Guid.NewGuid():N}", SortOrder = 1 };
        var withoutBudget = new Category { Name = $"E2E-Sin-{Guid.NewGuid():N}", SortOrder = 2 };
        var method = new PaymentMethod { Name = $"E2E-{Guid.NewGuid():N}" };
        db.Periods.Add(period);
        db.Categories.AddRange(withBudget, withoutBudget);
        db.PaymentMethods.Add(method);
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

        var upsertRes = await _client.PostAsJsonAsync("/api/budgets", new UpsertBudgetRequest
        {
            PeriodId = period.Id, CategoryId = withBudget.Id, Amount = 50000m,
        }, JsonOptions);
        upsertRes.EnsureSuccessStatusCode();
        var upserted = await upsertRes.Content.ReadFromJsonAsync<Budget>(JsonOptions);
        _budgetIds.Add(upserted!.Id);

        var res = await _client.GetAsync($"/api/budgets/period/{period.Id}");
        res.EnsureSuccessStatusCode();
        var items = await res.Content.ReadFromJsonAsync<List<BudgetByPeriodItem>>(JsonOptions);

        var conPresupuesto = items!.Single(i => i.CategoryId == withBudget.Id);
        Assert.NotNull(conPresupuesto.Id);
        Assert.Equal(50000m, conPresupuesto.Amount);

        var sinPresupuesto = items!.Single(i => i.CategoryId == withoutBudget.Id);
        Assert.Null(sinPresupuesto.Id);
        Assert.Equal(0m, sinPresupuesto.Amount);
        Assert.Null(sinPresupuesto.Percentage);
    }

    [Fact]
    public async Task GetByPeriod_calcula_spent_solo_con_gastos_de_esa_categoria_y_quincena()
    {
        var (period, category, _, method) = await SeedFixturesAsync();
        await using (var db = _factory.CreateDbContext())
        {
            db.Transactions.Add(new Transaction
            {
                PeriodId = period.Id, CategoryId = category.Id, PaymentMethodId = method.Id,
                Type = "expense", Amount = 15000m, Description = "E2E", Date = period.StartDate,
            });
            db.Transactions.Add(new Transaction
            {
                PeriodId = period.Id, CategoryId = category.Id, PaymentMethodId = method.Id,
                Type = "income", Amount = 999999m, Description = "E2E ingreso, no debe sumar", Date = period.StartDate,
            });
            await db.SaveChangesAsync();
            _transactionIds.AddRange(db.Transactions.Local.Select(t => t.Id));
        }

        var res = await _client.GetAsync($"/api/budgets/period/{period.Id}");
        var items = await res.Content.ReadFromJsonAsync<List<BudgetByPeriodItem>>(JsonOptions);

        var item = items!.Single(i => i.CategoryId == category.Id);
        Assert.Equal(15000m, item.Spent);
    }

    [Fact]
    public async Task Upsert_hace_on_conflict_do_update_en_vez_de_duplicar()
    {
        var (period, category, _, _) = await SeedFixturesAsync();

        var first = await _client.PostAsJsonAsync("/api/budgets", new UpsertBudgetRequest
        {
            PeriodId = period.Id, CategoryId = category.Id, Amount = 10000m,
        }, JsonOptions);
        var firstBudget = await first.Content.ReadFromJsonAsync<Budget>(JsonOptions);
        _budgetIds.Add(firstBudget!.Id);

        var second = await _client.PostAsJsonAsync("/api/budgets", new UpsertBudgetRequest
        {
            PeriodId = period.Id, CategoryId = category.Id, Amount = 25000m, Percentage = 50m,
        }, JsonOptions);
        second.EnsureSuccessStatusCode();
        var secondBudget = await second.Content.ReadFromJsonAsync<Budget>(JsonOptions);

        Assert.Equal(firstBudget.Id, secondBudget!.Id); // mismo registro, no un duplicado
        Assert.Equal(25000m, secondBudget.Amount);
        Assert.Equal(50m, secondBudget.Percentage);

        await using var db = _factory.CreateDbContext();
        var count = await db.Budgets.CountAsync(b => b.PeriodId == period.Id && b.CategoryId == category.Id);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Delete_responde_204_y_lo_elimina()
    {
        var (period, category, _, _) = await SeedFixturesAsync();
        var res = await _client.PostAsJsonAsync("/api/budgets", new UpsertBudgetRequest
        {
            PeriodId = period.Id, CategoryId = category.Id, Amount = 10000m,
        }, JsonOptions);
        var budget = await res.Content.ReadFromJsonAsync<Budget>(JsonOptions);

        var deleteRes = await _client.DeleteAsync($"/api/budgets/{budget!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        await using var db = _factory.CreateDbContext();
        Assert.Null(await db.Budgets.FirstOrDefaultAsync(b => b.Id == budget.Id));
    }
}
