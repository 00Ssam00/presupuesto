using Microsoft.AspNetCore.Mvc;
using Quincena.Application.Presupuestos;

namespace Quincena.Api.Controladores;

// Port de backend/src/controllers/budgetsController.ts.
[ApiController]
[Route("api/presupuestos")]
public class ControladorPresupuestos : ControllerBase
{
    private readonly IServicioPresupuestos _service;

    public ControladorPresupuestos(IServicioPresupuestos service)
    {
        _service = service;
    }

    [HttpGet("periodo/{periodId:guid}")]
    public async Task<IActionResult> GetByPeriod(Guid periodId) => Ok(await _service.ObtenerPorPeriodoAsync(periodId));

    [HttpPost]
    public async Task<IActionResult> Upsert([FromBody] GuardarPresupuestoPeticion request)
    {
        var upserted = await _service.GuardarAsync(request);
        return StatusCode(201, upserted);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.EliminarAsync(id);
        return NoContent();
    }
}
