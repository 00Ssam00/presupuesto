using Microsoft.EntityFrameworkCore;
using Quincena.Application.Metas;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Repositorios;

public class RepositorioMetas : IRepositorioMetas
{
    private readonly QuincenaDbContext _db;

    public RepositorioMetas(QuincenaDbContext db)
    {
        _db = db;
    }

    public Task<List<Meta>> ObtenerActivasOrdenadasAsync() =>
        _db.Metas.Where(g => g.EstaActivo).OrderBy(g => g.Tipo).ThenBy(g => g.Nombre).ToListAsync();

    public async Task<Meta> AgregarAsync(Meta goal)
    {
        _db.Metas.Add(goal);
        await _db.SaveChangesAsync();
        return goal;
    }

    public async Task<Meta?> ActualizarAsync(Guid id, Action<Meta> apply)
    {
        var goal = await _db.Metas.FirstOrDefaultAsync(g => g.Id == id);
        if (goal == null) return null;
        apply(goal);
        goal.ActualizadoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return goal;
    }

    public async Task DesactivarAsync(Guid id)
    {
        var goal = await _db.Metas.FirstOrDefaultAsync(g => g.Id == id);
        if (goal == null) return;
        goal.EstaActivo = false;
        await _db.SaveChangesAsync();
    }
}
