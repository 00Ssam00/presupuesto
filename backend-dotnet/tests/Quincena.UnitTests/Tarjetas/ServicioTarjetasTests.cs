using Moq;
using Quincena.Application.Tarjetas;
using Quincena.Domain.Entidades;
using Xunit;

namespace Quincena.UnitTests.Tarjetas;

// Port de backend/src/__tests__/cards.test.ts.
public class CardServiceTests
{
    [Fact]
    public async Task Create_convierte_last_four_y_bank_vacios_a_null()
    {
        var repo = new Mock<IRepositorioTarjetas>();
        Tarjeta? added = null;
        repo.Setup(r => r.AgregarAsync(It.IsAny<Tarjeta>()))
            .Callback<Tarjeta>(c => added = c)
            .ReturnsAsync((Tarjeta c) => c);
        var service = new ServicioTarjetas(repo.Object);

        await service.CrearAsync(new CrearTarjetaPeticion { Nombre = "Visa", UltimosCuatro = "", Banco = "" });

        Assert.Null(added!.UltimosCuatro);
        Assert.Null(added.Banco);
    }

    [Fact]
    public async Task Update_no_borra_is_active_cuando_no_viene_en_el_body()
    {
        // Regresión: is_active pasaba a NULL y la tarjeta desaparecía de los
        // listados que filtran is_active = TRUE (ver cards.test.ts en Node).
        var id = Guid.NewGuid();
        var existing = new Tarjeta { Id = id, Nombre = "Visa", EstaActivo = true };
        var repo = new Mock<IRepositorioTarjetas>();
        repo.Setup(r => r.ActualizarAsync(id, It.IsAny<Action<Tarjeta>>()))
            .Returns<Guid, Action<Tarjeta>>((_, apply) => { apply(existing); return Task.FromResult<Tarjeta?>(existing); });
        var service = new ServicioTarjetas(repo.Object);

        var result = await service.ActualizarAsync(id, new ActualizarTarjetaPeticion
        {
            Nombre = "Visa", UltimosCuatro = "1234", Banco = "Bancolombia",
        });

        Assert.True(result!.EstaActivo);
        Assert.Equal("1234", result.UltimosCuatro);
        Assert.Equal("Bancolombia", result.Banco);
    }

    [Fact]
    public async Task Update_sin_last_four_ni_bank_los_deja_en_null_igual_que_Node()
    {
        // A diferencia de icon en categories, last_four/bank en Node NO usan
        // COALESCE — se sobrescriben siempre, incluso a null si no vienen.
        var id = Guid.NewGuid();
        var existing = new Tarjeta { Id = id, Nombre = "Visa", UltimosCuatro = "9999", Banco = "Nu", EstaActivo = true };
        var repo = new Mock<IRepositorioTarjetas>();
        repo.Setup(r => r.ActualizarAsync(id, It.IsAny<Action<Tarjeta>>()))
            .Returns<Guid, Action<Tarjeta>>((_, apply) => { apply(existing); return Task.FromResult<Tarjeta?>(existing); });
        var service = new ServicioTarjetas(repo.Object);

        var result = await service.ActualizarAsync(id, new ActualizarTarjetaPeticion { Nombre = "Visa" });

        Assert.Null(result!.UltimosCuatro);
        Assert.Null(result.Banco);
    }

    [Fact]
    public async Task Update_permite_desactivar_explicitamente()
    {
        var id = Guid.NewGuid();
        var existing = new Tarjeta { Id = id, Nombre = "Visa", EstaActivo = true };
        var repo = new Mock<IRepositorioTarjetas>();
        repo.Setup(r => r.ActualizarAsync(id, It.IsAny<Action<Tarjeta>>()))
            .Returns<Guid, Action<Tarjeta>>((_, apply) => { apply(existing); return Task.FromResult<Tarjeta?>(existing); });
        var service = new ServicioTarjetas(repo.Object);

        var result = await service.ActualizarAsync(id, new ActualizarTarjetaPeticion { Nombre = "Visa", EstaActivo = false });

        Assert.False(result!.EstaActivo);
    }

    [Fact]
    public async Task Update_retorna_null_si_no_existe()
    {
        var repo = new Mock<IRepositorioTarjetas>();
        repo.Setup(r => r.ActualizarAsync(It.IsAny<Guid>(), It.IsAny<Action<Tarjeta>>()))
            .ReturnsAsync((Tarjeta?)null);
        var service = new ServicioTarjetas(repo.Object);

        var result = await service.ActualizarAsync(Guid.NewGuid(), new ActualizarTarjetaPeticion { Nombre = "X" });

        Assert.Null(result);
    }

    [Fact]
    public async Task Delete_hace_soft_delete()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<IRepositorioTarjetas>();
        var service = new ServicioTarjetas(repo.Object);

        await service.EliminarAsync(id);

        repo.Verify(r => r.DesactivarAsync(id), Times.Once);
    }
}
