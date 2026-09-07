namespace Quincena.Application.Transacciones;

public class CrearTransaccionPeticion
{
    public Guid PeriodoId { get; set; }
    public Guid CategoriaId { get; set; }
    public Guid MetodoPagoId { get; set; }
    public Guid? TarjetaId { get; set; }
    public string Tipo { get; set; } = null!;
    public decimal Monto { get; set; }
    public string Descripcion { get; set; } = null!;
    public string? Nota { get; set; }
    public DateOnly Fecha { get; set; }
}

// Sin period_id: igual que Node, el PUT no permite mover una transacción de
// quincena (solo POST la fija).
public class ActualizarTransaccionPeticion
{
    public Guid CategoriaId { get; set; }
    public Guid MetodoPagoId { get; set; }
    public Guid? TarjetaId { get; set; }
    public string Tipo { get; set; } = null!;
    public decimal Monto { get; set; }
    public string Descripcion { get; set; } = null!;
    public string? Nota { get; set; }
    public DateOnly Fecha { get; set; }
}
