using Quincena.Domain.Entities;

namespace Quincena.Application.Budgets;

public interface IBudgetRepository
{
    Task<List<BudgetByPeriodItem>> GetByPeriodAsync(Guid periodId);

    // INSERT ... ON CONFLICT (period_id, category_id) DO UPDATE — atómico,
    // igual que Node. No se modela como find-then-update porque una condición de
    // carrera entre el find y el update rompería esa atomicidad.
    Task<Budget> UpsertAsync(UpsertBudgetRequest request);

    // No valida existencia previa, igual que Node.
    Task DeleteAsync(Guid id);
}
