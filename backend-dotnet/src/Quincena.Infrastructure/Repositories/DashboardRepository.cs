using Microsoft.EntityFrameworkCore;
using Quincena.Application.Dashboard;
using Quincena.Domain.Entities;

namespace Quincena.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly QuincenaDbContext _db;

    public DashboardRepository(QuincenaDbContext db)
    {
        _db = db;
    }

    public Task<Period?> GetPeriodAsync(Guid periodId) =>
        _db.Periods.FirstOrDefaultAsync(p => p.Id == periodId);

    // Dos queries secuenciales (no Task.WhenAll) en vez del único CASE WHEN de Node:
    // un solo DbContext de EF Core no es seguro para operaciones concurrentes — mismo
    // criterio ya aplicado en RolloverDataSource/TransactionService.ImportAsync.
    public async Task<(decimal TotalExpenses, decimal TotalIncomeExtra)> GetTotalsAsync(Guid periodId)
    {
        var totalExpenses = await _db.Transactions
            .Where(t => t.PeriodId == periodId && t.Type == "expense")
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;
        var totalIncomeExtra = await _db.Transactions
            .Where(t => t.PeriodId == periodId && t.Type == "income")
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;
        return (totalExpenses, totalIncomeExtra);
    }

    public Task<List<ExpenseByCategoryItem>> GetExpensesByCategoryAsync(Guid periodId) =>
        (from c in _db.Categories
         where c.IsActive
         join b in _db.Budgets.Where(b => b.PeriodId == periodId) on c.Id equals b.CategoryId into budgetJoin
         from b in budgetJoin.DefaultIfEmpty()
         select new ExpenseByCategoryItem
         {
             CategoryId = c.Id,
             CategoryName = c.Name,
             CategoryColor = c.Color,
             Total = _db.Transactions
                 .Where(t => t.CategoryId == c.Id && t.PeriodId == periodId && t.Type == "expense")
                 .Sum(t => (decimal?)t.Amount) ?? 0m,
             Budget = b != null ? b.Amount : 0m,
         })
        .OrderByDescending(x => x.Total)
        .ToListAsync();

    public Task<List<UpcomingPaymentWithCategory>> GetUpcomingPaymentsAsync(Guid periodId) =>
        (from up in _db.UpcomingPayments
         join c in _db.Categories on up.CategoryId equals c.Id
         where (up.PeriodId == periodId || up.PeriodId == null) && up.Status == "pending"
         orderby up.DueDate
         select new UpcomingPaymentWithCategory
         {
             Id = up.Id,
             PeriodId = up.PeriodId,
             CategoryId = up.CategoryId,
             Name = up.Name,
             Amount = up.Amount,
             DueDate = up.DueDate,
             Status = up.Status,
             IsRecurring = up.IsRecurring,
             RecurrenceInterval = up.RecurrenceInterval,
             CreatedAt = up.CreatedAt,
             CategoryName = c.Name,
             CategoryColor = c.Color,
         })
        .Take(5)
        .ToListAsync();
}
