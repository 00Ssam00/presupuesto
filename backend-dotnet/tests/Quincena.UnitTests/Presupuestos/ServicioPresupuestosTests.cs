using Moq;
using Quincena.Application.Presupuestos;
using Quincena.Domain.Entidades;
using Xunit;

namespace Quincena.UnitTests.Presupuestos;

// ServicioPresupuestos es un passthrough delgado (sin orquestación como el recálculo de
// rollover en Periodos/Transacciones) — la lógica real está en el LINQ del
// repositorio, probada con Postgres real en BudgetsControllerTests.
public class BudgetServiceTests
{
    [Fact]
    public async Task GetByPeriod_delega_en_el_repositorio()
    {
        var periodId = Guid.NewGuid();
        var expected = new List<PresupuestoPorPeriodoItem> { new() { CategoriaId = Guid.NewGuid() } };
        var repo = new Mock<IRepositorioPresupuestos>();
        repo.Setup(r => r.ObtenerPorPeriodoAsync(periodId)).ReturnsAsync(expected);
        var service = new ServicioPresupuestos(repo.Object);

        var result = await service.ObtenerPorPeriodoAsync(periodId);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task Upsert_delega_en_el_repositorio()
    {
        var request = new GuardarPresupuestoPeticion { PeriodoId = Guid.NewGuid(), CategoriaId = Guid.NewGuid(), Monto = 100000m };
        var expected = new Presupuesto { Id = Guid.NewGuid() };
        var repo = new Mock<IRepositorioPresupuestos>();
        repo.Setup(r => r.GuardarAsync(request)).ReturnsAsync(expected);
        var service = new ServicioPresupuestos(repo.Object);

        var result = await service.GuardarAsync(request);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task Delete_delega_en_el_repositorio()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<IRepositorioPresupuestos>();
        var service = new ServicioPresupuestos(repo.Object);

        await service.EliminarAsync(id);

        repo.Verify(r => r.EliminarAsync(id), Times.Once);
    }
}
