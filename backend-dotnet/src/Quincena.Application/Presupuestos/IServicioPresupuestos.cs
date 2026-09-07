using Quincena.Domain.Entidades;

namespace Quincena.Application.Presupuestos;

public interface IServicioPresupuestos
{
    Task<List<PresupuestoPorPeriodoItem>> ObtenerPorPeriodoAsync(Guid periodId);
    Task<Presupuesto> GuardarAsync(GuardarPresupuestoPeticion request);
    Task EliminarAsync(Guid id);
}
