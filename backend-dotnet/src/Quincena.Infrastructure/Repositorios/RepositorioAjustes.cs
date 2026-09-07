using Microsoft.EntityFrameworkCore;
using Quincena.Application.Ajustes;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Repositorios;

public class RepositorioAjustes : IRepositorioAjustes
{
    private readonly QuincenaDbContext _db;

    public RepositorioAjustes(QuincenaDbContext db)
    {
        _db = db;
    }

    public Task<List<Ajuste>> ObtenerTodosAsync() => _db.Ajustes.ToListAsync();

    public async Task GuardarAsync(string clave, string valor)
    {
        var ajuste = await _db.Ajustes.FirstOrDefaultAsync(a => a.Clave == clave);
        if (ajuste == null)
        {
            _db.Ajustes.Add(new Ajuste { Clave = clave, Valor = valor, ActualizadoEn = DateTime.UtcNow });
        }
        else
        {
            ajuste.Valor = valor;
            ajuste.ActualizadoEn = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
    }
}
