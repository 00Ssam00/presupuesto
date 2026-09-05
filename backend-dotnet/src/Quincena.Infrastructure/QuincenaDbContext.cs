using Microsoft.EntityFrameworkCore;
using Quincena.Domain.Entities;
using Quincena.Infrastructure.Configurations;

namespace Quincena.Infrastructure;

public class QuincenaDbContext : DbContext
{
    public QuincenaDbContext(DbContextOptions<QuincenaDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Period> Periods => Set<Period>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<UpcomingPayment> UpcomingPayments => Set<UpcomingPayment>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentMethodConfiguration());
        modelBuilder.ApplyConfiguration(new CardConfiguration());
        modelBuilder.ApplyConfiguration(new PeriodConfiguration());
        modelBuilder.ApplyConfiguration(new BudgetConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        modelBuilder.ApplyConfiguration(new GoalConfiguration());
        modelBuilder.ApplyConfiguration(new UpcomingPaymentConfiguration());
        modelBuilder.ApplyConfiguration(new AppSettingConfiguration());
    }
}
