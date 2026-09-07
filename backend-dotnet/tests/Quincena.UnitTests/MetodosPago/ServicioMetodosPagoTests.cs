using Moq;
using Quincena.Application.MetodosPago;
using Quincena.Domain.Entidades;
using Xunit;

namespace Quincena.UnitTests.MetodosPago;

// Port de backend/src/__tests__ (paymentMethodsController.ts no tiene test dedicado
// en Node; se cubre igual el único comportamiento que existe: listar activos).
public class PaymentMethodServiceTests
{
    [Fact]
    public async Task List_delega_en_el_repositorio()
    {
        var expected = new List<MetodoPago> { new() { Nombre = "Efectivo" } };
        var repo = new Mock<IRepositorioMetodosPago>();
        repo.Setup(r => r.ObtenerActivosOrdenadosAsync()).ReturnsAsync(expected);
        var service = new ServicioMetodosPago(repo.Object);

        var result = await service.ListarAsync();

        Assert.Same(expected, result);
    }
}
