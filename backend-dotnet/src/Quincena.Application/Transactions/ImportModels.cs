namespace Quincena.Application.Transactions;

// Port de las interfaces ImportRow/ImportRowResult de transactionsController.ts.
public class ImportRow
{
    public string? Monto { get; set; }
    public string? Tipo { get; set; }
    public string? Fecha { get; set; }
    public string? Categoria { get; set; }
    public string? Metodo { get; set; }
    public string? Tarjeta { get; set; }
    public string? Descripcion { get; set; }
}

public class ImportRowResult
{
    public int Row { get; set; }
    public string Status { get; set; } = null!; // "created" | "error"
    public Guid? Id { get; set; }
    public string? Error { get; set; }
}

public class ImportResult
{
    public int Created { get; set; }
    public int Failed { get; set; }
    public List<ImportRowResult> Results { get; set; } = [];
}
