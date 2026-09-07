using Moq;
using Quincena.Application.Metas;
using Quincena.Domain.Entidades;
using Xunit;

namespace Quincena.UnitTests.Metas;

// Port de backend/src/__tests__/goals.test.ts.
public class ServicioMetasTests
{
    [Fact]
    public async Task Create_usa_cero_como_default_de_MontoActual_cuando_no_viene()
    {
        var repo = new Mock<IRepositorioMetas>();
        Meta? added = null;
        repo.Setup(r => r.AgregarAsync(It.IsAny<Meta>()))
            .Callback<Meta>(g => added = g)
            .ReturnsAsync((Meta g) => g);
        var service = new ServicioMetas(repo.Object);

        await service.CrearAsync(new CrearMetaPeticion { Nombre = "Vacaciones", Tipo = "savings", MontoObjetivo = 1000000 });

        Assert.Equal(0m, added!.MontoActual);
    }

    [Fact]
    public async Task Update_no_borra_EstaActivo_cuando_no_viene_en_el_body()
    {
        // Regresión: is_active pasaba a NULL y la meta desaparecía de los
        // listados que filtran is_active = TRUE (ver goals.test.ts en Node).
        var id = Guid.NewGuid();
        var existing = new Meta { Id = id, Nombre = "Vacaciones", Tipo = "savings", EstaActivo = true };
        var repo = new Mock<IRepositorioMetas>();
        repo.Setup(r => r.ActualizarAsync(id, It.IsAny<Action<Meta>>()))
            .Returns<Guid, Action<Meta>>((_, apply) => { apply(existing); return Task.FromResult<Meta?>(existing); });
        var service = new ServicioMetas(repo.Object);

        var result = await service.ActualizarAsync(id, new ActualizarMetaPeticion
        {
            Nombre = "Vacaciones", Tipo = "savings", MontoObjetivo = 1000000, MontoActual = 200000,
        });

        Assert.True(result!.EstaActivo);
    }

    [Fact]
    public async Task Update_permite_desactivar_explicitamente_enviando_EstaActivo_false()
    {
        var id = Guid.NewGuid();
        var existing = new Meta { Id = id, Nombre = "Vacaciones", Tipo = "savings", EstaActivo = true };
        var repo = new Mock<IRepositorioMetas>();
        repo.Setup(r => r.ActualizarAsync(id, It.IsAny<Action<Meta>>()))
            .Returns<Guid, Action<Meta>>((_, apply) => { apply(existing); return Task.FromResult<Meta?>(existing); });
        var service = new ServicioMetas(repo.Object);

        var result = await service.ActualizarAsync(id, new ActualizarMetaPeticion
        {
            Nombre = "Vacaciones", Tipo = "savings", MontoObjetivo = 1000000, MontoActual = 200000, EstaActivo = false,
        });

        Assert.False(result!.EstaActivo);
    }

    [Fact]
    public async Task Delete_hace_soft_delete()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<IRepositorioMetas>();
        var service = new ServicioMetas(repo.Object);

        await service.EliminarAsync(id);

        repo.Verify(r => r.DesactivarAsync(id), Times.Once);
    }
}
