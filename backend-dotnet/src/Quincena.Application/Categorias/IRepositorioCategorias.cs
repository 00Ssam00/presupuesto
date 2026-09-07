using Quincena.Domain.Entidades;

namespace Quincena.Application.Categorias;

public interface IRepositorioCategorias
{
    Task<List<Categoria>> ObtenerActivosOrdenadosAsync();
    Task<Categoria> AgregarAsync(Categoria category);

    // Retorna null si no existe, igual al 404 sin side-effects de Node.
    Task<Categoria?> ActualizarAsync(Guid id, Action<Categoria> apply);

    // No valida existencia previa, igual que el soft-delete de Node (UPDATE sin
    // chequear rowCount, siempre responde 204).
    Task DesactivarAsync(Guid id);
}
