using Quincena.Domain.Entidades;

namespace Quincena.Application.Presupuestos;

public interface IRepositorioPresupuestos
{
    Task<List<PresupuestoPorPeriodoItem>> ObtenerPorPeriodoAsync(Guid periodId);

    // INSERT ... ON CONFLICT (period_id, category_id) DO UPDATE — atómico,
    // igual que Node. No se modela como find-then-update porque una condición de
    // carrera entre el find y el update rompería esa atomicidad.
    Task<Presupuesto> GuardarAsync(GuardarPresupuestoPeticion request);

    // No valida existencia previa, igual que Node.
    Task EliminarAsync(Guid id);
}
