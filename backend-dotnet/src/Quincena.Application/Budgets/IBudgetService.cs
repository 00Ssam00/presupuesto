using Quincena.Domain.Entities;

namespace Quincena.Application.Budgets;

public interface IBudgetService
{
    Task<List<BudgetByPeriodItem>> GetByPeriodAsync(Guid periodId);
    Task<Budget> UpsertAsync(UpsertBudgetRequest request);
    Task DeleteAsync(Guid id);
}
