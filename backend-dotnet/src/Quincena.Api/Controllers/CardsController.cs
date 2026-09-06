using Microsoft.AspNetCore.Mvc;
using Quincena.Application.Cards;

namespace Quincena.Api.Controllers;

// Port de backend/src/controllers/cardsController.ts.
[ApiController]
[Route("api/cards")]
public class CardsController : ControllerBase
{
    private readonly ICardService _service;

    public CardsController(ICardService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List() => Ok(await _service.ListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCardRequest request)
    {
        var created = await _service.CreateAsync(request);
        return StatusCode(201, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCardRequest request)
    {
        var updated = await _service.UpdateAsync(id, request);
        if (updated == null) return NotFound(new { error = "No encontrada" });
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
