using Microsoft.Extensions.DependencyInjection;
using Quincena.Application.Cards;
using Quincena.Application.Categories;
using Quincena.Application.PaymentMethods;
using Quincena.Application.Periods;
using Quincena.Application.Rollover;
using Quincena.Application.Transactions;

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
        services.AddScoped<ITransactionService, TransactionService>();
        return services;
    }
}
