using Microsoft.Extensions.DependencyInjection;
using Quincena.Application.Periods;
using Quincena.Application.Rollover;

namespace Quincena.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRolloverService, RolloverService>();
        services.AddScoped<IPeriodService, PeriodService>();
        return services;
    }
}
