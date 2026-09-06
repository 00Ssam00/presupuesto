using Microsoft.AspNetCore.Mvc;
using Quincena.Application.Budgets;

namespace Quincena.Api.Controllers;

// Port de backend/src/controllers/budgetsController.ts.
[ApiController]
[Route("api/budgets")]
public class BudgetsController : ControllerBase
{
    private readonly IBudgetService _service;

    public BudgetsController(IBudgetService service)
    {
        _service = service;
    }

    [HttpGet("period/{periodId:guid}")]
    public async Task<IActionResult> GetByPeriod(Guid periodId) => Ok(await _service.GetByPeriodAsync(periodId));

    [HttpPost]
    public async Task<IActionResult> Upsert([FromBody] UpsertBudgetRequest request)
    {
        var upserted = await _service.UpsertAsync(request);
        return StatusCode(201, upserted);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
