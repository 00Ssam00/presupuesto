namespace Quincena.Application.PagosProximos;

public class CrearPagoProximoPeticion
{
    public Guid? PeriodoId { get; set; }
    public Guid CategoriaId { get; set; }
    public string Nombre { get; set; } = null!;
    public decimal Monto { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public bool? EsRecurrente { get; set; }
    public string? IntervaloRecurrencia { get; set; }
}

// Todos los campos se sobrescriben siempre (igual que Node, sin COALESCE) —
// a diferencia de Metas, acá ni siquiera EstaActivo/Estado se preserva.
public class ActualizarPagoProximoPeticion
{
    public Guid CategoriaId { get; set; }
    public string Nombre { get; set; } = null!;
    public decimal Monto { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public string Estado { get; set; } = null!;
    public bool EsRecurrente { get; set; }
    public string? IntervaloRecurrencia { get; set; }
}
