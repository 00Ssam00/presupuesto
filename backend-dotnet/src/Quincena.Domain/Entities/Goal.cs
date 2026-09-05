namespace Quincena.Domain.Entities;

public class Goal
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!; // "savings" | "investment"
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateOnly? Deadline { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
