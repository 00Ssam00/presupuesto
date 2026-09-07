namespace Quincena.Domain.Entidades;

public class Periodo
{
    public Guid Id { get; set; }
    public string Etiqueta { get; set; } = null!;
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public decimal IngresoBase { get; set; }
    public decimal Arrastre { get; set; }
    public bool EstaCerrado { get; set; }
    public DateTime CreadoEn { get; set; }
}
