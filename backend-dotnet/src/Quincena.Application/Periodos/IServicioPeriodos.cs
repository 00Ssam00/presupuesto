using Quincena.Domain.Entidades;

namespace Quincena.Application.Periodos;

public interface IServicioPeriodos
{
    Task<List<Periodo>> ListarAsync();
    Task<Periodo?> ObtenerActualAsync();
    Task<Periodo?> ObtenerPorIdAsync(Guid id);
    Task<Periodo> CrearAsync(CrearPeriodoPeticion request);
    Task<Periodo?> ActualizarAsync(Guid id, ActualizarPeriodoPeticion request);
    Task<bool> EliminarAsync(Guid id);
}
