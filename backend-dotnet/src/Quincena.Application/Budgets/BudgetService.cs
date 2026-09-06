using Quincena.Domain.Entities;

namespace Quincena.Application.Budgets;

// Port de backend/src/controllers/budgetsController.ts (delgado — no hay
// orquestación adicional, a diferencia de Periods/Transactions que recalculan
// rollovers).
public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _repository;

    public BudgetService(IBudgetRepository repository)
    {
        _repository = repository;
    }

    public Task<List<BudgetByPeriodItem>> GetByPeriodAsync(Guid periodId) => _repository.GetByPeriodAsync(periodId);

    public Task<Budget> UpsertAsync(UpsertBudgetRequest request) => _repository.UpsertAsync(request);

    public Task DeleteAsync(Guid id) => _repository.DeleteAsync(id);
}
