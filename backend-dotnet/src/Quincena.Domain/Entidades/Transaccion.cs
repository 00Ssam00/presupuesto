namespace Quincena.Domain.Entidades;

public class Transaccion
{
    public Guid Id { get; set; }
    public Guid PeriodoId { get; set; }
    public Guid CategoriaId { get; set; }
    public Guid MetodoPagoId { get; set; }
    public Guid? TarjetaId { get; set; }
    public string Tipo { get; set; } = null!; // "income" | "expense"
    public decimal Monto { get; set; }
    public string Descripcion { get; set; } = null!;
    public string? Nota { get; set; }
    public DateOnly Fecha { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime ActualizadoEn { get; set; }
}
