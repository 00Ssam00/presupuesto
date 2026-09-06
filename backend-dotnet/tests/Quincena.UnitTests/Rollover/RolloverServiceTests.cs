using Moq;
using Quincena.Application.Rollover;
using Xunit;

namespace Quincena.UnitTests.Rollover;

// Port de backend/src/services/rollover.test.ts — mismos 4 casos, más un caso de borde
// de redondeo específico de la migración (ver comentario en RolloverService).
public class RolloverServiceTests
{
    private static PeriodRolloverRow Row(Guid id, DateOnly start, decimal baseIncome, decimal rollover, decimal expenses, decimal incomeExtra) =>
        new(id, start, baseIncome, rollover, expenses, incomeExtra);

    private static (Mock<IRolloverDataSource> mock, Func<List<RolloverUpdate>?> captured) MockDataSource(List<PeriodRolloverRow> rows)
    {
        var mock = new Mock<IRolloverDataSource>();
        mock.Setup(d => d.GetPeriodTotalsAsync()).ReturnsAsync(rows);
        List<RolloverUpdate>? captured = null;
        mock.Setup(d => d.UpdateRolloversAsync(It.IsAny<IReadOnlyList<RolloverUpdate>>()))
            .Callback<IReadOnlyList<RolloverUpdate>>(u => captured = u.ToList())
            .Returns(Task.CompletedTask);
        return (mock, () => captured);
    }

    [Fact]
    public async Task No_actualiza_nada_si_los_rollovers_ya_estan_correctos()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var (mock, _) = MockDataSource(new List<PeriodRolloverRow>
        {
            Row(a, new DateOnly(2026, 6, 1), 1000m, 0m, 400m, 0m),   // primer período: esperado siempre 0, coincide
            Row(b, new DateOnly(2026, 6, 16), 1000m, 600m, 0m, 0m),  // carry de A = 1000+0+0-400 = 600, coincide
        });

        await new RolloverService(mock.Object).RecalculateAllAsync();

        mock.Verify(d => d.UpdateRolloversAsync(It.IsAny<IReadOnlyList<RolloverUpdate>>()), Times.Never);
    }

    [Fact]
    public async Task Encadena_el_sobrante_de_una_quincena_a_la_siguiente()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var (mock, captured) = MockDataSource(new List<PeriodRolloverRow>
        {
            Row(a, new DateOnly(2026, 6, 1), 1000m, 0m, 400m, 0m),
            Row(b, new DateOnly(2026, 6, 16), 1000m, 0m, 0m, 0m), // guardado 0, esperado 600
        });

        await new RolloverService(mock.Object).RecalculateAllAsync();

        var update = Assert.Single(captured()!);
        Assert.Equal(b, update.PeriodId);
        Assert.Equal(600m, update.Rollover);
    }

    [Fact]
    public async Task Propaga_deficits_negativos()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var (mock, captured) = MockDataSource(new List<PeriodRolloverRow>
        {
            Row(a, new DateOnly(2026, 6, 1), 500m, 0m, 1000m, 0m), // carry = 500 - 1000 = -500
            Row(b, new DateOnly(2026, 6, 16), 0m, 0m, 0m, 0m),
        });

        await new RolloverService(mock.Object).RecalculateAllAsync();

        var update = Assert.Single(captured()!);
        Assert.Equal(-500m, update.Rollover);
    }

    [Fact]
    public async Task La_primera_quincena_cronologica_siempre_queda_en_rollover_cero()
    {
        var a = Guid.NewGuid();
        var (mock, captured) = MockDataSource(new List<PeriodRolloverRow>
        {
            Row(a, new DateOnly(2026, 6, 1), 1000m, 999m, 0m, 0m), // guardado 999, pero es la primera => esperado 0
        });

        await new RolloverService(mock.Object).RecalculateAllAsync();

        var update = Assert.Single(captured()!);
        Assert.Equal(0m, update.Rollover);
    }

    [Fact]
    public async Task Redondea_hacia_afuera_de_cero_en_el_caso_borde_005()
    {
        // Sin MidpointRounding.AwayFromZero explícito, C# usaría redondeo bancario
        // (ToEven) y este caso daría 100.00 en vez de 100.01, divergiendo del
        // Math.round de JS (que siempre redondea .5 hacia arriba).
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var (mock, captured) = MockDataSource(new List<PeriodRolloverRow>
        {
            Row(a, new DateOnly(2026, 6, 1), 100.005m, 0m, 0m, 0m), // carry = 100.005
            Row(b, new DateOnly(2026, 6, 16), 0m, 0m, 0m, 0m),
        });

        await new RolloverService(mock.Object).RecalculateAllAsync();

        var update = Assert.Single(captured()!);
        Assert.Equal(100.01m, update.Rollover);
    }
}
