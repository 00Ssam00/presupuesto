using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Configuraciones;

public class ConfiguracionPeriodo : IEntityTypeConfiguration<Periodo>
{
    public void Configure(EntityTypeBuilder<Periodo> builder)
    {
        builder.ToTable("periods", t => t.HasCheckConstraint("periods_dates_check", "end_date > start_date"));
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.Etiqueta).HasColumnName("label").IsRequired();
        builder.Property(p => p.FechaInicio).HasColumnName("start_date").HasColumnType("date").IsRequired();
        builder.Property(p => p.FechaFin).HasColumnName("end_date").HasColumnType("date").IsRequired();
        builder.Property(p => p.IngresoBase).HasColumnName("base_income").HasColumnType("numeric(12,2)").HasDefaultValue(0m);
        builder.Property(p => p.Arrastre).HasColumnName("rollover").HasColumnType("numeric(12,2)").HasDefaultValue(0m);
        builder.Property(p => p.EstaCerrado).HasColumnName("is_closed").HasDefaultValue(false);
        builder.Property(p => p.CreadoEn).HasColumnName("created_at").HasDefaultValueSql("NOW()");
    }
}
