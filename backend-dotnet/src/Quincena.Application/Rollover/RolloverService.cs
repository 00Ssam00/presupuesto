namespace Quincena.Application.Rollover;

public interface IRolloverService
{
    Task RecalculateAllAsync();
}

// Port exacto de backend/src/services/rollover.ts::recalculateAllRollovers.
// El primer período cronológico SIEMPRE tiene rollover esperado 0, sin importar su
// base_income o sus transacciones. Si ningún período difiere de su rollover esperado,
// no se ejecuta ningún UPDATE (misma optimización que el original). Los updates se
// aplican de forma independiente y no transaccional — deuda técnica ya existente en
// Node que se replica deliberadamente, no se corrige en esta migración.
public class RolloverService : IRolloverService
{
    private readonly IRolloverDataSource _dataSource;

    public RolloverService(IRolloverDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task RecalculateAllAsync()
    {
        var rows = await _dataSource.GetPeriodTotalsAsync();
        var carry = 0m;
        var updates = new List<RolloverUpdate>();

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            // Math.round(carry * 100) / 100 en JS == redondeo "hacia afuera de cero" en los
            // casos .xx5 exactos. El MidpointRounding.ToEven por defecto de C# divergiría
            // silenciosamente en esos casos borde, así que se fuerza AwayFromZero.
            var expected = i == 0 ? 0m : Math.Round(carry, 2, MidpointRounding.AwayFromZero);

            if (row.Rollover != expected)
            {
                updates.Add(new RolloverUpdate(row.Id, expected));
            }

            var available = row.BaseIncome + expected + row.TotalIncomeExtra;
            carry = available - row.TotalExpenses;
        }

        if (updates.Count == 0) return;
        await _dataSource.UpdateRolloversAsync(updates);
    }
}
