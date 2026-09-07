namespace Quincena.Application.Transacciones;

// Port de las interfaces FilaImportacion/ResultadoFilaImportacion de transactionsController.ts.
public class FilaImportacion
{
    public string? Monto { get; set; }
    public string? Tipo { get; set; }
    public string? Fecha { get; set; }
    public string? Categoria { get; set; }
    public string? Metodo { get; set; }
    public string? Tarjeta { get; set; }
    public string? Descripcion { get; set; }
}

public class ResultadoFilaImportacion
{
    public int Fila { get; set; }
    public string Estado { get; set; } = null!; // "created" | "error"
    public Guid? Id { get; set; }
    public string? Error { get; set; }
}

public class ResultadoImportacion
{
    public int Creados { get; set; }
    public int Fallidos { get; set; }
    public List<ResultadoFilaImportacion> Resultados { get; set; } = [];
}
