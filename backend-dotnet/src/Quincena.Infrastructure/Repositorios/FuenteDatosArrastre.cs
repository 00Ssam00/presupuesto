using Microsoft.EntityFrameworkCore;
using Quincena.Application.Arrastre;

namespace Quincena.Infrastructure.Repositorios;

public class FuenteDatosArrastre : IFuenteDatosArrastre
{
    private readonly QuincenaDbContext _db;

    public FuenteDatosArrastre(QuincenaDbContext db)
    {
        _db = db;
    }

    public Task<List<FilaArrastrePeriodo>> ObtenerTotalesPeriodosAsync()
    {
        return _db.Periodos
            .OrderBy(p => p.FechaInicio)
            .Select(p => new FilaArrastrePeriodo(
                p.Id,
                p.FechaInicio,
                p.IngresoBase,
                p.Arrastre,
                _db.Transacciones.Where(t => t.PeriodoId == p.Id && t.Tipo == "expense").Sum(t => (decimal?)t.Monto) ?? 0m,
                _db.Transacciones.Where(t => t.PeriodoId == p.Id && t.Tipo == "income").Sum(t => (decimal?)t.Monto) ?? 0m))
            .ToListAsync();
    }

    public async Task ActualizarArrastresAsync(IReadOnlyList<ActualizacionArrastre> updates)
    {
        // Cada UPDATE es independiente, igual que el Promise.all sin transacción de Node.
        // Se ejecutan secuencialmente (no en paralelo): un único DbContext/conexión no es
        // thread-safe para operaciones concurrentes, así que paralelizar aquí introduciría
        // un bug real que Node no tiene. Lo que importa replicar es la falta de atomicidad
        // (cada update puede fallar independientemente), no la concurrencia literal a nivel
        // de red.
        foreach (var update in updates)
        {
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE periods SET rollover = {update.Arrastre} WHERE id = {update.PeriodoId}");
        }
    }
}
