using Microsoft.AspNetCore.Mvc;
using Quincena.Application.Periodos;

namespace Quincena.Api.Controladores;

// Port de backend/src/controllers/periodsController.ts.
[ApiController]
[Route("api/periodos")]
public class ControladorPeriodos : ControllerBase
{
    private readonly IServicioPeriodos _service;

    public ControladorPeriodos(IServicioPeriodos service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List() => Ok(await _service.ListarAsync());

    // Ruta literal "current" — en ASP.NET Core los segmentos literales siempre tienen
    // prioridad sobre los parametrizados, así que no puede ser interceptada por
    // GetById({id:guid}) sin importar el orden de declaración (a diferencia de Express,
    // donde Node depende del orden de registro de rutas). Nunca 404: si no hay ningún
    // período que cubra hoy, cae al más reciente; si no hay ninguno, responde null.
    [HttpGet("actual")]
    public async Task<IActionResult> Current()
    {
        // OkObjectResult(null) devuelve un body vacío en vez de "null" con [ApiController]
        // (una particularidad conocida de MVC). JsonResult sí serializa null tal cual,
        // igual que `res.json(null)` en Node.
        return new JsonResult(await _service.ObtenerActualAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var period = await _service.ObtenerPorIdAsync(id);
        if (period == null) return NotFound(new { error = "No encontrado" });
        return Ok(period);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearPeriodoPeticion request)
    {
        var created = await _service.CrearAsync(request);
        return StatusCode(201, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ActualizarPeriodoPeticion request)
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
