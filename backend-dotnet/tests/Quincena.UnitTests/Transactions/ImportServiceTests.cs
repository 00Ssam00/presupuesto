using Moq;
using Quincena.Application.Cards;
using Quincena.Application.Categories;
using Quincena.Application.PaymentMethods;
using Quincena.Application.Periods;
using Quincena.Application.Rollover;
using Quincena.Application.Transactions;
using Quincena.Domain.Entities;
using Xunit;

namespace Quincena.UnitTests.Transactions;

// Port de backend/src/__tests__/transactions-import.test.ts (los casos de negocio;
// "rechaza sin rows"/"rechaza array vacío" son un guard del controller, cubiertos en
// integración).
public class ImportServiceTests
{
    private static readonly Period PeriodA = new() { Id = Guid.NewGuid(), StartDate = new DateOnly(2026, 6, 1), EndDate = new DateOnly(2026, 6, 15) };
    private static readonly Period PeriodB = new() { Id = Guid.NewGuid(), StartDate = new DateOnly(2026, 6, 16), EndDate = new DateOnly(2026, 6, 30) };
    private static readonly Category CategoriaAlimentacion = new() { Id = Guid.NewGuid(), Name = "Alimentación" };
    private static readonly PaymentMethod MetodoEfectivo = new() { Id = Guid.NewGuid(), Name = "Efectivo", RequiresCard = false };
    private static readonly PaymentMethod MetodoCredito = new() { Id = Guid.NewGuid(), Name = "Crédito", RequiresCard = true };
    private static readonly Card TarjetaVisa = new() { Id = Guid.NewGuid(), Name = "Visa Gold" };

