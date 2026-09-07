using Quincena.Domain.Entidades;

namespace Quincena.Application.MetodosPago;

public interface IRepositorioMetodosPago
{
    Task<List<MetodoPago>> ObtenerActivosOrdenadosAsync();
}
