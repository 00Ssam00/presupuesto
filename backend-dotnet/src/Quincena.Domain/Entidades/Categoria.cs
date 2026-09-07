namespace Quincena.Domain.Entidades;

public class Categoria
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Color { get; set; } = "#01696f";
    public string? Icono { get; set; }
    public bool EstaActivo { get; set; } = true;
    public int Orden { get; set; }
    public DateTime CreadoEn { get; set; }
}
