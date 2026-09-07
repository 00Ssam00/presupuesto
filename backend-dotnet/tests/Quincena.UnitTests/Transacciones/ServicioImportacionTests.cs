using Moq;
using Quincena.Application.Tarjetas;
using Quincena.Application.Categorias;
using Quincena.Application.MetodosPago;
using Quincena.Application.Periodos;
using Quincena.Application.Arrastre;
using Quincena.Application.Transacciones;
using Quincena.Domain.Entidades;
using Xunit;

namespace Quincena.UnitTests.Transacciones;

// Port de backend/src/__tests__/transactions-import.test.ts (los casos de negocio;
// "rechaza sin rows"/"rechaza array vacío" son un guard del controller, cubiertos en
// integración).
public class ImportServiceTests
{
    private static readonly Periodo PeriodA = new() { Id = Guid.NewGuid(), FechaInicio = new DateOnly(2026, 6, 1), FechaFin = new DateOnly(2026, 6, 15) };
    private static readonly Periodo PeriodB = new() { Id = Guid.NewGuid(), FechaInicio = new DateOnly(2026, 6, 16), FechaFin = new DateOnly(2026, 6, 30) };
    private static readonly Categoria CategoriaAlimentacion = new() { Id = Guid.NewGuid(), Nombre = "Alimentación" };
    private static readonly MetodoPago MetodoEfectivo = new() { Id = Guid.NewGuid(), Nombre = "Efectivo", RequiereTarjeta = false };
    private static readonly MetodoPago MetodoCredito = new() { Id = Guid.NewGuid(), Nombre = "Crédito", RequiereTarjeta = true };
    private static readonly Tarjeta TarjetaVisa = new() { Id = Guid.NewGuid(), Nombre = "Visa Gold" };

    private static ServicioTransacciones BuildService(
        out Mock<IRepositorioTransacciones> repo,
        out Mock<IServicioArrastre> rollover,
        List<Periodo>? periods = null,
        List<Categoria>? categories = null,
        List<MetodoPago>? methods = null,
        List<Tarjeta>? cards = null)
    {
        repo = new Mock<IRepositorioTransacciones>();
        repo.Setup(r => r.AgregarAsync(It.IsAny<Transaccion>())).ReturnsAsync((Transaccion t) => { t.Id = Guid.NewGuid(); return t; });
        rollover = new Mock<IServicioArrastre>();

        var periodRepo = new Mock<IRepositorioPeriodos>();
        periodRepo.Setup(r => r.ObtenerTodosOrdenadosPorFechaInicioDescAsync()).ReturnsAsync(periods ?? [PeriodA, PeriodB]);
        var categoryRepo = new Mock<IRepositorioCategorias>();
        categoryRepo.Setup(r => r.ObtenerActivosOrdenadosAsync()).ReturnsAsync(categories ?? [CategoriaAlimentacion]);
        var methodRepo = new Mock<IRepositorioMetodosPago>();
        methodRepo.Setup(r => r.ObtenerActivosOrdenadosAsync()).ReturnsAsync(methods ?? [MetodoEfectivo]);
        var cardRepo = new Mock<IRepositorioTarjetas>();
        cardRepo.Setup(r => r.ObtenerActivosOrdenadosAsync()).ReturnsAsync(cards ?? []);

        return new ServicioTransacciones(repo.Object, rollover.Object, periodRepo.Object, categoryRepo.Object, methodRepo.Object, cardRepo.Object);
    }

