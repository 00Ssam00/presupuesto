namespace Quincena.Application.Presupuestos;

// Port del SELECT de getBudgetsByPeriod en budgetsController.ts: una fila por
// CADA categoría (LEFT JOIN budgets, sin filtrar is_active — igual que Node),
// con el gasto ya calculado. Id es null cuando la categoría todavía no tiene
// presupuesto asignado para esa quincena.
public class PresupuestoPorPeriodoItem
{
    public Guid? Id { get; set; }
    public Guid PeriodoId { get; set; }
    public Guid CategoriaId { get; set; }
    public decimal Monto { get; set; }
    public decimal? Porcentaje { get; set; }
    public string NombreCategoria { get; set; } = null!;
    public string ColorCategoria { get; set; } = null!;
    public decimal Gastado { get; set; }
}
