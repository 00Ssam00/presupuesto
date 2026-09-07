using Moq;
using Quincena.Application.PagosProximos;
using Quincena.Domain.Entidades;
using Xunit;

namespace Quincena.UnitTests.PagosProximos;

// Port de backend/src/controllers/upcomingPaymentsController.ts (sin test propio en Node).
public class ServicioPagosProximosTests
{
    [Fact]
    public async Task Create_usa_false_como_default_de_EsRecurrente_cuando_no_viene()
    {
        var repo = new Mock<IRepositorioPagosProximos>();
        PagoProximo? added = null;
        repo.Setup(r => r.AgregarAsync(It.IsAny<PagoProximo>()))
            .Callback<PagoProximo>(p => added = p)
            .ReturnsAsync((PagoProximo p) => p);
        var service = new ServicioPagosProximos(repo.Object);

        await service.CrearAsync(new CrearPagoProximoPeticion
        {
            CategoriaId = Guid.NewGuid(),
            Nombre = "Netflix",
            Monto = 25000,
            FechaVencimiento = new DateOnly(2026, 9, 1),
        });

        Assert.False(added!.EsRecurrente);
        Assert.Null(added.IntervaloRecurrencia);
    }

    [Fact]
    public async Task Update_sobreescribe_todos_los_campos_incluido_Estado()
    {
        var id = Guid.NewGuid();
        var existing = new PagoProximo
        {
            Id = id, CategoriaId = Guid.NewGuid(), Nombre = "Netflix", Monto = 25000,
            FechaVencimiento = new DateOnly(2026, 9, 1), Estado = "pending",
        };
        var repo = new Mock<IRepositorioPagosProximos>();
        repo.Setup(r => r.ActualizarAsync(id, It.IsAny<Action<PagoProximo>>()))
            .Returns<Guid, Action<PagoProximo>>((_, apply) => { apply(existing); return Task.FromResult<PagoProximo?>(existing); });
        var service = new ServicioPagosProximos(repo.Object);

        var nuevaCategoria = Guid.NewGuid();
        var result = await service.ActualizarAsync(id, new ActualizarPagoProximoPeticion
        {
            CategoriaId = nuevaCategoria, Nombre = "Netflix Premium", Monto = 30000,
            FechaVencimiento = new DateOnly(2026, 10, 1), Estado = "paid",
        });

        Assert.Equal("paid", result!.Estado);
        Assert.Equal(nuevaCategoria, result.CategoriaId);
        Assert.Equal(30000m, result.Monto);
    }

    [Fact]
    public async Task Delete_hace_borrado_fisico_no_soft_delete()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<IRepositorioPagosProximos>();
        var service = new ServicioPagosProximos(repo.Object);

        await service.EliminarAsync(id);

        repo.Verify(r => r.EliminarAsync(id), Times.Once);
    }
}
