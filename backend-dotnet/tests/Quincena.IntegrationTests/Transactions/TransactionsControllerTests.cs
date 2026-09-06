using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Quincena.Api.Controllers;
using Quincena.Api.Serialization;
using Quincena.Application.Cards;
using Quincena.Application.Categories;
using Quincena.Application.PaymentMethods;
using Quincena.Application.Periods;
using Quincena.Application.Transactions;
using Quincena.Domain.Entities;
using Quincena.IntegrationTests.Infrastructure;
using Xunit;

namespace Quincena.IntegrationTests.Transactions;

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
            db.Transactions.RemoveRange(db.Transactions.Where(t => _transactionIds.Contains(t.Id)));
        if (_periodIds.Count > 0)
            db.Periods.RemoveRange(db.Periods.Where(p => _periodIds.Contains(p.Id)));
        if (_categoryIds.Count > 0)
            db.Categories.RemoveRange(db.Categories.Where(c => _categoryIds.Contains(c.Id)));
        if (_paymentMethodIds.Count > 0)
            db.PaymentMethods.RemoveRange(db.PaymentMethods.Where(m => _paymentMethodIds.Contains(m.Id)));
        await db.SaveChangesAsync();
    }

    private async Task<(Period Period, Category Category, PaymentMethod Method)> SeedFixturesAsync()
    {
        await using var db = _factory.CreateDbContext();
        var period = new Period { Label = "E2E", StartDate = new DateOnly(2026, 6, 1), EndDate = new DateOnly(2026, 6, 15), BaseIncome = 0 };
        var category = new Category { Name = $"E2E-{Guid.NewGuid():N}" };
        var method = new PaymentMethod { Name = $"E2E-{Guid.NewGuid():N}", RequiresCard = false };
        db.Periods.Add(period);
        db.Categories.Add(category);
        db.PaymentMethods.Add(method);
        await db.SaveChangesAsync();
        _periodIds.Add(period.Id);
        _categoryIds.Add(category.Id);
        _paymentMethodIds.Add(method.Id);
        return (period, category, method);
    }

    private async Task<Transaction> CreateTransactionAsync(Period period, Category category, PaymentMethod method, decimal amount = 1000m)
    {
        var res = await _client.PostAsJsonAsync("/api/transactions", new CreateTransactionRequest
        {
            PeriodId = period.Id, CategoryId = category.Id, PaymentMethodId = method.Id,
            Type = "expense", Amount = amount, Description = "E2E", Date = period.StartDate,
        }, JsonOptions);
        res.EnsureSuccessStatusCode();
        var transaction = (await res.Content.ReadFromJsonAsync<Transaction>(JsonOptions))!;
        _transactionIds.Add(transaction.Id);
        return transaction;
    }

    [Fact]
    public async Task Create_no_incluye_los_campos_de_join_a_diferencia_de_List()
    {
        var (period, category, method) = await SeedFixturesAsync();

        var res = await _client.PostAsJsonAsync("/api/transactions", new CreateTransactionRequest
        {
            PeriodId = period.Id, CategoryId = category.Id, PaymentMethodId = method.Id,
            Type = "expense", Amount = 1000m, Description = "E2E", Date = period.StartDate,
        }, JsonOptions);
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        _transactionIds.Add(body.GetProperty("id").GetGuid());

        Assert.False(body.TryGetProperty("category_name", out _));
    }

    [Fact]
    public async Task List_incluye_los_nombres_via_join_y_filtra_por_period_id_type_y_category_id()
    {
        var (period, category, method) = await SeedFixturesAsync();
        var transaction = await CreateTransactionAsync(period, category, method);

        var res = await _client.GetAsync($"/api/transactions?period_id={period.Id}&type=expense&category_id={category.Id}");
        res.EnsureSuccessStatusCode();
        var list = await res.Content.ReadFromJsonAsync<List<TransactionListItem>>(JsonOptions);

        var found = Assert.Single(list!, t => t.Id == transaction.Id);
        Assert.Equal(category.Name, found.CategoryName);
        Assert.Equal(method.Name, found.PaymentMethodName);
        Assert.Null(found.CardName);
    }

    [Fact]
    public async Task GetById_responde_404_con_el_shape_de_error_de_Node()
    {
        var res = await _client.GetAsync($"/api/transactions/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("No encontrada", body.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Update_no_permite_cambiar_period_id_y_recalcula()
    {
        var (period, category, method) = await SeedFixturesAsync();
        var transaction = await CreateTransactionAsync(period, category, method, amount: 500m);

        var res = await _client.PutAsJsonAsync($"/api/transactions/{transaction.Id}", new UpdateTransactionRequest
        {
            CategoryId = category.Id, PaymentMethodId = method.Id,
            Type = "expense", Amount = 900m, Description = "Editada", Date = period.StartDate,
        }, JsonOptions);
        res.EnsureSuccessStatusCode();
        var updated = await res.Content.ReadFromJsonAsync<Transaction>(JsonOptions);

        Assert.Equal(period.Id, updated!.PeriodId);
        Assert.Equal(900m, updated.Amount);
    }

    [Fact]
    public async Task Delete_responde_204_y_la_transaccion_desaparece()
    {
        var (period, category, method) = await SeedFixturesAsync();
        var transaction = await CreateTransactionAsync(period, category, method);

        var deleteRes = await _client.DeleteAsync($"/api/transactions/{transaction.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        var getRes = await _client.GetAsync($"/api/transactions/{transaction.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getRes.StatusCode);
        _transactionIds.Remove(transaction.Id);
    }

    [Fact]
    public async Task Create_con_categoria_inexistente_responde_500_por_violacion_de_FK()
    {
        var (period, _, method) = await SeedFixturesAsync();

        var res = await _client.PostAsJsonAsync("/api/transactions", new CreateTransactionRequest
        {
            PeriodId = period.Id, CategoryId = Guid.NewGuid(), PaymentMethodId = method.Id,
            Type = "expense", Amount = 1000m, Description = "E2E", Date = period.StartDate,
        }, JsonOptions);

        Assert.Equal(HttpStatusCode.InternalServerError, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.TryGetProperty("error", out var error));
        Assert.False(string.IsNullOrEmpty(error.GetString())); // Development: mensaje real, no genérico
    }

    [Fact]
    public async Task Import_sin_rows_responde_400()
    {
        var res = await _client.PostAsJsonAsync("/api/transactions/import", new { }, JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Import_con_rows_vacio_responde_400()
    {
        var res = await _client.PostAsJsonAsync("/api/transactions/import", new ImportRequest { Rows = [] }, JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Import_de_una_fila_valida_crea_la_transaccion_y_resuelve_la_quincena_por_fecha()
    {
        var (period, category, method) = await SeedFixturesAsync();

        var res = await _client.PostAsJsonAsync("/api/transactions/import", new ImportRequest
        {
            Rows =
            [
                new ImportRow { Monto = "85000", Tipo = "expense", Fecha = period.StartDate.ToString("yyyy-MM-dd"), Categoria = category.Name, Metodo = method.Name },
            ],
        }, JsonOptions);
        res.EnsureSuccessStatusCode();
        var result = await res.Content.ReadFromJsonAsync<ImportResult>(JsonOptions);

        Assert.Equal(1, result!.Created);
        Assert.Equal(0, result.Failed);
        _transactionIds.Add(result.Results[0].Id!.Value);
    }
}
