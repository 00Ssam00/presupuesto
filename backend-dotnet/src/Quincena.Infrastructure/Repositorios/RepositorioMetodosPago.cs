using Microsoft.EntityFrameworkCore;
using Quincena.Application.MetodosPago;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Repositorios;

public class RepositorioMetodosPago : IRepositorioMetodosPago
{
    private readonly QuincenaDbContext _db;

    public RepositorioMetodosPago(QuincenaDbContext db)
    {
        _db = db;
    }

    public Task<List<MetodoPago>> ObtenerActivosOrdenadosAsync() =>
        _db.MetodosPago.Where(p => p.EstaActivo).OrderBy(p => p.Nombre).ToListAsync();
}
