using Moq;
using Quincena.Application.Categories;
using Quincena.Domain.Entities;
using Xunit;

namespace Quincena.UnitTests.Categories;

// Port de backend/src/__tests__/categories.test.ts.
public class CategoryServiceTests
{
    [Fact]
    public async Task Create_aplica_defaults_cuando_color_icon_y_sort_order_no_vienen()
    {
        var repo = new Mock<ICategoryRepository>();
        Category? added = null;
        repo.Setup(r => r.AddAsync(It.IsAny<Category>()))
            .Callback<Category>(c => added = c)
            .ReturnsAsync((Category c) => c);
        var service = new CategoryService(repo.Object);

        await service.CreateAsync(new CreateCategoryRequest { Name = "Ocio" });

        Assert.Equal("#01696f", added!.Color);
        Assert.Null(added.Icon);
        Assert.Equal(0, added.SortOrder);
    }

    [Fact]
    public async Task Create_conserva_valores_explicitos()
    {
        var repo = new Mock<ICategoryRepository>();
        Category? added = null;
        repo.Setup(r => r.AddAsync(It.IsAny<Category>()))
            .Callback<Category>(c => added = c)
            .ReturnsAsync((Category c) => c);
        var service = new CategoryService(repo.Object);

        await service.CreateAsync(new CreateCategoryRequest
        {
            Name = "Ocio", Color = "#111111", Icon = "smile", SortOrder = 3,
        });

        Assert.Equal("#111111", added!.Color);
        Assert.Equal("smile", added.Icon);
        Assert.Equal(3, added.SortOrder);
    }

    [Fact]
    public async Task Update_no_borra_icon_is_active_ni_sort_order_cuando_no_vienen_en_el_body()
    {
        // Regresión: is_active pasaba a NULL y la categoría desaparecía de los
        // listados que filtran is_active = TRUE (ver categories.test.ts en Node).
        var id = Guid.NewGuid();
        var existing = new Category { Id = id, Name = "Vieja", Color = "#222222", Icon = "star", IsActive = true, SortOrder = 5 };
        var repo = new Mock<ICategoryRepository>();
        repo.Setup(r => r.UpdateAsync(id, It.IsAny<Action<Category>>()))
            .Returns<Guid, Action<Category>>((_, apply) => { apply(existing); return Task.FromResult<Category?>(existing); });
        var service = new CategoryService(repo.Object);

        var result = await service.UpdateAsync(id, new UpdateCategoryRequest { Name = "Nueva", Color = "#111111" });

        Assert.Equal("Nueva", result!.Name);
        Assert.Equal("#111111", result.Color);
        Assert.Equal("star", result.Icon);
        Assert.True(result.IsActive);
        Assert.Equal(5, result.SortOrder);
    }

    [Fact]
    public async Task Update_permite_desactivar_explicitamente()
    {
        var id = Guid.NewGuid();
        var existing = new Category { Id = id, Name = "X", Color = "#222222", IsActive = true };
        var repo = new Mock<ICategoryRepository>();
        repo.Setup(r => r.UpdateAsync(id, It.IsAny<Action<Category>>()))
            .Returns<Guid, Action<Category>>((_, apply) => { apply(existing); return Task.FromResult<Category?>(existing); });
        var service = new CategoryService(repo.Object);

        var result = await service.UpdateAsync(id, new UpdateCategoryRequest
        {
            Name = "X", Color = "#222222", IsActive = false,
        });

        Assert.False(result!.IsActive);
    }

    [Fact]
    public async Task Update_retorna_null_si_no_existe()
    {
        var repo = new Mock<ICategoryRepository>();
        repo.Setup(r => r.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Action<Category>>()))
            .ReturnsAsync((Category?)null);
        var service = new CategoryService(repo.Object);

        var result = await service.UpdateAsync(Guid.NewGuid(), new UpdateCategoryRequest { Name = "X", Color = "#000000" });

        Assert.Null(result);
    }

    [Fact]
    public async Task Delete_hace_soft_delete()
    {
        var id = Guid.NewGuid();
        var repo = new Mock<ICategoryRepository>();
        var service = new CategoryService(repo.Object);

        await service.DeleteAsync(id);

        repo.Verify(r => r.DeactivateAsync(id), Times.Once);
    }
}
