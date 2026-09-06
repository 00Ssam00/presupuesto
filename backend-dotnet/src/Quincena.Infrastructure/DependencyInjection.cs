using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quincena.Application.Budgets;
using Quincena.Application.Cards;
using Quincena.Application.Categories;
using Quincena.Application.Dashboard;
using Quincena.Application.PaymentMethods;
using Quincena.Application.Periods;
using Quincena.Application.Rollover;
using Quincena.Application.Transactions;
using Quincena.Infrastructure.Repositories;

namespace Quincena.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<QuincenaDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IPeriodRepository, PeriodRepository>();
        services.AddScoped<IRolloverDataSource, RolloverDataSource>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        return services;
    }
}
