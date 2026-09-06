using Quincena.Domain.Entities;

namespace Quincena.Application.Dashboard;

public class ExpenseByCategoryItem
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string CategoryColor { get; set; } = null!;
    public decimal Total { get; set; }
    public decimal Budget { get; set; }
}

// Port de `up.*` + category_name/category_color en dashboardController.ts.
public class UpcomingPaymentWithCategory
{
    public Guid Id { get; set; }
    public Guid? PeriodId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = null!;
    public bool IsRecurring { get; set; }
    public string? RecurrenceInterval { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CategoryName { get; set; } = null!;
    public string CategoryColor { get; set; } = null!;
}

// `available`/`total_expenses`/`total_income_extra`/`remaining` son `double`, no
// `decimal`: dashboardController.ts los calcula con `Number(...)` de JS, así que
// salen como JSON number plano (sin comillas), a diferencia de todos los demás
// campos NUMERIC de la app. Usar `double` acá replica ese mismo comportamiento
// (mismo IEEE-754 de doble precisión que `Number()`) sin necesitar un converter
// de JSON a nivel de propiedad, que rompería la regla de capas (Application no
// puede depender de Api).
public class DashboardResult
{
    public Period Period { get; set; } = null!;
    public double Available { get; set; }
    public double TotalExpenses { get; set; }
    public double TotalIncomeExtra { get; set; }
    public double Remaining { get; set; }
    public List<ExpenseByCategoryItem> ExpensesByCategory { get; set; } = [];
    public List<UpcomingPaymentWithCategory> UpcomingPayments { get; set; } = [];
}
