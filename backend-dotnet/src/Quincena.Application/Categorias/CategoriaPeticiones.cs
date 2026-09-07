namespace Quincena.Application.Categorias;

public class CrearCategoriaPeticion
{
    public string Nombre { get; set; } = null!;
    public string? Color { get; set; }
    public string? Icono { get; set; }
    public int? Orden { get; set; }
}

// name/color siempre se sobrescriben (igual que Node, sin COALESCE); icon/is_active/
// sort_order se preservan si no vienen en el body — ver ServicioCategorias.ActualizarAsync.
public class ActualizarCategoriaPeticion
{
    public string Nombre { get; set; } = null!;
    public string Color { get; set; } = null!;
    public string? Icono { get; set; }
    public bool? EstaActivo { get; set; }
    public int? Orden { get; set; }
}
