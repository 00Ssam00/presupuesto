using Microsoft.AspNetCore.Mvc;
using Quincena.Application.PagosProximos;

namespace Quincena.Api.Controladores;

// Port de backend/src/controllers/upcomingPaymentsController.ts.
[ApiController]
[Route("api/pagos-proximos")]
public class ControladorPagosProximos : ControllerBase
{
    private readonly IServicioPagosProximos _service;

    public ControladorPagosProximos(IServicioPagosProximos service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery(Name = "periodo_id")] Guid? periodId,
        [FromQuery(Name = "estado")] string? status)
        => Ok(await _service.ListarAsync(periodId, status));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearPagoProximoPeticion request)
    {
        var created = await _service.CrearAsync(request);
        return StatusCode(201, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ActualizarPagoProximoPeticion request)
    {
        var updated = await _service.ActualizarAsync(id, request);
        if (updated == null) return NotFound(new { error = "No encontrado" });
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.EliminarAsync(id);
        return NoContent();
    }
}
