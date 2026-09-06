using Quincena.Domain.Entities;

namespace Quincena.Application.Dashboard;

public interface IDashboardRepository
{
    Task<Period?> GetPeriodAsync(Guid periodId);
    Task<(decimal TotalExpenses, decimal TotalIncomeExtra)> GetTotalsAsync(Guid periodId);
    Task<List<ExpenseByCategoryItem>> GetExpensesByCategoryAsync(Guid periodId);
    Task<List<UpcomingPaymentWithCategory>> GetUpcomingPaymentsAsync(Guid periodId);
}
