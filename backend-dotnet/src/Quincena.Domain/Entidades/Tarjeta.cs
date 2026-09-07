namespace Quincena.Domain.Entidades;

public class Tarjeta
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? UltimosCuatro { get; set; }
    public string? Banco { get; set; }
    public bool EstaActivo { get; set; } = true;
    public DateTime CreadoEn { get; set; }
}
