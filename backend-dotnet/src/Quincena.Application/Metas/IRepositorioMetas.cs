using Quincena.Domain.Entidades;

namespace Quincena.Application.Metas;

public interface IRepositorioMetas
{
    // Node: SELECT * FROM goals WHERE is_active = TRUE ORDER BY type, name.
    Task<List<Meta>> ObtenerActivasOrdenadasAsync();
    Task<Meta> AgregarAsync(Meta goal);

    // Retorna null si no existe, igual al 404 sin side-effects de Node.
    Task<Meta?> ActualizarAsync(Guid id, Action<Meta> apply);

    // No valida existencia previa, igual que el soft-delete de Node.
    Task DesactivarAsync(Guid id);
}
