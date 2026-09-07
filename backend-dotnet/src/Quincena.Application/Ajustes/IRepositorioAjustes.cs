using Quincena.Domain.Entidades;

namespace Quincena.Application.Ajustes;

public interface IRepositorioAjustes
{
    Task<List<Ajuste>> ObtenerTodosAsync();

    // Node: INSERT ... ON CONFLICT (key) DO UPDATE. Acá se resuelve con un
    // find-or-add porque cada escritura es un valor puntual poco frecuente,
    // sin necesidad de la atomicidad de bajo nivel que sí justificó el
    // FromSqlInterpolated de Presupuestos.
    Task GuardarAsync(string clave, string valor);
}
