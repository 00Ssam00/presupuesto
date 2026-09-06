namespace Quincena.Application.Periods;

public class CreatePeriodRequest
{
    public string Label { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal? BaseIncome { get; set; }
    public decimal? Rollover { get; set; }
}

// Sin COALESCE: igual que el Node actual, se asume que el frontend siempre manda
// todos los campos. No confundir con categories/cards/goals, que sí preservan
// campos ausentes.
public class UpdatePeriodRequest
{
    public string Label { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal BaseIncome { get; set; }
    public decimal Rollover { get; set; }
    public bool IsClosed { get; set; }
}
