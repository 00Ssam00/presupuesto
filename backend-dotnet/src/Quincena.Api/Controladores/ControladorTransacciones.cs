using Microsoft.AspNetCore.Mvc;
using Quincena.Application.Transacciones;

namespace Quincena.Api.Controladores;

// Port de backend/src/controllers/transactionsController.ts.
[ApiController]
[Route("api/transacciones")]
public class ControladorTransacciones : ControllerBase
{
    private readonly IServicioTransacciones _service;

    public ControladorTransacciones(IServicioTransacciones service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery(Name = "periodo_id")] Guid? periodId,
        [FromQuery(Name = "tipo")] string? type,
        [FromQuery(Name = "categoria_id")] Guid? categoryId)
        => Ok(await _service.ListarAsync(periodId, type, categoryId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var transaction = await _service.ObtenerPorIdAsync(id);
        if (transaction == null) return NotFound(new { error = "No encontrada" });
        return Ok(transaction);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearTransaccionPeticion request)
    {
        var created = await _service.CrearAsync(request);
        return StatusCode(201, created);
    }

    // Ruta literal "import" antes que "{id:guid}" — ASP.NET Core la prioriza
    // automáticamente igual que con "current" en ControladorPeriodos.
    [HttpPost("importar")]
    public async Task<IActionResult> Import([FromBody] ImportarPeticion? request)
    {
        if (request?.Filas == null || request.Filas.Count == 0)
            return BadRequest(new { error = "Se requiere al menos una fila" });

        var result = await _service.ImportarAsync(request.Filas);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ActualizarTransaccionPeticion request)
    {
        var updated = await _service.ActualizarAsync(id, request);
        if (updated == null) return NotFound(new { error = "No encontrada" });
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.EliminarAsync(id);
        return NoContent();
    }
}

public class ImportarPeticion
{
    public List<FilaImportacion>? Filas { get; set; }
}
