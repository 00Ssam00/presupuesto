using Microsoft.EntityFrameworkCore;
using Quincena.Domain.Entidades;
using Quincena.Infrastructure.Configuraciones;

namespace Quincena.Infrastructure;

public class QuincenaDbContext : DbContext
{
    public QuincenaDbContext(DbContextOptions<QuincenaDbContext> options) : base(options) { }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<MetodoPago> MetodosPago => Set<MetodoPago>();
    public DbSet<Tarjeta> Tarjetas => Set<Tarjeta>();
    public DbSet<Periodo> Periodos => Set<Periodo>();
    public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();
    public DbSet<Transaccion> Transacciones => Set<Transaccion>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<PagoProximo> PagosProximos => Set<PagoProximo>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.ApplyConfiguration(new ConfiguracionCategoria());
        modelBuilder.ApplyConfiguration(new ConfiguracionMetodoPago());
        modelBuilder.ApplyConfiguration(new ConfiguracionTarjeta());
        modelBuilder.ApplyConfiguration(new ConfiguracionPeriodo());
        modelBuilder.ApplyConfiguration(new ConfiguracionPresupuesto());
        modelBuilder.ApplyConfiguration(new ConfiguracionTransaccion());
        modelBuilder.ApplyConfiguration(new GoalConfiguration());
        modelBuilder.ApplyConfiguration(new ConfiguracionPagoProximo());
        modelBuilder.ApplyConfiguration(new AppSettingConfiguration());
    }
}
