namespace Quincena.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid PeriodId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public Guid? CardId { get; set; }
    public string Type { get; set; } = null!; // "income" | "expense"
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public string? Note { get; set; }
    public DateOnly Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