    private static TransactionService BuildService(
        out Mock<ITransactionRepository> repo,
        out Mock<IRolloverService> rollover,
        List<Period>? periods = null,
        List<Category>? categories = null,
        List<PaymentMethod>? methods = null,
        List<Card>? cards = null)
    {
        repo = new Mock<ITransactionRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<Transaction>())).ReturnsAsync((Transaction t) => { t.Id = Guid.NewGuid(); return t; });
        rollover = new Mock<IRolloverService>();

        var periodRepo = new Mock<IPeriodRepository>();
        periodRepo.Setup(r => r.GetAllOrderByStartDateDescAsync()).ReturnsAsync(periods ?? [PeriodA, PeriodB]);
        var categoryRepo = new Mock<ICategoryRepository>();
        categoryRepo.Setup(r => r.GetActiveOrderedAsync()).ReturnsAsync(categories ?? [CategoriaAlimentacion]);
        var methodRepo = new Mock<IPaymentMethodRepository>();
        methodRepo.Setup(r => r.GetActiveOrderedAsync()).ReturnsAsync(methods ?? [MetodoEfectivo]);
        var cardRepo = new Mock<ICardRepository>();
        cardRepo.Setup(r => r.GetActiveOrderedAsync()).ReturnsAsync(cards ?? []);

        return new TransactionService(repo.Object, rollover.Object, periodRepo.Object, categoryRepo.Object, methodRepo.Object, cardRepo.Object);
    }

    [Fact]
    public async Task Importa_una_fila_valida_resolviendo_la_quincena_por_su_fecha_y_recalcula()
    {
        var service = BuildService(out _, out var rollover);

        var result = await service.ImportAsync([
            new ImportRow { Monto = "85000", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Efectivo", Descripcion = "Mercado" },
        ]);

        Assert.Equal(1, result.Created);
        Assert.Equal(0, result.Failed);
        Assert.Equal("created", result.Results[0].Status);
        rollover.Verify(r => r.RecalculateAllAsync(), Times.Once);
    }

    [Fact]
    public async Task Importa_un_archivo_que_abarca_dos_quincenas_en_una_sola_pasada()
    {
        var service = BuildService(out var repo, out _);
        var inserted = new List<Guid>();
        repo.Setup(r => r.AddAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(t => inserted.Add(t.PeriodId))
            .ReturnsAsync((Transaction t) => { t.Id = Guid.NewGuid(); return t; });

        var result = await service.ImportAsync([
            new ImportRow { Monto = "10000", Tipo = "expense", Fecha = "2026-06-05", Categoria = "Alimentación", Metodo = "Efectivo" },
            new ImportRow { Monto = "20000", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Efectivo" },
        ]);

        Assert.Equal(2, result.Created);
        Assert.Equal(PeriodA.Id, inserted[0]);
        Assert.Equal(PeriodB.Id, inserted[1]);
    }

    [Fact]
    public async Task Rechaza_filas_sin_fecha_o_con_fecha_invalida()
    {
        var service = BuildService(out var repo, out var rollover);

        var result = await service.ImportAsync([
            new ImportRow { Monto = "85000", Tipo = "expense", Categoria = "Alimentación", Metodo = "Efectivo" },
        ]);

        Assert.Equal(0, result.Created);
        Assert.Contains("Fecha requerida", result.Results[0].Error);
        repo.Verify(r => r.AddAsync(It.IsAny<Transaction>()), Times.Never);
        rollover.Verify(r => r.RecalculateAllAsync(), Times.Never);
    }

    [Fact]
    public async Task Reporta_error_claro_cuando_la_fecha_no_cae_en_ninguna_quincena()
    {
        var service = BuildService(out _, out _);

        var result = await service.ImportAsync([
            new ImportRow { Monto = "85000", Tipo = "expense", Fecha = "2099-01-01", Categoria = "Alimentación", Metodo = "Efectivo" },
        ]);

        Assert.Equal(1, result.Failed);
        Assert.Contains("ninguna quincena que incluya la fecha", result.Results[0].Error);
    }

    [Fact]
    public async Task Reporta_por_fila_categorias_inexistentes_sin_bloquear_el_resto()
    {
        var service = BuildService(out _, out _);

        var result = await service.ImportAsync([
            new ImportRow { Monto = "85000", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Categoría Inexistente", Metodo = "Efectivo" },
            new ImportRow { Monto = "20000", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Efectivo" },
        ]);

        Assert.Equal(1, result.Created);
        Assert.Equal(1, result.Failed);
        Assert.Equal("error", result.Results[0].Status);
        Assert.Contains("Categoría Inexistente", result.Results[0].Error);
        Assert.Equal("created", result.Results[1].Status);
    }

    [Fact]
    public async Task Exige_tarjeta_cuando_el_metodo_de_pago_la_requiere()
    {
        var service = BuildService(out _, out _, methods: [MetodoCredito], cards: []);

        var result = await service.ImportAsync([
            new ImportRow { Monto = "85000", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Crédito", Tarjeta = "Visa Gold" },
        ]);

        Assert.Equal(1, result.Failed);
        Assert.Contains("requiere tarjeta", result.Results[0].Error);
    }

    [Fact]
    public async Task Resuelve_la_tarjeta_correctamente_cuando_existe_case_insensitive()
    {
        var service = BuildService(out var repo, out _, methods: [MetodoCredito], cards: [TarjetaVisa]);
        Transaction? added = null;
        repo.Setup(r => r.AddAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(t => added = t)
            .ReturnsAsync((Transaction t) => { t.Id = Guid.NewGuid(); return t; });

        var result = await service.ImportAsync([
            new ImportRow { Monto = "85000", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Crédito", Tarjeta = "visa gold" },
        ]);

        Assert.Equal(1, result.Created);
        Assert.Equal(TarjetaVisa.Id, added!.CardId);
    }

    [Fact]
    public async Task Monto_vacio_o_no_positivo_es_invalido()
    {
        var service = BuildService(out _, out _);

        var result = await service.ImportAsync([
            new ImportRow { Monto = "", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Efectivo" },
            new ImportRow { Monto = "0", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Efectivo" },
            new ImportRow { Monto = "-5", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Efectivo" },
        ]);

        Assert.Equal(0, result.Created);
        Assert.Equal(3, result.Failed);
        Assert.All(result.Results, r => Assert.Contains("Monto inválido", r.Error));
    }

    [Fact]
    public async Task Tipo_debe_ser_income_o_expense()
    {
        var service = BuildService(out _, out _);

        var result = await service.ImportAsync([
            new ImportRow { Monto = "1000", Tipo = "otro", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Efectivo" },
        ]);

        Assert.Equal(1, result.Failed);
        Assert.Contains("income", result.Results[0].Error);
    }

    [Fact]
    public async Task Descripcion_vacia_usa_Importado_por_defecto()
    {
        var service = BuildService(out var repo, out _);
        Transaction? added = null;
        repo.Setup(r => r.AddAsync(It.IsAny<Transaction>()))
            .Callback<Transaction>(t => added = t)
            .ReturnsAsync((Transaction t) => { t.Id = Guid.NewGuid(); return t; });

        await service.ImportAsync([
            new ImportRow { Monto = "1000", Tipo = "expense", Fecha = "2026-06-20", Categoria = "Alimentación", Metodo = "Efectivo" },
        ]);

        Assert.Equal("Importado", added!.Description);
    }
}
