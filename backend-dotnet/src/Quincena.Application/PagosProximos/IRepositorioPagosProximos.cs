using Quincena.Domain.Entidades;

namespace Quincena.Application.PagosProximos;

public interface IRepositorioPagosProximos
{
    // Node: WHERE dinámico sobre period_id/status, JOIN con categories, ORDER BY due_date ASC.
    Task<List<PagoProximoListaItem>> ListarConCategoriaAsync(Guid? periodoId, string? estado);
    Task<PagoProximo> AgregarAsync(PagoProximo payment);

    // Retorna null si no existe, igual al 404 sin side-effects de Node.
    Task<PagoProximo?> ActualizarAsync(Guid id, Action<PagoProximo> apply);

    // Node hace DELETE físico acá (a diferencia del soft-delete de Goals/Cards/etc.).
    Task EliminarAsync(Guid id);
}
