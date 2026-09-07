namespace Quincena.Application.Panel;

// Port de backend/src/controllers/dashboardController.ts. Único endpoint de la app
// donde available/remaining/total_expenses/total_income_extra salen como number
// JSON plano en vez de string — ver el comentario en DashboardModels.cs.
public class ServicioPanel : IServicioPanel
{
    private readonly IRepositorioPanel _repository;

    public ServicioPanel(IRepositorioPanel repository)
    {
        _repository = repository;
    }

    public async Task<ResultadoPanel?> ObtenerPanelAsync(Guid periodId)
    {
        var period = await _repository.ObtenerPeriodoAsync(periodId);
        if (period == null) return null;

        var (totalExpenses, totalIncomeExtra) = await _repository.ObtenerTotalesAsync(periodId);
        var expensesByCategory = await _repository.ObtenerGastosPorCategoriaAsync(periodId);
        var upcomingPayments = await _repository.ObtenerPagosProximosAsync(periodId);

        var available = (double)period.IngresoBase + (double)period.Arrastre + (double)totalIncomeExtra;
        var remaining = available - (double)totalExpenses;

        return new ResultadoPanel
        {
            Periodo = period,
            Disponible = available,
            TotalGastos = (double)totalExpenses,
            TotalIngresoExtra = (double)totalIncomeExtra,
            Restante = remaining,
            GastosPorCategoria = expensesByCategory,
            PagosProximos = upcomingPayments,
        };
    }
}
