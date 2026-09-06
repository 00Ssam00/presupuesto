using Moq;
using Quincena.Application.Dashboard;
using Quincena.Domain.Entities;
using Xunit;

namespace Quincena.UnitTests.Dashboard;

// Port de backend/src/__tests__/dashboard.test.ts. El caso "los montos NUMERIC de
// Postgres (string) no producen NaN ni concatenación" no tiene equivalente acá: en
// Node guardaba contra la coerción implícita de JS al concatenar strings en vez de
// sumar; en C# las propiedades son decimal tipadas en compilación, esa clase de bug
// no puede existir.
public class DashboardServiceTests
{
    [Fact]
    public async Task GetAsync_retorna_null_si_el_periodo_no_existe()
    {
        var repo = new Mock<IDashboardRepository>();
        repo.Setup(r => r.GetPeriodAsync(It.IsAny<Guid>())).ReturnsAsync((Period?)null);
        var service = new DashboardService(repo.Object);

        var result = await service.GetAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task Calcula_available_y_remaining_a_partir_de_ingreso_base_rollover_y_gastos()
    {
        var periodId = Guid.NewGuid();
        var period = new Period { Id = periodId, BaseIncome = 1000000m, Rollover = 50000m };
        var repo = new Mock<IDashboardRepository>();
        repo.Setup(r => r.GetPeriodAsync(periodId)).ReturnsAsync(period);
        repo.Setup(r => r.GetTotalsAsync(periodId)).ReturnsAsync((300000m, 0m));
        repo.Setup(r => r.GetExpensesByCategoryAsync(periodId)).ReturnsAsync([]);
        repo.Setup(r => r.GetUpcomingPaymentsAsync(periodId)).ReturnsAsync([]);
        var service = new DashboardService(repo.Object);

        var result = await service.GetAsync(periodId);

        // available = base_income + rollover + total_income_extra
        Assert.Equal(1050000d, result!.Available);
        // remaining = available - total_expenses
        Assert.Equal(750000d, result.Remaining);
        Assert.Equal(300000d, result.TotalExpenses);
    }
}
