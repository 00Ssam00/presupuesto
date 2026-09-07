using Quincena.Domain.Entidades;

namespace Quincena.Application.Categorias;

public interface IServicioCategorias
{
    Task<List<Categoria>> ListarAsync();
    Task<Categoria> CrearAsync(CrearCategoriaPeticion request);
    Task<Categoria?> ActualizarAsync(Guid id, ActualizarCategoriaPeticion request);
    Task EliminarAsync(Guid id);
}
