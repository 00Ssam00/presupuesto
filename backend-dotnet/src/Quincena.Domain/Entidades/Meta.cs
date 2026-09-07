namespace Quincena.Domain.Entidades;

public class Meta
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Tipo { get; set; } = null!; // "savings" | "investment"
    public decimal MontoObjetivo { get; set; }
    public decimal MontoActual { get; set; }
    public DateOnly? FechaLimite { get; set; }
    public bool EstaActivo { get; set; } = true;
    public DateTime CreadoEn { get; set; }
    public DateTime ActualizadoEn { get; set; }
}
