namespace Quincena.Application.Budgets;

public class UpsertBudgetRequest
{
    public Guid PeriodId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public decimal? Percentage { get; set; }
}
