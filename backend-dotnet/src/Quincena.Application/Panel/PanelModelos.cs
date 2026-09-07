using Quincena.Domain.Entidades;

namespace Quincena.Application.Panel;

public class GastoPorCategoriaItem
{
    public Guid CategoriaId { get; set; }
    public string NombreCategoria { get; set; } = null!;
    public string ColorCategoria { get; set; } = null!;
    public decimal Total { get; set; }
    public decimal Presupuesto { get; set; }
}

// Port de `up.*` + category_name/category_color en dashboardController.ts.
public class PagoProximoConCategoria
{
    public Guid Id { get; set; }
    public Guid? PeriodoId { get; set; }
    public Guid CategoriaId { get; set; }
    public string Nombre { get; set; } = null!;
    public decimal Monto { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public string Estado { get; set; } = null!;
    public bool EsRecurrente { get; set; }
    public string? IntervaloRecurrencia { get; set; }
    public DateTime CreadoEn { get; set; }
    public string NombreCategoria { get; set; } = null!;
    public string ColorCategoria { get; set; } = null!;
}

// `available`/`total_expenses`/`total_income_extra`/`remaining` son `double`, no
// `decimal`: dashboardController.ts los calcula con `Number(...)` de JS, así que
// salen como JSON number plano (sin comillas), a diferencia de todos los demás
// campos NUMERIC de la app. Usar `double` acá replica ese mismo comportamiento
// (mismo IEEE-754 de doble precisión que `Number()`) sin necesitar un converter
// de JSON a nivel de propiedad, que rompería la regla de capas (Application no
// puede depender de Api).
public class ResultadoPanel
{
    public Periodo Periodo { get; set; } = null!;
    public double Disponible { get; set; }
    public double TotalGastos { get; set; }
    public double TotalIngresoExtra { get; set; }
    public double Restante { get; set; }
    public List<GastoPorCategoriaItem> GastosPorCategoria { get; set; } = [];
    public List<PagoProximoConCategoria> PagosProximos { get; set; } = [];
}
