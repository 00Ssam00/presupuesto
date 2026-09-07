using Moq;
using Quincena.Application.Periodos;
using Quincena.Application.Arrastre;
using Quincena.Domain.Entidades;
using Xunit;

namespace Quincena.UnitTests.Periodos;

// Port de backend/src/__tests__/periods.test.ts (los casos que viven en la capa de
// servicio; la resolución de "período actual" por fecha vive en el repositorio y se
// prueba contra Postgres real en Quincena.IntegrationTests).
public class PeriodServiceTests
{
    [Fact]
    public async Task Create_aplica_defaults_de_base_income_y_rollover_a_cero_y_recalcula()
    {
        var repo = new Mock<IRepositorioPeriodos>();
        var rollover = new Mock<IServicioArrastre>();
        Periodo? stored = null;

        repo.Setup(r => r.AgregarAsync(It.IsAny<Periodo>()))
            .Callback<Periodo>(p => { p.Id = Guid.NewGuid(); stored = p; })
            .ReturnsAsync((Periodo p) => p);
        repo.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>())).ReturnsAsync(() => stored);

        var service = new ServicioPeriodos(repo.Object, rollover.Object);
        var request = new CrearPeriodoPeticion
        {
            Etiqueta = "Test",
            FechaInicio = new DateOnly(2026, 1, 1),
            FechaFin = new DateOnly(2026, 1, 15),
        };

        var result = await service.CrearAsync(request);

        Assert.Equal(0m, result.IngresoBase);
        Assert.Equal(0m, result.Arrastre);
        rollover.Verify(r => r.RecalcularTodoAsync(), Times.Once);
        repo.Verify(r => r.ObtenerPorIdAsync(stored!.Id), Times.Once);
    }

    [Fact]
    public async Task Create_conserva_base_income_y_rollover_explicitos()
    {
        var repo = new Mock<IRepositorioPeriodos>();
        var rollover = new Mock<IServicioArrastre>();
        Periodo? stored = null;
        repo.Setup(r => r.AgregarAsync(It.IsAny<Periodo>()))
            .Callback<Periodo>(p => { p.Id = Guid.NewGuid(); stored = p; })
            .ReturnsAsync((Periodo p) => p);
        repo.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>())).ReturnsAsync(() => stored);

        var service = new ServicioPeriodos(repo.Object, rollover.Object);
        var request = new CrearPeriodoPeticion
        {
            Etiqueta = "Test",
            FechaInicio = new DateOnly(2026, 1, 1),
            FechaFin = new DateOnly(2026, 1, 15),
            IngresoBase = 1500000m,
            Arrastre = 250000m,
        };

        var result = await service.CrearAsync(request);

        Assert.Equal(1500000m, result.IngresoBase);
        Assert.Equal(250000m, result.Arrastre);
    }

    [Fact]
    public async Task Update_retorna_null_sin_recalcular_si_no_existe()
    {
        var repo = new Mock<IRepositorioPeriodos>();
        repo.Setup(r => r.ActualizarAsync(It.IsAny<Guid>(), It.IsAny<Action<Periodo>>())).ReturnsAsync(false);
        var rollover = new Mock<IServicioArrastre>();
        var service = new ServicioPeriodos(repo.Object, rollover.Object);

        var result = await service.ActualizarAsync(Guid.NewGuid(), new ActualizarPeriodoPeticion
        {
            Etiqueta = "x",
            FechaInicio = new DateOnly(2026, 1, 1),
            FechaFin = new DateOnly(2026, 1, 15),
            IngresoBase = 0,
            Arrastre = 0,
            EstaCerrado = false,
        });

        Assert.Null(result);
        rollover.Verify(r => r.RecalcularTodoAsync(), Times.Never);
    }

    [Fact]
    public async Task Update_recalcula_y_relee_si_existe()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<IRepositorioPeriodos>();
        repo.Setup(r => r.ActualizarAsync(id, It.IsAny<Action<Periodo>>())).ReturnsAsync(true);
        var updated = new Periodo { Id = id, Etiqueta = "actualizado" };
        repo.Setup(r => r.ObtenerPorIdAsync(id)).ReturnsAsync(updated);
        var rollover = new Mock<IServicioArrastre>();
        var service = new ServicioPeriodos(repo.Object, rollover.Object);

        var result = await service.ActualizarAsync(id, new ActualizarPeriodoPeticion
        {
            Etiqueta = "actualizado",
            FechaInicio = new DateOnly(2026, 1, 1),
            FechaFin = new DateOnly(2026, 1, 15),
            IngresoBase = 0,
            Arrastre = 0,
            EstaCerrado = false,
        });

        Assert.Same(updated, result);
        rollover.Verify(r => r.RecalcularTodoAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_siempre_recalcula_sin_validar_existencia_previa()
    {
        var repo = new Mock<IRepositorioPeriodos>();
        repo.Setup(r => r.EliminarAsync(It.IsAny<Guid>())).ReturnsAsync(false);
        var rollover = new Mock<IServicioArrastre>();
        var service = new ServicioPeriodos(repo.Object, rollover.Object);

        var result = await service.EliminarAsync(Guid.NewGuid());

        Assert.True(result);
        rollover.Verify(r => r.RecalcularTodoAsync(), Times.Once);
    }
}
