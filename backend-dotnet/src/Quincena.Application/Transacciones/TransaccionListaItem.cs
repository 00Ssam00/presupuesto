namespace Quincena.Application.Transacciones;

// Port del SELECT_WITH_JOINS de transactionsController.ts (t.* + category_name/
// category_color/payment_method_name/card_name). El orden de propiedades importa:
// define el orden de las claves en el JSON de salida, igual que Postgres devuelve
// t.* primero y los alias de los JOIN después.
public class TransaccionListaItem
{
    public Guid Id { get; set; }
    public Guid PeriodoId { get; set; }
    public Guid CategoriaId { get; set; }
    public Guid MetodoPagoId { get; set; }
    public Guid? TarjetaId { get; set; }
    public string Tipo { get; set; } = null!;
    public decimal Monto { get; set; }
    public string Descripcion { get; set; } = null!;
    public string? Nota { get; set; }
    public DateOnly Fecha { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime ActualizadoEn { get; set; }
    public string NombreCategoria { get; set; } = null!;
    public string ColorCategoria { get; set; } = null!;
    public string NombreMetodoPago { get; set; } = null!;
    public string? NombreTarjeta { get; set; }
}
