using Quincena.Domain.Entities;

namespace Quincena.Application.Periods;

public interface IPeriodService
{
    Task<List<Period>> ListAsync();
    Task<Period?> GetCurrentAsync();
    Task<Period?> GetByIdAsync(Guid id);
    Task<Period> CreateAsync(CreatePeriodRequest request);
    Task<Period?> UpdateAsync(Guid id, UpdatePeriodRequest request);
    Task<bool> DeleteAsync(Guid id);
}
