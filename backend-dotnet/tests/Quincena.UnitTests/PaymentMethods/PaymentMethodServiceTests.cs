using Moq;
using Quincena.Application.PaymentMethods;
using Quincena.Domain.Entities;
using Xunit;

namespace Quincena.UnitTests.PaymentMethods;

// Port de backend/src/__tests__ (paymentMethodsController.ts no tiene test dedicado
// en Node; se cubre igual el único comportamiento que existe: listar activos).
public class PaymentMethodServiceTests
{
    [Fact]
    public async Task List_delega_en_el_repositorio()
    {
        var expected = new List<PaymentMethod> { new() { Name = "Efectivo" } };
        var repo = new Mock<IPaymentMethodRepository>();
        repo.Setup(r => r.GetActiveOrderedAsync()).ReturnsAsync(expected);
        var service = new PaymentMethodService(repo.Object);

        var result = await service.ListAsync();

        Assert.Same(expected, result);
    }
}
