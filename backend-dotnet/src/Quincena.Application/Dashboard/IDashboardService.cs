namespace Quincena.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardResult?> GetAsync(Guid periodId);
}
