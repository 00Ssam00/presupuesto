namespace Quincena.Domain.Entities;

public class UpcomingPayment
{
    public Guid Id { get; set; }
    public Guid? PeriodId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = "pending"; // "pending" | "paid" | "overdue"
    public bool IsRecurring { get; set; }
    public string? RecurrenceInterval { get; set; } // "weekly" | "biweekly" | "monthly"
    public DateTime CreatedAt { get; set; }
}
