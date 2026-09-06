using Quincena.Domain.Entities;

namespace Quincena.Application.Periods;

public interface IPeriodRepository
{
    Task<List<Period>> GetAllOrderByStartDateDescAsync();
    Task<Period?> GetByIdAsync(Guid id);

    // Busca el período cuyo rango [start_date, end_date] cubre `today`; si ninguno
    // cubre esa fecha, cae al más reciente por start_date; si no hay ninguno, null.
    Task<Period?> GetCoveringOrLatestAsync(DateOnly today);

    Task<Period> AddAsync(Period period);

    // Retorna false si no existe (sin aplicar `apply`), igual al 404 sin side-effects de Node.
    Task<bool> UpdateAsync(Guid id, Action<Period> apply);

    Task<bool> DeleteAsync(Guid id);
}
