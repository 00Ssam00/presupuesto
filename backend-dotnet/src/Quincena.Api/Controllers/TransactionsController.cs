using Microsoft.AspNetCore.Mvc;
using Quincena.Application.Transactions;

namespace Quincena.Api.Controllers;

// Port de backend/src/controllers/transactionsController.ts.
[ApiController]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _service;

    public TransactionsController(ITransactionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery(Name = "period_id")] Guid? periodId,
        [FromQuery(Name = "type")] string? type,
        [FromQuery(Name = "category_id")] Guid? categoryId)
        => Ok(await _service.ListAsync(periodId, type, categoryId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var transaction = await _service.GetByIdAsync(id);
        if (transaction == null) return NotFound(new { error = "No encontrada" });
        return Ok(transaction);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
    {
        var created = await _service.CreateAsync(request);
        return StatusCode(201, created);
    }

    // Ruta literal "import" antes que "{id:guid}" — ASP.NET Core la prioriza
    // automáticamente igual que con "current" en PeriodsController.
    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] ImportRequest? request)
    {
        if (request?.Rows == null || request.Rows.Count == 0)
            return BadRequest(new { error = "Se requiere al menos una fila" });

        var result = await _service.ImportAsync(request.Rows);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionRequest request)
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

public class ImportRequest
{
    public List<ImportRow>? Rows { get; set; }
}
