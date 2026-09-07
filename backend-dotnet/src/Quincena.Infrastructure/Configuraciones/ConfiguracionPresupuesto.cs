using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Configuraciones;

public class ConfiguracionPresupuesto : IEntityTypeConfiguration<Presupuesto>
{
    public void Configure(EntityTypeBuilder<Presupuesto> builder)
    {
        builder.ToTable("budgets");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(b => b.PeriodoId).HasColumnName("period_id").IsRequired();
        builder.Property(b => b.CategoriaId).HasColumnName("category_id").IsRequired();
        builder.Property(b => b.Monto).HasColumnName("amount").HasColumnType("numeric(12,2)").HasDefaultValue(0m);
        builder.Property(b => b.Porcentaje).HasColumnName("percentage").HasColumnType("numeric(5,2)");
        builder.Property(b => b.CreadoEn).HasColumnName("created_at").HasDefaultValueSql("NOW()");

        builder.HasIndex(b => new { b.PeriodoId, b.CategoriaId }).IsUnique();
        builder.HasIndex(b => b.PeriodoId).HasDatabaseName("idx_budgets_period");

        builder.HasOne<Periodo>().WithMany().HasForeignKey(b => b.PeriodoId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Categoria>().WithMany().HasForeignKey(b => b.CategoriaId).OnDelete(DeleteBehavior.Cascade);
    }
}
