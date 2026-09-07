namespace Quincena.Domain.Entidades;

public class Goal
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Tipo { get; set; } = null!; // "savings" | "investment"
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateOnly? Deadline { get; set; }
    public bool EstaActivo { get; set; } = true;
    public DateTime CreadoEn { get; set; }
    public DateTime ActualizadoEn { get; set; }
}
