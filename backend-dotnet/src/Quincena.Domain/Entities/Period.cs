namespace Quincena.Domain.Entities;

public class Period
{
    public Guid Id { get; set; }
    public string Label { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal BaseIncome { get; set; }
    public decimal Rollover { get; set; }
    public bool IsClosed { get; set; }
    public DateTime CreatedAt { get; set; }
}
