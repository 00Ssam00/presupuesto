using Microsoft.EntityFrameworkCore;
using Quincena.Application.Periods;
using Quincena.Domain.Entities;

namespace Quincena.Infrastructure.Repositories;

public class PeriodRepository : IPeriodRepository
{
    private readonly QuincenaDbContext _db;

    public PeriodRepository(QuincenaDbContext db)
    {
        _db = db;
    }

    public Task<List<Period>> GetAllOrderByStartDateDescAsync() =>
        _db.Periods.OrderByDescending(p => p.StartDate).ToListAsync();

    public Task<Period?> GetByIdAsync(Guid id) =>
        _db.Periods.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Period?> GetCoveringOrLatestAsync(DateOnly today)
    {
        var covering = await _db.Periods
            .FirstOrDefaultAsync(p => p.StartDate <= today && p.EndDate >= today);
        if (covering != null) return covering;

        return await _db.Periods
            .OrderByDescending(p => p.StartDate)
            .FirstOrDefaultAsync();
    }

    public async Task<Period> AddAsync(Period period)
    {
        _db.Periods.Add(period);
        await _db.SaveChangesAsync();
        return period;
    }

    public async Task<bool> UpdateAsync(Guid id, Action<Period> apply)
    {
        var period = await _db.Periods.FirstOrDefaultAsync(p => p.Id == id);
        if (period == null) return false;
        apply(period);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var period = await _db.Periods.FirstOrDefaultAsync(p => p.Id == id);
        if (period == null) return false;
        _db.Periods.Remove(period);
        await _db.SaveChangesAsync();
        return true;
    }
}
