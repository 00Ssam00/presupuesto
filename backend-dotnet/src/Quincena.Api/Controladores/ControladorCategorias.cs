using Microsoft.AspNetCore.Mvc;
using Quincena.Application.Categorias;

namespace Quincena.Api.Controladores;

// Port de backend/src/controllers/categoriesController.ts.
[ApiController]
[Route("api/categorias")]
public class ControladorCategorias : ControllerBase
{
    private readonly IServicioCategorias _service;

    public ControladorCategorias(IServicioCategorias service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List() => Ok(await _service.ListarAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearCategoriaPeticion request)
    {
        var created = await _service.CrearAsync(request);
        return StatusCode(201, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ActualizarCategoriaPeticion request)
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
