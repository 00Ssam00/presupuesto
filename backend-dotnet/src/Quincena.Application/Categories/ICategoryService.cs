using Quincena.Domain.Entities;

namespace Quincena.Application.Categories;

public interface ICategoryService
{
    Task<List<Category>> ListAsync();
    Task<Category> CreateAsync(CreateCategoryRequest request);
    Task<Category?> UpdateAsync(Guid id, UpdateCategoryRequest request);
    Task DeleteAsync(Guid id);
}
