using Microsoft.EntityFrameworkCore;
using Quincena.Application.Transacciones;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Repositorios;

// Los entities Transaccion/Categoria/MetodoPago/Tarjeta no tienen propiedades de
// navegación (ver sus Configuration), así que los JOIN de SELECT_WITH_JOINS de
// Node se replican acá como joins LINQ manuales sobre las FKs, no con Include().
public class RepositorioTransacciones : IRepositorioTransacciones
{
    private readonly QuincenaDbContext _db;

    public RepositorioTransacciones(QuincenaDbContext db)
    {
        _db = db;
    }

    private IQueryable<TransaccionListaItem> JoinedQuery() =>
        from t in _db.Transacciones
        join c in _db.Categorias on t.CategoriaId equals c.Id
        join pm in _db.MetodosPago on t.MetodoPagoId equals pm.Id
        join ca in _db.Tarjetas on t.TarjetaId equals ca.Id into cardJoin
        from ca in cardJoin.DefaultIfEmpty()
        select new TransaccionListaItem
        {
            Id = t.Id,
            PeriodoId = t.PeriodoId,
            CategoriaId = t.CategoriaId,
            MetodoPagoId = t.MetodoPagoId,
            TarjetaId = t.TarjetaId,
            Tipo = t.Tipo,
            Monto = t.Monto,
            Descripcion = t.Descripcion,
            Nota = t.Nota,
            Fecha = t.Fecha,
            CreadoEn = t.CreadoEn,
            ActualizadoEn = t.ActualizadoEn,
            NombreCategoria = c.Nombre,
            ColorCategoria = c.Color,
            NombreMetodoPago = pm.Nombre,
            NombreTarjeta = ca != null ? ca.Nombre : null,
        };

    public async Task<List<TransaccionListaItem>> ListarAsync(Guid? periodId, string? type, Guid? categoryId)
    {
        var query = JoinedQuery();
        if (periodId.HasValue) query = query.Where(x => x.PeriodoId == periodId.Value);
        if (!string.IsNullOrEmpty(type)) query = query.Where(x => x.Tipo == type);
        if (categoryId.HasValue) query = query.Where(x => x.CategoriaId == categoryId.Value);

        return await query
            .OrderByDescending(x => x.Fecha).ThenByDescending(x => x.CreadoEn)
            .ToListAsync();
    }

    public Task<TransaccionListaItem?> ObtenerPorIdConJoinsAsync(Guid id) =>
        JoinedQuery().FirstOrDefaultAsync(x => x.Id == id);

    public async Task<Transaccion> AgregarAsync(Transaccion transaction)
    {
        _db.Transacciones.Add(transaction);
        await _db.SaveChangesAsync();
        return transaction;
    }

    public async Task<Transaccion?> ActualizarAsync(Guid id, Action<Transaccion> apply)
    {
        var transaction = await _db.Transacciones.FirstOrDefaultAsync(t => t.Id == id);
        if (transaction == null) return null;
        apply(transaction);
        await _db.SaveChangesAsync();
        return transaction;
    }

    public async Task EliminarAsync(Guid id)
    {
        var transaction = await _db.Transacciones.FirstOrDefaultAsync(t => t.Id == id);
        if (transaction == null) return;
        _db.Transacciones.Remove(transaction);
        await _db.SaveChangesAsync();
    }
}
