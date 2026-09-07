namespace Quincena.Application.PagosProximos;

// Igual forma que Panel.PagoProximoConCategoria — se duplica en vez de
// compartir entre módulos, siguiendo el mismo criterio de independencia por
// recurso que ya usan Presupuestos/Panel.
public class PagoProximoListaItem
{
    public Guid Id { get; set; }
    public Guid? PeriodoId { get; set; }
    public Guid CategoriaId { get; set; }
    public string Nombre { get; set; } = null!;
    public decimal Monto { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public string Estado { get; set; } = null!;
    public bool EsRecurrente { get; set; }
    public string? IntervaloRecurrencia { get; set; }
    public DateTime CreadoEn { get; set; }
    public string NombreCategoria { get; set; } = null!;
    public string ColorCategoria { get; set; } = null!;
}
