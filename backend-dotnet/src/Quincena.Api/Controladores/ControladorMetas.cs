using Microsoft.AspNetCore.Mvc;
using Quincena.Application.Metas;

namespace Quincena.Api.Controladores;

// Port de backend/src/controllers/goalsController.ts.
[ApiController]
[Route("api/metas")]
public class ControladorMetas : ControllerBase
{
    private readonly IServicioMetas _service;

    public ControladorMetas(IServicioMetas service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List() => Ok(await _service.ListarAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearMetaPeticion request)
    {
        var created = await _service.CrearAsync(request);
        return StatusCode(201, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ActualizarMetaPeticion request)
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
