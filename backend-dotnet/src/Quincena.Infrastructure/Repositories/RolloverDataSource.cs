using Microsoft.EntityFrameworkCore;
using Quincena.Application.Rollover;

namespace Quincena.Infrastructure.Repositories;

public class RolloverDataSource : IRolloverDataSource
{
    private readonly QuincenaDbContext _db;

    public RolloverDataSource(QuincenaDbContext db)
    {
        _db = db;
    }

    public Task<List<PeriodRolloverRow>> GetPeriodTotalsAsync()
    {
        return _db.Periods
            .OrderBy(p => p.StartDate)
            .Select(p => new PeriodRolloverRow(
                p.Id,
                p.StartDate,
                p.BaseIncome,
                p.Rollover,
                _db.Transactions.Where(t => t.PeriodId == p.Id && t.Type == "expense").Sum(t => (decimal?)t.Amount) ?? 0m,
                _db.Transactions.Where(t => t.PeriodId == p.Id && t.Type == "income").Sum(t => (decimal?)t.Amount) ?? 0m))
            .ToListAsync();
    }

    public async Task UpdateRolloversAsync(IReadOnlyList<RolloverUpdate> updates)
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
                $"UPDATE periods SET rollover = {update.Rollover} WHERE id = {update.PeriodId}");
        }
    }
}
