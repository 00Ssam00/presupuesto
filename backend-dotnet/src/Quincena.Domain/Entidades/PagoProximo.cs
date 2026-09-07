namespace Quincena.Domain.Entidades;

public class PagoProximo
{
    public Guid Id { get; set; }
    public Guid? PeriodoId { get; set; }
    public Guid CategoriaId { get; set; }
    public string Nombre { get; set; } = null!;
    public decimal Monto { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public string Estado { get; set; } = "pending"; // "pending" | "paid" | "overdue"
    public bool EsRecurrente { get; set; }
    public string? IntervaloRecurrencia { get; set; } // "weekly" | "biweekly" | "monthly"
    public DateTime CreadoEn { get; set; }
}
