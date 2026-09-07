namespace Quincena.Application.Metas;

public class CrearMetaPeticion
{
    public string Nombre { get; set; } = null!;
    public string Tipo { get; set; } = null!;
    public decimal MontoObjetivo { get; set; }
    public decimal? MontoActual { get; set; }
    public DateOnly? FechaLimite { get; set; }
}

// Todos los campos se sobrescriben siempre (igual que Node, sin COALESCE),
// salvo EstaActivo: se preserva si no viene en el body — ver ServicioMetas.ActualizarAsync.
public class ActualizarMetaPeticion
{
    public string Nombre { get; set; } = null!;
    public string Tipo { get; set; } = null!;
    public decimal MontoObjetivo { get; set; }
    public decimal MontoActual { get; set; }
    public DateOnly? FechaLimite { get; set; }
    public bool? EstaActivo { get; set; }
}
