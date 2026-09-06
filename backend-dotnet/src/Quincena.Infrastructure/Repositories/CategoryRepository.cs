using Microsoft.EntityFrameworkCore;
using Quincena.Application.Categories;
using Quincena.Domain.Entities;

namespace Quincena.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly QuincenaDbContext _db;

    public CategoryRepository(QuincenaDbContext db)
    {
        _db = db;
    }

    public Task<List<Category>> GetActiveOrderedAsync() =>
        _db.Categories.Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .ToListAsync();

    public async Task<Category> AddAsync(Category category)
    {
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> UpdateAsync(Guid id, Action<Category> apply)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return null;
        apply(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task DeactivateAsync(Guid id)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return;
        category.IsActive = false;
        await _db.SaveChangesAsync();
    }
}
