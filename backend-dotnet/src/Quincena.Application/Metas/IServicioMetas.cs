using Quincena.Domain.Entidades;

namespace Quincena.Application.Metas;

public interface IServicioMetas
{
    Task<List<Meta>> ListarAsync();
    Task<Meta> CrearAsync(CrearMetaPeticion request);
    Task<Meta?> ActualizarAsync(Guid id, ActualizarMetaPeticion request);
    Task EliminarAsync(Guid id);
}
