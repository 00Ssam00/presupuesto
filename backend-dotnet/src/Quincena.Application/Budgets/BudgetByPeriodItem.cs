namespace Quincena.Application.Budgets;

// Port del SELECT de getBudgetsByPeriod en budgetsController.ts: una fila por
// CADA categoría (LEFT JOIN budgets, sin filtrar is_active — igual que Node),
// con el gasto ya calculado. Id es null cuando la categoría todavía no tiene
// presupuesto asignado para esa quincena.
public class BudgetByPeriodItem
{
    public Guid? Id { get; set; }
    public Guid PeriodId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public decimal? Percentage { get; set; }
    public string CategoryName { get; set; } = null!;
    public string CategoryColor { get; set; } = null!;
    public decimal Spent { get; set; }
}
