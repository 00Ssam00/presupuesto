using Quincena.Application.Rollover;
using Quincena.Domain.Entities;

namespace Quincena.Application.Periods;

// Port de backend/src/controllers/periodsController.ts. Tras cada mutación que afecta
// el rollover (crear/editar/eliminar), se recalcula y se relee el registro fresco,
// igual que el original.
public class PeriodService : IPeriodService
{
    private readonly IPeriodRepository _repository;
    private readonly IRolloverService _rolloverService;

    public PeriodService(IPeriodRepository repository, IRolloverService rolloverService)
    {
        _repository = repository;
        _rolloverService = rolloverService;
    }

    public Task<List<Period>> ListAsync() => _repository.GetAllOrderByStartDateDescAsync();

    public Task<Period?> GetCurrentAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return _repository.GetCoveringOrLatestAsync(today);
    }

    public Task<Period?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);

    public async Task<Period> CreateAsync(CreatePeriodRequest request)
    {
        var period = new Period
        {
            Label = request.Label,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            BaseIncome = request.BaseIncome ?? 0m,
            Rollover = request.Rollover ?? 0m,
        };

        var created = await _repository.AddAsync(period);
        await _rolloverService.RecalculateAllAsync();
        return (await _repository.GetByIdAsync(created.Id))!;
    }

    public async Task<Period?> UpdateAsync(Guid id, UpdatePeriodRequest request)
    {
        var found = await _repository.UpdateAsync(id, period =>
        {
            period.Label = request.Label;
            period.StartDate = request.StartDate;
            period.EndDate = request.EndDate;
            period.BaseIncome = request.BaseIncome;
            period.Rollover = request.Rollover;
            period.IsClosed = request.IsClosed;
        });

        if (!found) return null;

        await _rolloverService.RecalculateAllAsync();
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        // Igual que Node: no valida existencia previa, siempre recalcula.
        await _repository.DeleteAsync(id);
        await _rolloverService.RecalculateAllAsync();
        return true;
    }
}
