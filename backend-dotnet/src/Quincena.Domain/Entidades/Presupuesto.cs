namespace Quincena.Domain.Entidades;

public class Presupuesto
{
    public Guid Id { get; set; }
    public Guid PeriodoId { get; set; }
    public Guid CategoriaId { get; set; }
    public decimal Monto { get; set; }
    public decimal? Porcentaje { get; set; }
    public DateTime CreadoEn { get; set; }
}
