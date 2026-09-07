using Quincena.Domain.Entidades;

namespace Quincena.Application.PagosProximos;

public interface IServicioPagosProximos
{
    Task<List<PagoProximoListaItem>> ListarAsync(Guid? periodoId, string? estado);
    Task<PagoProximo> CrearAsync(CrearPagoProximoPeticion request);
    Task<PagoProximo?> ActualizarAsync(Guid id, ActualizarPagoProximoPeticion request);
    Task EliminarAsync(Guid id);
}
