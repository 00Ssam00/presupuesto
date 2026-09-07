using Quincena.Domain.Entidades;

namespace Quincena.Application.Presupuestos;

// Port de backend/src/controllers/budgetsController.ts (delgado — no hay
// orquestación adicional, a diferencia de Periodos/Transacciones que recalculan
// rollovers).
public class ServicioPresupuestos : IServicioPresupuestos
{
    private readonly IRepositorioPresupuestos _repository;

    public ServicioPresupuestos(IRepositorioPresupuestos repository)
    {
        _repository = repository;
    }

    public Task<List<PresupuestoPorPeriodoItem>> ObtenerPorPeriodoAsync(Guid periodId) => _repository.ObtenerPorPeriodoAsync(periodId);

    public Task<Presupuesto> GuardarAsync(GuardarPresupuestoPeticion request) => _repository.GuardarAsync(request);

    public Task EliminarAsync(Guid id) => _repository.EliminarAsync(id);
}
