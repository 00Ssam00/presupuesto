namespace Quincena.Domain.Entities;

public class PaymentMethod
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public bool RequiresCard { get; set; }
    public bool IsActive { get; set; } = true;
}