    [Fact]
    public async Task Importa_una_fila_valida_resolviendo_la_quincena_por_su_fecha_y_recalcula()
    {
        var service = BuildService(out _, out var rollover);

        var result = await service.ImportarAsync([
            new FilaImportacion { Monto = "85000", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Efectivo", Descripcion = "Mercado" },
        ]);

        Assert.Equal(1, result.Creados);
        Assert.Equal(0, result.Fallidos);
        Assert.Equal("created", result.Resultados[0].Estado);
        rollover.Verify(r => r.RecalcularTodoAsync(), Times.Once);
    }

    [Fact]
    public async Task Importa_un_archivo_que_abarca_dos_quincenas_en_una_sola_pasada()
    {
        var service = BuildService(out var repo, out _);
        var inserted = new List<Guid>();
        repo.Setup(r => r.AgregarAsync(It.IsAny<Transaccion>()))
            .Callback<Transaccion>(t => inserted.Add(t.PeriodoId))
            .ReturnsAsync((Transaccion t) => { t.Id = Guid.NewGuid(); return t; });

        var result = await service.ImportarAsync([
            new FilaImportacion { Monto = "10000", Tipo = "expense", Fecha = "2026-06-05", Categoria = "Alimentación", Metodo = "Efectivo" },
            new FilaImportacion { Monto = "20000", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Efectivo" },
        ]);

        Assert.Equal(2, result.Creados);
        Assert.Equal(PeriodA.Id, inserted[0]);
        Assert.Equal(PeriodB.Id, inserted[1]);
    }

    [Fact]
    public async Task Rechaza_filas_sin_fecha_o_con_fecha_invalida()
    {
        var service = BuildService(out var repo, out var rollover);

        var result = await service.ImportarAsync([
            new FilaImportacion { Monto = "85000", Tipo = "expense", Categoria = "Alimentación", Metodo = "Efectivo" },
        ]);

        Assert.Equal(0, result.Creados);
        Assert.Contains("Fecha requerida", result.Resultados[0].Error);
        repo.Verify(r => r.AgregarAsync(It.IsAny<Transaccion>()), Times.Never);
        rollover.Verify(r => r.RecalcularTodoAsync(), Times.Never);
    }

    [Fact]
    public async Task Reporta_error_claro_cuando_la_fecha_no_cae_en_ninguna_quincena()
    {
        var service = BuildService(out _, out _);

        var result = await service.ImportarAsync([
            new FilaImportacion { Monto = "85000", Tipo = "expense", Fecha = "2099-01-01", Categoria = "Alimentación", Metodo = "Efectivo" },
        ]);

        Assert.Equal(1, result.Fallidos);
        Assert.Contains("ninguna quincena que incluya la fecha", result.Resultados[0].Error);
    }

    [Fact]
    public async Task Reporta_por_fila_categorias_inexistentes_sin_bloquear_el_resto()
    {
        var service = BuildService(out _, out _);

        var result = await service.ImportarAsync([
            new FilaImportacion { Monto = "85000", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Categoría Inexistente", Metodo = "Efectivo" },
            new FilaImportacion { Monto = "20000", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Efectivo" },
        ]);

        Assert.Equal(1, result.Creados);
        Assert.Equal(1, result.Fallidos);
        Assert.Equal("error", result.Resultados[0].Estado);
        Assert.Contains("Categoría Inexistente", result.Resultados[0].Error);
        Assert.Equal("created", result.Resultados[1].Estado);
    }

    [Fact]
    public async Task Exige_tarjeta_cuando_el_metodo_de_pago_la_requiere()
    {
        var service = BuildService(out _, out _, methods: [MetodoCredito], cards: []);

        var result = await service.ImportarAsync([
            new FilaImportacion { Monto = "85000", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Crédito", Tarjeta = "Visa Gold" },
        ]);

        Assert.Equal(1, result.Fallidos);
        Assert.Contains("requiere tarjeta", result.Resultados[0].Error);
    }

    [Fact]
    public async Task Resuelve_la_tarjeta_correctamente_cuando_existe_case_insensitive()
    {
        var service = BuildService(out var repo, out _, methods: [MetodoCredito], cards: [TarjetaVisa]);
        Transaccion? added = null;
        repo.Setup(r => r.AgregarAsync(It.IsAny<Transaccion>()))
            .Callback<Transaccion>(t => added = t)
            .ReturnsAsync((Transaccion t) => { t.Id = Guid.NewGuid(); return t; });

        var result = await service.ImportarAsync([
            new FilaImportacion { Monto = "85000", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Crédito", Tarjeta = "visa gold" },
        ]);

        Assert.Equal(1, result.Creados);
        Assert.Equal(TarjetaVisa.Id, added!.TarjetaId);
    }

    [Fact]
    public async Task Monto_vacio_o_no_positivo_es_invalido()
    {
        var service = BuildService(out _, out _);

        var result = await service.ImportarAsync([
            new FilaImportacion { Monto = "", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Efectivo" },
            new FilaImportacion { Monto = "0", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Efectivo" },
            new FilaImportacion { Monto = "-5", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Efectivo" },
        ]);

        Assert.Equal(0, result.Creados);
        Assert.Equal(3, result.Fallidos);
        Assert.All(result.Resultados, r => Assert.Contains("Monto inválido", r.Error));
    }

    [Fact]
    public async Task Tipo_debe_ser_income_o_expense()
    {
        var service = BuildService(out _, out _);

        var result = await service.ImportarAsync([
            new FilaImportacion { Monto = "1000", Tipo = "otro", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Efectivo" },
        ]);

        Assert.Equal(1, result.Fallidos);
        Assert.Contains("income", result.Resultados[0].Error);
    }

    [Fact]
    public async Task Descripcion_vacia_usa_Importado_por_defecto()
    {
        var service = BuildService(out var repo, out _);
        Transaccion? added = null;
        repo.Setup(r => r.AgregarAsync(It.IsAny<Transaccion>()))
            .Callback<Transaccion>(t => added = t)
            .ReturnsAsync((Transaccion t) => { t.Id = Guid.NewGuid(); return t; });

        await service.ImportarAsync([
            new FilaImportacion { Monto = "1000", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Efectivo" },
        ]);

        Assert.Equal("Importado", added!.Descripcion);
    }
}
