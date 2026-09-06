namespace Quincena.Application.Dashboard;

// Port de backend/src/controllers/dashboardController.ts. Único endpoint de la app
// donde available/remaining/total_expenses/total_income_extra salen como number
// JSON plano en vez de string — ver el comentario en DashboardModels.cs.
public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;

    public DashboardService(IDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<DashboardResult?> GetAsync(Guid periodId)
    {
        var period = await _repository.GetPeriodAsync(periodId);
        if (period == null) return null;

        var (totalExpenses, totalIncomeExtra) = await _repository.GetTotalsAsync(periodId);
        var expensesByCategory = await _repository.GetExpensesByCategoryAsync(periodId);
        var upcomingPayments = await _repository.GetUpcomingPaymentsAsync(periodId);

        var available = (double)period.BaseIncome + (double)period.Rollover + (double)totalIncomeExtra;
        var remaining = available - (double)totalExpenses;

        return new DashboardResult
        {
            Period = period,
            Available = available,
            TotalExpenses = (double)totalExpenses,
            TotalIncomeExtra = (double)totalIncomeExtra,
            Remaining = remaining,
            ExpensesByCategory = expensesByCategory,
            UpcomingPayments = upcomingPayments,
        };
    }
}
