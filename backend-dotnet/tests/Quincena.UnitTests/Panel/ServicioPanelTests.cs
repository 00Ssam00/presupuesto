using Moq;
using Quincena.Application.Panel;
using Quincena.Domain.Entidades;
using Xunit;

namespace Quincena.UnitTests.Panel;

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
        var repo = new Mock<IRepositorioPanel>();
        repo.Setup(r => r.ObtenerPeriodoAsync(It.IsAny<Guid>())).ReturnsAsync((Periodo?)null);
        var service = new ServicioPanel(repo.Object);

        var result = await service.ObtenerPanelAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task Calcula_available_y_remaining_a_partir_de_ingreso_base_rollover_y_gastos()
    {
        var periodId = Guid.NewGuid();
        var period = new Periodo { Id = periodId, IngresoBase = 1000000m, Arrastre = 50000m };
        var repo = new Mock<IRepositorioPanel>();
        repo.Setup(r => r.ObtenerPeriodoAsync(periodId)).ReturnsAsync(period);
        repo.Setup(r => r.ObtenerTotalesAsync(periodId)).ReturnsAsync((300000m, 0m));
        repo.Setup(r => r.ObtenerGastosPorCategoriaAsync(periodId)).ReturnsAsync([]);
        repo.Setup(r => r.ObtenerPagosProximosAsync(periodId)).ReturnsAsync([]);
        var service = new ServicioPanel(repo.Object);

        var result = await service.ObtenerPanelAsync(periodId);

        // available = base_income + rollover + total_income_extra
        Assert.Equal(1050000d, result!.Disponible);
        // remaining = available - total_expenses
        Assert.Equal(750000d, result.Restante);
        Assert.Equal(300000d, result.TotalGastos);
    }
}
