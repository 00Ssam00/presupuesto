using Quincena.Domain.Entidades;

namespace Quincena.Application.Periodos;

public interface IRepositorioPeriodos
{
    Task<List<Periodo>> ObtenerTodosOrdenadosPorFechaInicioDescAsync();
    Task<Periodo?> ObtenerPorIdAsync(Guid id);

    // Busca el período cuyo rango [start_date, end_date] cubre `today`; si ninguno
    // cubre esa fecha, cae al más reciente por start_date; si no hay ninguno, null.
    Task<Periodo?> ObtenerVigenteOMasRecienteAsync(DateOnly today);

    Task<Periodo> AgregarAsync(Periodo period);

    // Retorna false si no existe (sin aplicar `apply`), igual al 404 sin side-effects de Node.
    Task<bool> ActualizarAsync(Guid id, Action<Periodo> apply);

    Task<bool> EliminarAsync(Guid id);
}
