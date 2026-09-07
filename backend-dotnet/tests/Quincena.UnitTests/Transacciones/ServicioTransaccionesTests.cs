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

// Port de la parte CRUD de backend/src/__tests__/transactions.test.ts (la parte de
// importTransactions se prueba en ImportServiceTests). "propaga errores de BD como
// 500" se cubre a nivel de integración vía ExceptionHandlingMiddleware.
public class TransactionServiceTests
{
    private static ServicioTransacciones BuildService(
        Mock<IRepositorioTransacciones> repo,
        Mock<IServicioArrastre>? rollover = null) =>
        new(
            repo.Object,
            (rollover ?? new Mock<IServicioArrastre>()).Object,
            new Mock<IRepositorioPeriodos>().Object,
            new Mock<IRepositorioCategorias>().Object,
            new Mock<IRepositorioMetodosPago>().Object,
            new Mock<IRepositorioTarjetas>().Object);

    [Fact]
    public async Task Create_usa_null_para_card_id_ausente_y_recalcula()
    {
        var repo = new Mock<IRepositorioTransacciones>();
        Transaccion? added = null;
        repo.Setup(r => r.AgregarAsync(It.IsAny<Transaccion>()))
            .Callback<Transaccion>(t => added = t)
            .ReturnsAsync((Transaccion t) => t);
        var rollover = new Mock<IServicioArrastre>();
        var service = BuildService(repo, rollover);

        await service.CrearAsync(new CrearTransaccionPeticion
        {
            PeriodoId = Guid.NewGuid(), CategoriaId = Guid.NewGuid(), MetodoPagoId = Guid.NewGuid(),
            Tipo = "expense", Monto = 5000m, Descripcion = "Mercado", Fecha = new DateOnly(2026, 6, 20),
        });

        Assert.Null(added!.TarjetaId);
        rollover.Verify(r => r.RecalcularTodoAsync(), Times.Once);
    }

    [Fact]
    public async Task Update_retorna_null_sin_recalcular_si_no_existe()
    {
        var repo = new Mock<IRepositorioTransacciones>();
        repo.Setup(r => r.ActualizarAsync(It.IsAny<Guid>(), It.IsAny<Action<Transaccion>>())).ReturnsAsync((Transaccion?)null);
        var rollover = new Mock<IServicioArrastre>();
        var service = BuildService(repo, rollover);

        var result = await service.ActualizarAsync(Guid.NewGuid(), new ActualizarTransaccionPeticion
        {
            CategoriaId = Guid.NewGuid(), MetodoPagoId = Guid.NewGuid(),
            Tipo = "expense", Monto = 3000m, Descripcion = "x", Fecha = new DateOnly(2026, 6, 21),
        });

        Assert.Null(result);
        rollover.Verify(r => r.RecalcularTodoAsync(), Times.Never);
    }

    [Fact]
    public async Task Update_recalcula_si_existe_y_no_permite_mover_de_quincena()
    {
        var id = Guid.NewGuid();
        var existing = new Transaccion { Id = id, PeriodoId = Guid.NewGuid() };
        var originalPeriodId = existing.PeriodoId;
        var repo = new Mock<IRepositorioTransacciones>();
        repo.Setup(r => r.ActualizarAsync(id, It.IsAny<Action<Transaccion>>()))
            .Returns<Guid, Action<Transaccion>>((_, apply) => { apply(existing); return Task.FromResult<Transaccion?>(existing); });
        var rollover = new Mock<IServicioArrastre>();
        var service = BuildService(repo, rollover);

        var result = await service.ActualizarAsync(id, new ActualizarTransaccionPeticion
        {
            CategoriaId = Guid.NewGuid(), MetodoPagoId = Guid.NewGuid(),
            Tipo = "expense", Monto = 3000m, Descripcion = "Ajuste", Fecha = new DateOnly(2026, 6, 21),
        });

        Assert.Same(existing, result);
        Assert.Equal(originalPeriodId, result!.PeriodoId); // ActualizarTransaccionPeticion no trae period_id
        rollover.Verify(r => r.RecalcularTodoAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_siempre_recalcula_sin_validar_existencia_previa()
    {
        var repo = new Mock<IRepositorioTransacciones>();
        var rollover = new Mock<IServicioArrastre>();
        var service = BuildService(repo, rollover);

        await service.EliminarAsync(Guid.NewGuid());

        rollover.Verify(r => r.RecalcularTodoAsync(), Times.Once);
    }
}
