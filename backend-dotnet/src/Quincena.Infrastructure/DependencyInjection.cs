using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quincena.Application.Periods;
using Quincena.Application.Rollover;
using Quincena.Infrastructure.Repositories;

namespace Quincena.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<QuincenaDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IPeriodRepository, PeriodRepository>();
        services.AddScoped<IRolloverDataSource, RolloverDataSource>();
        return services;
    }
}
