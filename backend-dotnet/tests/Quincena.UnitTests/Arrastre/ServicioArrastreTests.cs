using Moq;
using Quincena.Application.Arrastre;
using Xunit;

namespace Quincena.UnitTests.Arrastre;

// Port de backend/src/services/rollover.test.ts — mismos 4 casos, más un caso de borde
// de redondeo específico de la migración (ver comentario en ServicioArrastre).
public class RolloverServiceTests
{
    private static FilaArrastrePeriodo Fila(Guid id, DateOnly start, decimal baseIncome, decimal rollover, decimal expenses, decimal incomeExtra) =>
        new(id, start, baseIncome, rollover, expenses, incomeExtra);

    private static (Mock<IFuenteDatosArrastre> mock, Func<List<ActualizacionArrastre>?> captured) MockDataSource(List<FilaArrastrePeriodo> rows)
    {
        var mock = new Mock<IFuenteDatosArrastre>();
        mock.Setup(d => d.ObtenerTotalesPeriodosAsync()).ReturnsAsync(rows);
        List<ActualizacionArrastre>? captured = null;
        mock.Setup(d => d.ActualizarArrastresAsync(It.IsAny<IReadOnlyList<ActualizacionArrastre>>()))
            .Callback<IReadOnlyList<ActualizacionArrastre>>(u => captured = u.ToList())
            .Returns(Task.CompletedTask);
        return (mock, () => captured);
    }

    [Fact]
    public async Task No_actualiza_nada_si_los_rollovers_ya_estan_correctos()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var (mock, _) = MockDataSource(new List<FilaArrastrePeriodo>
        {
            Fila(a, new DateOnly(2026, 6, 1), 1000m, 0m, 400m, 0m),   // primer período: esperado siempre 0, coincide
            Fila(b, new DateOnly(2026, 6, 16), 1000m, 600m, 0m, 0m),  // carry de A = 1000+0+0-400 = 600, coincide
        });

        await new ServicioArrastre(mock.Object).RecalcularTodoAsync();

        mock.Verify(d => d.ActualizarArrastresAsync(It.IsAny<IReadOnlyList<ActualizacionArrastre>>()), Times.Never);
    }

    [Fact]
    public async Task Encadena_el_sobrante_de_una_quincena_a_la_siguiente()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var (mock, captured) = MockDataSource(new List<FilaArrastrePeriodo>
        {
            Fila(a, new DateOnly(2026, 6, 1), 1000m, 0m, 400m, 0m),
            Fila(b, new DateOnly(2026, 6, 16), 1000m, 0m, 0m, 0m), // guardado 0, esperado 600
        });

        await new ServicioArrastre(mock.Object).RecalcularTodoAsync();

        var update = Assert.Single(captured()!);
        Assert.Equal(b, update.PeriodoId);
        Assert.Equal(600m, update.Arrastre);
    }

    [Fact]
    public async Task Propaga_deficits_negativos()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var (mock, captured) = MockDataSource(new List<FilaArrastrePeriodo>
        {
            Fila(a, new DateOnly(2026, 6, 1), 500m, 0m, 1000m, 0m), // carry = 500 - 1000 = -500
            Fila(b, new DateOnly(2026, 6, 16), 0m, 0m, 0m, 0m),
        });

        await new ServicioArrastre(mock.Object).RecalcularTodoAsync();

        var update = Assert.Single(captured()!);
        Assert.Equal(-500m, update.Arrastre);
    }

    [Fact]
    public async Task La_primera_quincena_cronologica_siempre_queda_en_rollover_cero()
    {
        var a = Guid.NewGuid();
        var (mock, captured) = MockDataSource(new List<FilaArrastrePeriodo>
        {
            Fila(a, new DateOnly(2026, 6, 1), 1000m, 999m, 0m, 0m), // guardado 999, pero es la primera => esperado 0
        });

        await new ServicioArrastre(mock.Object).RecalcularTodoAsync();

        var update = Assert.Single(captured()!);
        Assert.Equal(0m, update.Arrastre);
    }

    [Fact]
    public async Task Redondea_hacia_afuera_de_cero_en_el_caso_borde_005()
    {
        // Sin MidpointRounding.AwayFromZero explícito, C# usaría redondeo bancario
        // (ToEven) y este caso daría 100.00 en vez de 100.01, divergiendo del
        // Math.round de JS (que siempre redondea .5 hacia arriba).
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var (mock, captured) = MockDataSource(new List<FilaArrastrePeriodo>
        {
            Fila(a, new DateOnly(2026, 6, 1), 100.005m, 0m, 0m, 0m), // carry = 100.005
            Fila(b, new DateOnly(2026, 6, 16), 0m, 0m, 0m, 0m),
        });

        await new ServicioArrastre(mock.Object).RecalcularTodoAsync();

        var update = Assert.Single(captured()!);
        Assert.Equal(100.01m, update.Arrastre);
    }
}
