using Quincena.Domain.Entities;

namespace Quincena.Application.Categories;

public interface ICategoryRepository
{
    Task<List<Category>> GetActiveOrderedAsync();
    Task<Category> AddAsync(Category category);

    // Retorna null si no existe, igual al 404 sin side-effects de Node.
    Task<Category?> UpdateAsync(Guid id, Action<Category> apply);

    // No valida existencia previa, igual que el soft-delete de Node (UPDATE sin
    // chequear rowCount, siempre responde 204).
    Task DeactivateAsync(Guid id);
}
