namespace Quincena.Domain.Entidades;

public class MetodoPago
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = null!;
    public bool RequiereTarjeta { get; set; }
    public bool EstaActivo { get; set; } = true;
}
