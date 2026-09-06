using Quincena.Domain.Entities;

namespace Quincena.Application.Categories;

// Port de backend/src/controllers/categoriesController.ts.
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public Task<List<Category>> ListAsync() => _repository.GetActiveOrderedAsync();

    public Task<Category> CreateAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
            Color = string.IsNullOrEmpty(request.Color) ? "#01696f" : request.Color,
            Icon = string.IsNullOrEmpty(request.Icon) ? null : request.Icon,
            SortOrder = request.SortOrder ?? 0,
        };
        return _repository.AddAsync(category);
    }

    public Task<Category?> UpdateAsync(Guid id, UpdateCategoryRequest request) =>
        _repository.UpdateAsync(id, category =>
        {
            category.Name = request.Name;
            category.Color = request.Color;
            // COALESCE: solo se pisa si el campo vino en el body (no null); si no
            // vino, se conserva el valor actual — así una edición que solo manda
            // name/color no borra icon/is_active/sort_order.
            if (request.Icon != null) category.Icon = request.Icon;
            if (request.IsActive.HasValue) category.IsActive = request.IsActive.Value;
            if (request.SortOrder.HasValue) category.SortOrder = request.SortOrder.Value;
        });

    public Task DeleteAsync(Guid id) => _repository.DeactivateAsync(id);
}
