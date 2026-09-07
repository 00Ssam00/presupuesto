using Microsoft.EntityFrameworkCore;
using Quincena.Application.Tarjetas;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Repositorios;

public class RepositorioTarjetas : IRepositorioTarjetas
{
    private readonly QuincenaDbContext _db;

    public RepositorioTarjetas(QuincenaDbContext db)
    {
        _db = db;
    }

    public Task<List<Tarjeta>> ObtenerActivosOrdenadosAsync() =>
        _db.Tarjetas.Where(c => c.EstaActivo).OrderBy(c => c.Nombre).ToListAsync();

    public async Task<Tarjeta> AgregarAsync(Tarjeta card)
    {
        _db.Tarjetas.Add(card);
        await _db.SaveChangesAsync();
        return card;
    }

    public async Task<Tarjeta?> ActualizarAsync(Guid id, Action<Tarjeta> apply)
    {
        var card = await _db.Tarjetas.FirstOrDefaultAsync(c => c.Id == id);
        if (card == null) return null;
        apply(card);
        await _db.SaveChangesAsync();
        return card;
    }

    public async Task DesactivarAsync(Guid id)
    {
        var card = await _db.Tarjetas.FirstOrDefaultAsync(c => c.Id == id);
        if (card == null) return;
        card.EstaActivo = false;
        await _db.SaveChangesAsync();
    }
}
