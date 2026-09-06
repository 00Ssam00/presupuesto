using Microsoft.Extensions.DependencyInjection;
using Quincena.Application.Cards;
using Quincena.Application.Categories;
using Quincena.Application.PaymentMethods;
using Quincena.Application.Periods;
using Quincena.Application.Rollover;

namespace Quincena.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRolloverService, RolloverService>();
        services.AddScoped<IPeriodService, PeriodService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICardService, CardService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        return services;
    }
}
