using Microsoft.EntityFrameworkCore;
using Quincena.Application.Categorias;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Repositorios;

public class RepositorioCategorias : IRepositorioCategorias
{
    private readonly QuincenaDbContext _db;

    public RepositorioCategorias(QuincenaDbContext db)
    {
        _db = db;
    }

    public Task<List<Categoria>> ObtenerActivosOrdenadosAsync() =>
        _db.Categorias.Where(c => c.EstaActivo)
            .OrderBy(c => c.Orden).ThenBy(c => c.Nombre)
            .ToListAsync();

    public async Task<Categoria> AgregarAsync(Categoria category)
    {
        _db.Categorias.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task<Categoria?> ActualizarAsync(Guid id, Action<Categoria> apply)
    {
        var category = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return null;
        apply(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task DesactivarAsync(Guid id)
    {
        var category = await _db.Categorias.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null) return;
        category.EstaActivo = false;
        await _db.SaveChangesAsync();
    }
}
