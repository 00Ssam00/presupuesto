using Microsoft.EntityFrameworkCore;
using Quincena.Application.Budgets;
using Quincena.Domain.Entities;

namespace Quincena.Infrastructure.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly QuincenaDbContext _db;

    public BudgetRepository(QuincenaDbContext db)
    {
        _db = db;
    }

    // Una fila por CADA categoría (sin filtrar is_active, igual que Node) con LEFT
    // JOIN al presupuesto de esa quincena y el gasto ya sumado — no hay navigation
    // properties entre Category/Budget/Transaction, así que se arma a mano.
    public Task<List<BudgetByPeriodItem>> GetByPeriodAsync(Guid periodId) =>
        (from c in _db.Categories
         join b in _db.Budgets.Where(b => b.PeriodId == periodId) on c.Id equals b.CategoryId into budgetJoin
         from b in budgetJoin.DefaultIfEmpty()
         orderby c.SortOrder
         select new BudgetByPeriodItem
         {
             Id = b != null ? b.Id : (Guid?)null,
             PeriodId = periodId,
             CategoryId = c.Id,
             Amount = b != null ? b.Amount : 0m,
             Percentage = b != null ? b.Percentage : null,
             CategoryName = c.Name,
             CategoryColor = c.Color,
             Spent = _db.Transactions
                 .Where(t => t.CategoryId == c.Id && t.PeriodId == periodId && t.Type == "expense")
                 .Sum(t => (decimal?)t.Amount) ?? 0m,
         }).ToListAsync();

    public async Task<Budget> UpsertAsync(UpsertBudgetRequest request)
    {
        var results = await _db.Budgets.FromSqlInterpolated($@"
            INSERT INTO budgets (period_id, category_id, amount, percentage)
            VALUES ({request.PeriodId}, {request.CategoryId}, {request.Amount}, {request.Percentage})
            ON CONFLICT (period_id, category_id)
            DO UPDATE SET amount = {request.Amount}, percentage = {request.Percentage}
            RETURNING *
        ").ToListAsync();
        return results[0];
    }

    public async Task DeleteAsync(Guid id)
    {
        var budget = await _db.Budgets.FirstOrDefaultAsync(b => b.Id == id);
        if (budget == null) return;
        _db.Budgets.Remove(budget);
        await _db.SaveChangesAsync();
    }
}
