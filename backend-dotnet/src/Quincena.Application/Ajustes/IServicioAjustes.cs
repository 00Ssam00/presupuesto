namespace Quincena.Application.Ajustes;

public interface IServicioAjustes
{
    // Node: { [key]: value } — un mapa plano, no una lista de filas.
    Task<Dictionary<string, string>> ObtenerTodosAsync();
    Task ActualizarAsync(string clave, string valor);
}
