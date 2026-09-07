namespace Quincena.Application.Presupuestos;

public class GuardarPresupuestoPeticion
{
    public Guid PeriodoId { get; set; }
    public Guid CategoriaId { get; set; }
    public decimal Monto { get; set; }
    public decimal? Porcentaje { get; set; }
}
