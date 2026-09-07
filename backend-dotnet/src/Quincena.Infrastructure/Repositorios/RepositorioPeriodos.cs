using Microsoft.EntityFrameworkCore;
using Quincena.Application.Periodos;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Repositorios;

public class RepositorioPeriodos : IRepositorioPeriodos
{
    private readonly QuincenaDbContext _db;

    public RepositorioPeriodos(QuincenaDbContext db)
    {
        _db = db;
    }

    public Task<List<Periodo>> ObtenerTodosOrdenadosPorFechaInicioDescAsync() =>
        _db.Periodos.OrderByDescending(p => p.FechaInicio).ToListAsync();

    public Task<Periodo?> ObtenerPorIdAsync(Guid id) =>
        _db.Periodos.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Periodo?> ObtenerVigenteOMasRecienteAsync(DateOnly today)
    {
        var covering = await _db.Periodos
            .FirstOrDefaultAsync(p => p.FechaInicio <= today && p.FechaFin >= today);
        if (covering != null) return covering;

        return await _db.Periodos
            .OrderByDescending(p => p.FechaInicio)
            .FirstOrDefaultAsync();
    }

    public async Task<Periodo> AgregarAsync(Periodo period)
    {
        _db.Periodos.Add(period);
        await _db.SaveChangesAsync();
        return period;
    }

    public async Task<bool> ActualizarAsync(Guid id, Action<Periodo> apply)
    {
        var period = await _db.Periodos.FirstOrDefaultAsync(p => p.Id == id);
        if (period == null) return false;
        apply(period);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        var period = await _db.Periodos.FirstOrDefaultAsync(p => p.Id == id);
        if (period == null) return false;
        _db.Periodos.Remove(period);
        await _db.SaveChangesAsync();
        return true;
    }
}
