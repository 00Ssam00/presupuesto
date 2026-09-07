using Quincena.Domain.Entidades;

namespace Quincena.Application.MetodosPago;

// Port de backend/src/controllers/paymentMethodsController.ts (solo lectura,
// no hay create/update/delete en Node para este recurso).
public class ServicioMetodosPago : IServicioMetodosPago
{
    private readonly IRepositorioMetodosPago _repository;

    public ServicioMetodosPago(IRepositorioMetodosPago repository)
    {
        _repository = repository;
    }

    public Task<List<MetodoPago>> ListarAsync() => _repository.ObtenerActivosOrdenadosAsync();
}
