using Quincena.Domain.Entidades;

namespace Quincena.Application.Tarjetas;

public interface IRepositorioTarjetas
{
    Task<List<Tarjeta>> ObtenerActivosOrdenadosAsync();
    Task<Tarjeta> AgregarAsync(Tarjeta card);

    // Retorna null si no existe, igual al 404 sin side-effects de Node.
    Task<Tarjeta?> ActualizarAsync(Guid id, Action<Tarjeta> apply);

    // No valida existencia previa, igual que el soft-delete de Node.
    Task DesactivarAsync(Guid id);
}
