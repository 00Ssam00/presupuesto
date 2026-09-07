using Moq;
using Quincena.Application.Ajustes;
using Quincena.Domain.Entidades;
using Xunit;

namespace Quincena.UnitTests.Ajustes;

// Port de backend/src/controllers/settingsController.ts (sin test propio en Node).
public class ServicioAjustesTests
{
    [Fact]
    public async Task ObtenerTodosAsync_convierte_la_lista_en_un_mapa_clave_valor()
    {
        var repo = new Mock<IRepositorioAjustes>();
        repo.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(
        [
            new Ajuste { Clave = "moneda", Valor = "COP" },
            new Ajuste { Clave = "tema", Valor = "oscuro" },
        ]);
        var service = new ServicioAjustes(repo.Object);

        var result = await service.ObtenerTodosAsync();

        Assert.Equal("COP", result["moneda"]);
        Assert.Equal("oscuro", result["tema"]);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ActualizarAsync_delega_el_upsert_al_repositorio()
    {
        var repo = new Mock<IRepositorioAjustes>();
        var service = new ServicioAjustes(repo.Object);

        await service.ActualizarAsync("moneda", "USD");

        repo.Verify(r => r.GuardarAsync("moneda", "USD"), Times.Once);
    }
}
