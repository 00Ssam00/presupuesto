using Moq;
using Quincena.Application.Categorias;
using Quincena.Domain.Entidades;
using Xunit;

namespace Quincena.UnitTests.Categorias;

// Port de backend/src/__tests__/categories.test.ts.
public class CategoryServiceTests
{
    [Fact]
    public async Task Create_aplica_defaults_cuando_color_icon_y_sort_order_no_vienen()
    {
        var repo = new Mock<IRepositorioCategorias>();
        Categoria? added = null;
        repo.Setup(r => r.AgregarAsync(It.IsAny<Categoria>()))
            .Callback<Categoria>(c => added = c)
            .ReturnsAsync((Categoria c) => c);
        var service = new ServicioCategorias(repo.Object);

        await service.CrearAsync(new CrearCategoriaPeticion { Nombre = "Ocio" });

        Assert.Equal("#01696f", added!.Color);
        Assert.Null(added.Icono);
        Assert.Equal(0, added.Orden);
    }

    [Fact]
    public async Task Create_conserva_valores_explicitos()
    {
        var repo = new Mock<IRepositorioCategorias>();
        Categoria? added = null;
        repo.Setup(r => r.AgregarAsync(It.IsAny<Categoria>()))
            .Callback<Categoria>(c => added = c)
            .ReturnsAsync((Categoria c) => c);
        var service = new ServicioCategorias(repo.Object);

        await service.CrearAsync(new CrearCategoriaPeticion
        {
            Nombre = "Ocio", Color = "#111111", Icono = "smile", Orden = 3,
        });

        Assert.Equal("#111111", added!.Color);
        Assert.Equal("smile", added.Icono);
        Assert.Equal(3, added.Orden);
    }

    [Fact]
    public async Task Update_no_borra_icon_is_active_ni_sort_order_cuando_no_vienen_en_el_body()
    {
        // Regresión: is_active pasaba a NULL y la categoría desaparecía de los
        // listados que filtran is_active = TRUE (ver categories.test.ts en Node).
        var id = Guid.NewGuid();
        var existing = new Categoria { Id = id, Nombre = "Vieja", Color = "#222222", Icono = "star", EstaActivo = true, Orden = 5 };
        var repo = new Mock<IRepositorioCategorias>();
        repo.Setup(r => r.ActualizarAsync(id, It.IsAny<Action<Categoria>>()))
            .Returns<Guid, Action<Categoria>>((_, apply) => { apply(existing); return Task.FromResult<Categoria?>(existing); });
        var service = new ServicioCategorias(repo.Object);

        var result = await service.ActualizarAsync(id, new ActualizarCategoriaPeticion { Nombre = "Nueva", Color = "#111111" });

        Assert.Equal("Nueva", result!.Nombre);
        Assert.Equal("#111111", result.Color);
        Assert.Equal("star", result.Icono);
        Assert.True(result.EstaActivo);
        Assert.Equal(5, result.Orden);
    }

    [Fact]
    public async Task Update_permite_desactivar_explicitamente()
    {
        var id = Guid.NewGuid();
        var existing = new Categoria { Id = id, Nombre = "X", Color = "#222222", EstaActivo = true };
        var repo = new Mock<IRepositorioCategorias>();
        repo.Setup(r => r.ActualizarAsync(id, It.IsAny<Action<Categoria>>()))
            .Returns<Guid, Action<Categoria>>((_, apply) => { apply(existing); return Task.FromResult<Categoria?>(existing); });
        var service = new ServicioCategorias(repo.Object);

        var result = await service.ActualizarAsync(id, new ActualizarCategoriaPeticion
        {
            Nombre = "X", Color = "#222222", EstaActivo = false,
        });

        Assert.False(result!.EstaActivo);
    }

    [Fact]
    public async Task Update_retorna_null_si_no_existe()
    {
        var repo = new Mock<IRepositorioCategorias>();
        repo.Setup(r => r.ActualizarAsync(It.IsAny<Guid>(), It.IsAny<Action<Categoria>>()))
            .ReturnsAsync((Categoria?)null);
        var service = new ServicioCategorias(repo.Object);

        var result = await service.ActualizarAsync(Guid.NewGuid(), new ActualizarCategoriaPeticion { Nombre = "X", Color = "#000000" });

        Assert.Null(result);
    }

    [Fact]
    public async Task Delete_hace_soft_delete()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<IRepositorioCategorias>();
        var service = new ServicioCategorias(repo.Object);

        await service.EliminarAsync(id);

        repo.Verify(r => r.DesactivarAsync(id), Times.Once);
    }
}
