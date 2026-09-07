namespace Quincena.Application.Arrastre;

public interface IFuenteDatosArrastre
{
    // Equivalente a la query agregada de backend/src/services/rollover.ts:
    // SUM de expense/income por período, ya ordenado por start_date ASC.
    Task<List<FilaArrastrePeriodo>> ObtenerTotalesPeriodosAsync();

    Task ActualizarArrastresAsync(IReadOnlyList<ActualizacionArrastre> updates);
}
