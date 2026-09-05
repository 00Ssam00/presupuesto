namespace Quincena.Domain.Entities;

public class Card
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? LastFour { get; set; }
    public string? Bank { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
