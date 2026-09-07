using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quincena.Domain.Entidades;

namespace Quincena.Infrastructure.Configuraciones;

public class ConfiguracionAjuste : IEntityTypeConfiguration<Ajuste>
{
    public void Configure(EntityTypeBuilder<Ajuste> builder)
    {
        builder.ToTable("app_settings");
        builder.HasKey(s => s.Clave);
        builder.Property(s => s.Clave).HasColumnName("key");
        builder.Property(s => s.Valor).HasColumnName("value").IsRequired();
        builder.Property(s => s.ActualizadoEn).HasColumnName("updated_at").HasDefaultValueSql("NOW()");
    }
}
