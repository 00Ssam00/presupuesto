namespace Quincena.Application.Rollover;

public interface IRolloverDataSource
{
    // Equivalente a la query agregada de backend/src/services/rollover.ts:
    // SUM de expense/income por período, ya ordenado por start_date ASC.
    Task<List<PeriodRolloverRow>> GetPeriodTotalsAsync();

    Task UpdateRolloversAsync(IReadOnlyList<RolloverUpdate> updates);
}
