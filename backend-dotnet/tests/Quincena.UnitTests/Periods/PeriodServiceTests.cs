using Moq;
using Quincena.Application.Periods;
using Quincena.Application.Rollover;
using Quincena.Domain.Entities;
using Xunit;

namespace Quincena.UnitTests.Periods;

// Port de backend/src/__tests__/periods.test.ts (los casos que viven en la capa de
// servicio; la resolución de "período actual" por fecha vive en el repositorio y se
// prueba contra Postgres real en Quincena.IntegrationTests).
public class PeriodServiceTests
{
    [Fact]
    public async Task Create_aplica_defaults_de_base_income_y_rollover_a_cero_y_recalcula()
    {
        var repo = new Mock<IPeriodRepository>();
        var rollover = new Mock<IRolloverService>();
        Period? stored = null;

        repo.Setup(r => r.AddAsync(It.IsAny<Period>()))
            .Callback<Period>(p => { p.Id = Guid.NewGuid(); stored = p; })
            .ReturnsAsync((Period p) => p);
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(() => stored);

        var service = new PeriodService(repo.Object, rollover.Object);
        var request = new CreatePeriodRequest
        {
            Label = "Test",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 1, 15),
        };

        var result = await service.CreateAsync(request);

        Assert.Equal(0m, result.BaseIncome);
        Assert.Equal(0m, result.Rollover);
        rollover.Verify(r => r.RecalculateAllAsync(), Times.Once);
        repo.Verify(r => r.GetByIdAsync(stored!.Id), Times.Once);
    }

    [Fact]
    public async Task Create_conserva_base_income_y_rollover_explicitos()
    {
        var repo = new Mock<IPeriodRepository>();
        var rollover = new Mock<IRolloverService>();
        Period? stored = null;
        repo.Setup(r => r.AddAsync(It.IsAny<Period>()))
            .Callback<Period>(p => { p.Id = Guid.NewGuid(); stored = p; })
            .ReturnsAsync((Period p) => p);
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(() => stored);

        var service = new PeriodService(repo.Object, rollover.Object);
        var request = new CreatePeriodRequest
        {
            Label = "Test",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 1, 15),
            BaseIncome = 1500000m,
            Rollover = 250000m,
        };

        var result = await service.CreateAsync(request);

        Assert.Equal(1500000m, result.BaseIncome);
        Assert.Equal(250000m, result.Rollover);
    }

    [Fact]
    public async Task Update_retorna_null_sin_recalcular_si_no_existe()
    {
        var repo = new Mock<IPeriodRepository>();
        repo.Setup(r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Action<Period>>())).ReturnsAsync(false);
        var rollover = new Mock<IRolloverService>();
        var service = new PeriodService(repo.Object, rollover.Object);

        var result = await service.UpdateAsync(Guid.NewGuid(), new UpdatePeriodRequest
        {
            Label = "x",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 1, 15),
            BaseIncome = 0,
            Rollover = 0,
            IsClosed = false,
        });

        Assert.Null(result);
        rollover.Verify(r => r.RecalculateAllAsync(), Times.Never);
    }

    [Fact]
    public async Task Update_recalcula_y_relee_si_existe()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<IPeriodRepository>();
        repo.Setup(r => r.UpdateAsync(id, It.IsAny<Action<Period>>())).ReturnsAsync(true);
        var updated = new Period { Id = id, Label = "actualizado" };
        repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(updated);
        var rollover = new Mock<IRolloverService>();
        var service = new PeriodService(repo.Object, rollover.Object);

        var result = await service.UpdateAsync(id, new UpdatePeriodRequest
        {
            Label = "actualizado",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 1, 15),
            BaseIncome = 0,
            Rollover = 0,
            IsClosed = false,
        });

        Assert.Same(updated, result);
        rollover.Verify(r => r.RecalculateAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_siempre_recalcula_sin_validar_existencia_previa()
    {
        var repo = new Mock<IPeriodRepository>();
        repo.Setup(r => r.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(false);
        var rollover = new Mock<IRolloverService>();
        var service = new PeriodService(repo.Object, rollover.Object);

        var result = await service.DeleteAsync(Guid.NewGuid());

        Assert.True(result);
        rollover.Verify(r => r.RecalculateAllAsync(), Times.Once);
    }
}
