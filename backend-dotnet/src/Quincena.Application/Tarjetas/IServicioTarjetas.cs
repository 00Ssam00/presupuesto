using Quincena.Domain.Entidades;

namespace Quincena.Application.Tarjetas;

public interface IServicioTarjetas
{
    Task<List<Tarjeta>> ListarAsync();
    Task<Tarjeta> CrearAsync(CrearTarjetaPeticion request);
    Task<Tarjeta?> ActualizarAsync(Guid id, ActualizarTarjetaPeticion request);
    Task EliminarAsync(Guid id);
}
