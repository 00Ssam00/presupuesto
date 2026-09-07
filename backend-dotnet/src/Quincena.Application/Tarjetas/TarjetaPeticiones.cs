namespace Quincena.Application.Tarjetas;

public class CrearTarjetaPeticion
{
    public string Nombre { get; set; } = null!;
    public string? UltimosCuatro { get; set; }
    public string? Banco { get; set; }
}

// name/last_four/bank siempre se sobrescriben (igual que Node, sin COALESCE —
// omitir last_four/bank en un PUT los deja en null); is_active se preserva si
// no viene en el body — ver ServicioTarjetas.ActualizarAsync.
public class ActualizarTarjetaPeticion
{
    public string Nombre { get; set; } = null!;
    public string? UltimosCuatro { get; set; }
    public string? Banco { get; set; }
    public bool? EstaActivo { get; set; }
}
