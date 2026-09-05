namespace Quincena.Domain.Entities;

public class Budget
{
    public Guid Id { get; set; }
    public Guid PeriodId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public decimal? Percentage { get; set; }
    public DateTime CreatedAt { get; set; }
}
