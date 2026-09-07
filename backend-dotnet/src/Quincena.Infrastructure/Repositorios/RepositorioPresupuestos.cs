using Microsoft.EntityFrameworkCore;
using Quincena.Application.Presupuestos;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Repositorios;

public class RepositorioPresupuestos : IRepositorioPresupuestos
{
    private readonly QuincenaDbContext _db;

    public RepositorioPresupuestos(QuincenaDbContext db)
    {
        _db = db;
    }

    // Una fila por CADA categoría (sin filtrar is_active, igual que Node) con LEFT
    // JOIN al presupuesto de esa quincena y el gasto ya sumado — no hay navigation
    // properties entre Categoria/Presupuesto/Transaccion, así que se arma a mano.
    public Task<List<PresupuestoPorPeriodoItem>> ObtenerPorPeriodoAsync(Guid periodId) =>
        (from c in _db.Categorias
         join b in _db.Presupuestos.Where(b => b.PeriodoId == periodId) on c.Id equals b.CategoriaId into budgetJoin
         from b in budgetJoin.DefaultIfEmpty()
         orderby c.Orden
         select new PresupuestoPorPeriodoItem
         {
             Id = b != null ? b.Id : (Guid?)null,
             PeriodoId = periodId,
             CategoriaId = c.Id,
             Monto = b != null ? b.Monto : 0m,
             Porcentaje = b != null ? b.Porcentaje : null,
             NombreCategoria = c.Nombre,
             ColorCategoria = c.Color,
             Gastado = _db.Transacciones
                 .Where(t => t.CategoriaId == c.Id && t.PeriodoId == periodId && t.Tipo == "expense")
                 .Sum(t => (decimal?)t.Monto) ?? 0m,
         }).ToListAsync();

    public async Task<Presupuesto> GuardarAsync(GuardarPresupuestoPeticion request)
    {
        var results = await _db.Presupuestos.FromSqlInterpolated($@"
            INSERT INTO budgets (period_id, category_id, amount, percentage)
            VALUES ({request.PeriodoId}, {request.CategoriaId}, {request.Monto}, {request.Porcentaje})
            ON CONFLICT (period_id, category_id)
            DO UPDATE SET amount = {request.Monto}, percentage = {request.Porcentaje}
            RETURNING *
        ").ToListAsync();
        return results[0];
    }

    public async Task EliminarAsync(Guid id)
    {
        var budget = await _db.Presupuestos.FirstOrDefaultAsync(b => b.Id == id);
        if (budget == null) return;
        _db.Presupuestos.Remove(budget);
        await _db.SaveChangesAsync();
    }
}
