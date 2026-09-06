using Moq;
using Quincena.Application.Budgets;
using Quincena.Domain.Entities;
using Xunit;

namespace Quincena.UnitTests.Budgets;

// BudgetService es un passthrough delgado (sin orquestación como el recálculo de
// rollover en Periods/Transactions) — la lógica real está en el LINQ del
// repositorio, probada con Postgres real en BudgetsControllerTests.
public class BudgetServiceTests
{
    [Fact]
    public async Task GetByPeriod_delega_en_el_repositorio()
    {
        var periodId = Guid.NewGuid();
        var expected = new List<BudgetByPeriodItem> { new() { CategoryId = Guid.NewGuid() } };
        var repo = new Mock<IBudgetRepository>();
        repo.Setup(r => r.GetByPeriodAsync(periodId)).ReturnsAsync(expected);
        var service = new BudgetService(repo.Object);

        var result = await service.GetByPeriodAsync(periodId);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task Upsert_delega_en_el_repositorio()
    {
        var request = new UpsertBudgetRequest { PeriodId = Guid.NewGuid(), CategoryId = Guid.NewGuid(), Amount = 100000m };
        var expected = new Budget { Id = Guid.NewGuid() };
        var repo = new Mock<IBudgetRepository>();
        repo.Setup(r => r.UpsertAsync(request)).ReturnsAsync(expected);
        var service = new BudgetService(repo.Object);

        var result = await service.UpsertAsync(request);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task Delete_delega_en_el_repositorio()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<IBudgetRepository>();
        var service = new BudgetService(repo.Object);

        await service.DeleteAsync(id);

        repo.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}
