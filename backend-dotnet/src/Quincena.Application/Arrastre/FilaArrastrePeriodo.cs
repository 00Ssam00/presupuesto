namespace Quincena.Application.Arrastre;

public record FilaArrastrePeriodo(
    Guid Id,
    DateOnly FechaInicio,
    decimal IngresoBase,
    decimal Arrastre,
    decimal TotalGastos,
    decimal TotalIngresoExtra);

public record ActualizacionArrastre(Guid PeriodoId, decimal Arrastre);
