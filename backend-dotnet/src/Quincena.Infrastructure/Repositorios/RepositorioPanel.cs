using Microsoft.EntityFrameworkCore;
using Quincena.Application.Panel;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Repositorios;

public class RepositorioPanel : IRepositorioPanel
{
    private readonly QuincenaDbContext _db;

    public RepositorioPanel(QuincenaDbContext db)
    {
        _db = db;
    }

    public Task<Periodo?> ObtenerPeriodoAsync(Guid periodId) =>
        _db.Periodos.FirstOrDefaultAsync(p => p.Id == periodId);

    // Dos queries secuenciales (no Task.WhenAll) en vez del único CASE WHEN de Node:
    // un solo DbContext de EF Core no es seguro para operaciones concurrentes — mismo
    // criterio ya aplicado en FuenteDatosArrastre/ServicioTransacciones.ImportarAsync.
    public async Task<(decimal TotalGastos, decimal TotalIngresoExtra)> ObtenerTotalesAsync(Guid periodId)
    {
        var totalExpenses = await _db.Transacciones
            .Where(t => t.PeriodoId == periodId && t.Tipo == "expense")
            .SumAsync(t => (decimal?)t.Monto) ?? 0m;
        var totalIncomeExtra = await _db.Transacciones
            .Where(t => t.PeriodoId == periodId && t.Tipo == "income")
            .SumAsync(t => (decimal?)t.Monto) ?? 0m;
        return (totalExpenses, totalIncomeExtra);
    }

    public Task<List<GastoPorCategoriaItem>> ObtenerGastosPorCategoriaAsync(Guid periodId) =>
        (from c in _db.Categorias
         where c.EstaActivo
         join b in _db.Presupuestos.Where(b => b.PeriodoId == periodId) on c.Id equals b.CategoriaId into budgetJoin
         from b in budgetJoin.DefaultIfEmpty()
         select new GastoPorCategoriaItem
         {
             CategoriaId = c.Id,
             NombreCategoria = c.Nombre,
             ColorCategoria = c.Color,
             Total = _db.Transacciones
                 .Where(t => t.CategoriaId == c.Id && t.PeriodoId == periodId && t.Tipo == "expense")
                 .Sum(t => (decimal?)t.Monto) ?? 0m,
             Presupuesto = b != null ? b.Monto : 0m,
         })
        .OrderByDescending(x => x.Total)
        .ToListAsync();

    public Task<List<PagoProximoConCategoria>> ObtenerPagosProximosAsync(Guid periodId) =>
        (from up in _db.PagosProximos
         join c in _db.Categorias on up.CategoriaId equals c.Id
         where (up.PeriodoId == periodId || up.PeriodoId == null) && up.Estado == "pending"
         orderby up.FechaVencimiento
         select new PagoProximoConCategoria
         {
             Id = up.Id,
             PeriodoId = up.PeriodoId,
             CategoriaId = up.CategoriaId,
             Nombre = up.Nombre,
             Monto = up.Monto,
             FechaVencimiento = up.FechaVencimiento,
             Estado = up.Estado,
             EsRecurrente = up.EsRecurrente,
             IntervaloRecurrencia = up.IntervaloRecurrencia,
             CreadoEn = up.CreadoEn,
             NombreCategoria = c.Nombre,
             ColorCategoria = c.Color,
         })
        .Take(5)
        .ToListAsync();
}
