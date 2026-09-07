namespace Quincena.Application.Periodos;

public class CrearPeriodoPeticion
{
    public string Etiqueta { get; set; } = null!;
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public decimal? IngresoBase { get; set; }
    public decimal? Arrastre { get; set; }
}

// Sin COALESCE: igual que el Node actual, se asume que el frontend siempre manda
// todos los campos. No confundir con categories/cards/goals, que sí preservan
// campos ausentes.
public class ActualizarPeriodoPeticion
{
    public string Etiqueta { get; set; } = null!;
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public decimal IngresoBase { get; set; }
    public decimal Arrastre { get; set; }
    public bool EstaCerrado { get; set; }
}
