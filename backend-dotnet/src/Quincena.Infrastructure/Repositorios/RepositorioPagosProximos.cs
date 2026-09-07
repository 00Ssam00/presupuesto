using Microsoft.EntityFrameworkCore;
using Quincena.Application.PagosProximos;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Repositorios;

public class RepositorioPagosProximos : IRepositorioPagosProximos
{
    private readonly QuincenaDbContext _db;

    public RepositorioPagosProximos(QuincenaDbContext db)
    {
        _db = db;
    }

    public Task<List<PagoProximoListaItem>> ListarConCategoriaAsync(Guid? periodoId, string? estado) =>
        (from up in _db.PagosProximos
         join c in _db.Categorias on up.CategoriaId equals c.Id
         where (periodoId == null || up.PeriodoId == periodoId) && (estado == null || up.Estado == estado)
         orderby up.FechaVencimiento
         select new PagoProximoListaItem
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
        .ToListAsync();

    public async Task<PagoProximo> AgregarAsync(PagoProximo payment)
    {
        _db.PagosProximos.Add(payment);
        await _db.SaveChangesAsync();
        return payment;
    }

    public async Task<PagoProximo?> ActualizarAsync(Guid id, Action<PagoProximo> apply)
    {
        var payment = await _db.PagosProximos.FirstOrDefaultAsync(p => p.Id == id);
        if (payment == null) return null;
        apply(payment);
        await _db.SaveChangesAsync();
        return payment;
    }

    public async Task EliminarAsync(Guid id)
    {
        var payment = await _db.PagosProximos.FirstOrDefaultAsync(p => p.Id == id);
        if (payment == null) return;
        _db.PagosProximos.Remove(payment);
        await _db.SaveChangesAsync();
    }
}
