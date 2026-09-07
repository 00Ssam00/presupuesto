using Quincena.Domain.Entidades;

namespace Quincena.Application.MetodosPago;

public interface IServicioMetodosPago
{
    Task<List<MetodoPago>> ListarAsync();
}
